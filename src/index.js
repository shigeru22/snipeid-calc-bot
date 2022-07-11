"use strict";

const dotenv = require("dotenv");
const Discord = require("discord.js");
const { Pool } = require("pg");
const { validateEnvironmentVariables } = require("./utils/env");
const { LogSeverity, log } = require("./utils/log");
const { getAccessToken } = require("./utils/api/osu");
const { calculatePoints } = require("./utils/messages/counter");
const { parseTopCountDescription, parseUsername, parseOsuIdFromLink, parseWhatIfCount } = require("./utils/parser");
const { sendMessage } = require("./utils/commands/conversations");
const { userLeaderboardsCount, userWhatIfCount } = require("./utils/commands/count");
const { sendPointLeaderboard } = require("./utils/commands/leaderboards");
const { countPoints } = require("./utils/commands/points");
const { addWysiReaction } = require("./utils/commands/reactions");
const { addRole } = require("./utils/commands/roles");
const { updateUserData, fetchUser, fetchOsuUser, fetchOsuStats, insertUserData } = require("./utils/commands/userdata");

dotenv.config();

const db = new Pool({
  host: process.env.DB_HOST,
  port: parseInt(process.env.DB_PORT, 10),
  user: process.env.DB_USERNAME,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE
});

const client = new Discord.Client({ intents: [ "GUILDS", "GUILD_MESSAGES", ]});

const BATHBOT_USER_ID = "297073686916366336";

let token = "";
let expired = new Date(0);

async function getToken() {
  const now = new Date();
  if(now.getTime() >= expired.getTime()) {
    log(LogSeverity.LOG, "getToken", "Access token expired. Requesting new access token...");
    const response = await getAccessToken(process.env.OSU_CLIENT_ID, process.env.OSU_CLIENT_SECRET);

    if(Object.keys(response).length === 0) {
      log(LogSeverity.WARN, "getToken", "Unable to request access token. osu! site might be down?");
      return 0;
    }
    else {
      token = response.token;
      expired = response.expire;
    }
  }

  return token;
}

if (process.platform === "win32") {
  var rl = require("readline").createInterface({
    input: process.stdin,
    output: process.stdout
  });

  rl.on("SIGINT", function () {
    process.emit("SIGINT");
  });
}

process.on("SIGINT", async () => await onExit());
process.on("SIGTERM", async () => await onExit());

client.on("ready", async () => await onStartup());
client.on("messageCreate", async (msg) => await onNewMessage(msg));

async function onStartup() {
  log(LogSeverity.LOG, "onStartup", "Requesting access token...");
  const response = await getAccessToken(process.env.OSU_CLIENT_ID, process.env.OSU_CLIENT_SECRET);

  if(Object.keys(response).length === 0) {
    log(LogSeverity.WARN, "onStartup", "Unable to request access token. osu! API might be down?");
  }
  else {
    token = response.token;
    expired = response.expire;
  }

  client.user.setActivity("Bathbot everyday", { type: "WATCHING" });
  log(LogSeverity.LOG, "onStartup", process.env.BOT_NAME + " is now running.");
}

async function onNewMessage(msg) {
  const contents = msg.content.split(/\s+/g); // split by one or more spaces
  const isClientMentioned = msg.mentions.users.has(client.user.id) && contents[0].includes(client.user.id);;
  let processed = false;

  if(msg.channelId === process.env.LEADERBOARD_CHANNEL_ID) {
    const channel = client.channels.cache.get(process.env.LEADERBOARD_CHANNEL_ID);

    if(isClientMentioned) {
      if(contents[1] === "lb" || contents[1] === "leaderboard") {
        await sendPointLeaderboard(channel, db);
        processed = true;
      }
    }
  }
  else if(msg.channelId === process.env.CHANNEL_ID) {
    const channel = client.channels.cache.get(process.env.CHANNEL_ID);

    if(msg.author.id === BATHBOT_USER_ID) {
      const embeds = msg.embeds; // always 0
      const index = embeds.findIndex(
        embed => typeof(embed.title) === "string" && embed.title.toLowerCase().startsWith("in how many top x map leaderboards is")
      );

      if(index === -1) {
        return;
      }

      const title = embeds[index].title;
      const desc = embeds[index].description;
      const link = embeds[index].author.url;

      const username = parseUsername(title);
      const osuId = parseOsuIdFromLink(link);

      // [ top_1, top_8, top_15, top_25, top_50 ]
      const topCounts = parseTopCountDescription(desc);
      const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
      const message = await countPoints(channel, username, topCounts);
      await addWysiReaction(client, message, topCounts, points);

      const tempToken = await getToken();
      if(tempToken === 0) {
        await channel.send("**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?");
        return;
      }

      await updateUserData(tempToken, client, channel, db, osuId, points);
    }
    else {
      if(isClientMentioned) {
        const tempToken = await getToken();
        if(tempToken === 0) {
          await channel.send("**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?");
          return;
        }

        if(contents[1] === "count") {
          await userLeaderboardsCount(client, channel, db, tempToken, msg.author.id);
          processed = true;
        }
        else if(contents[1] === "whatif") {
          const commands = [...contents];
          commands.splice(0, 2); // remove first two elements

          const whatifs = [];
          {
            const len = commands.length;
            for(let i = 0; i < len; i++) {
              const temp = parseWhatIfCount(commands[i]);
              if(typeof(temp) === "number" && temp < 0) { // TODO: handle return codes
                await channel.send(`**Error:** Invalid what if expression${ len > 1 ? "s" : "" }.`);
                return;
              }

              whatifs.push(temp);
            }
          }

          const tops = [ 1, 8, 15, 25, 50 ]; // match bathbot <osc top ranks data

          let valid = true;
          whatifs.forEach(whatif => {
            if(!tops.includes(whatif[0])) {
              valid = false;
            }
          });

          if(!valid) {
            await channel.send("**Error:** Rank query must be 1, 8, 15, 25, or 50.");
            return;
          }

          await userWhatIfCount(client, channel, db, tempToken, msg.author.id, whatifs);
          processed = true;
        }
      }
    }
  }
  else if(msg.channelId === process.env.VERIFICATION_CHANNEL_ID) {
    const channel = client.channels.cache.get(process.env.VERIFICATION_CHANNEL_ID);

    if(contents[1] === "link") {
      if(typeof(contents[2]) !== "string") {
        await channel.send("You need to specify your osu! user ID: `@" + process.env.BOT_NAME + " link [osu! user ID]`");
        return;
      }

      const osuId = parseInt(contents[2], 10);

      if(isNaN(osuId)) {
        await channel.send("**Error:** ID must be in numbers.");
        return;
      }

      if(osuId <= 0) {
        await channel.send("**Error:** I see what you did there. That's funny.");
        return;
      }

      const tempToken = await getToken();

      if(tempToken === 0) {
        await channel.send("**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?");
        return;
      }

      const osuUser = await fetchOsuUser(channel, tempToken, osuId);
      if(!osuUser) {
        return;
      }

      if(!osuUser.isCountryCodeAllowed) {
        await channel.send("**Error:** Wrong country code from osu! profile. Please contact server moderators.");
        return;
      }

      const result = await insertUserData(channel, db, msg.author.id, osuId, osuUser.username);
      if(!result) {
        return;
      }

      if(typeof(process.env.VERIFIED_ROLE_ID) !== "string" || process.env.VERIFIED_ROLE_ID === "") {
        log(LogSeverity.LOG, "onNewMessage", "VERIFIED_ROLE_ID not set. Role granting skipped.");
        processed = true;
        return;
      }

      await addRole(client, channel, msg.author.id, process.env.VERIFIED_ROLE_ID);
      processed = true;
    }
  }

  if(!processed && isClientMentioned) {
    await sendMessage(client, msg.channelId, contents);
  }
}

async function onExit() {
  log(LogSeverity.LOG, "onExit", "Exit signal received. Cleaning up process...");
  client.destroy();
  log(LogSeverity.LOG, "onExit", "Cleanup success. Exiting...");

  process.exit(0);
}

if(!validateEnvironmentVariables()) {
  process.exit(0);
}

client.login(process.env.BOT_TOKEN);
