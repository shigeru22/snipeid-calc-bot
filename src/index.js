"use strict"

const dotenv = require("dotenv");
const Discord = require("discord.js");
const { Pool } = require("pg");
const { validateEnvironmentVariables } = require("./utils/env");
const { calculatePoints } = require("./utils/messages/counter");
const { parseTopCountDescription, parseUsername, parseOsuIdFromLink } = require("./utils/parser");
const { greet, agree, disagree, notUnderstood } = require("./utils/messages/msg");
const { getAccessToken } = require("./utils/api/osu");
const { countPoints } = require("./utils/commands/points");
const { addWysiReaction } = require("./utils/commands/reactions");
const { updateUserData, fetchUser, fetchOsuUser, fetchOsuStats, insertUserData } = require("./utils/commands/userdata");
const { addRole } = require("./utils/commands/roles");
const { sendPointLeaderboard } = require("./utils/commands/leaderboards");

dotenv.config();

const pool = new Pool({
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
    console.log("[LOG] Access token expired. Requesting new access token...");
    const response = await getAccessToken(process.env.OSU_CLIENT_ID, process.env.OSU_CLIENT_SECRET);

    if(Object.keys(response).length === 0) {
      console.log("[LOG] Unable to request access token. osu! site might be down?");
      return 0;
    }
    else {
      token = response.token;
      expired = response.expire;
    }
  }

  return token;
}

client.on("ready", async () => await onStartup());
client.on("messageCreate", async (msg) => await onNewMessage(msg));

async function onStartup() {
  console.log("[LOG] Requesting access token...");
  const response = await getAccessToken(process.env.OSU_CLIENT_ID, process.env.OSU_CLIENT_SECRET);
  
  if(Object.keys(response).length === 0) {
    console.log("[LOG] Unable to request access token. osu! API might be down?");
  }
  else {
    token = response.token;
    expired = response.expire;
  }

  client.user.setActivity("Bathbot everyday", { type: "WATCHING" });
  console.log("[LOG] " + process.env.BOT_NAME + " is now running.");
}

async function onNewMessage(msg) {
  const channel = await client.channels.cache.get(process.env.CHANNEL_ID);

  if(msg.channelId === process.env.LEADERBOARD_CHANNEL_ID) {
    const channel = await client.channels.cache.get(process.env.LEADERBOARD_CHANNEL_ID);

    const mentionedUsers = msg.mentions.users;
    const contents = msg.content.split(/\s+/g); // split by one or more spaces
    const isClientMentioned = mentionedUsers.has(client.user.id) && contents[0].includes(client.user.id);

    if(isClientMentioned) {
      if(contents[1] === "lb" || contents[1] === "leaderboard") {
        await sendPointLeaderboard(channel, pool);
      }
    }
  }
  else if(msg.channelId === process.env.CHANNEL_ID) {
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

      await updateUserData(tempToken, client, channel, pool, osuId, points);
    }
    else {
      const mentionedUsers = msg.mentions.users;
      const contents = msg.content.split(/\s+/g); // split by one or more spaces
      const isClientMentioned = mentionedUsers.has(client.user.id) && contents[0].includes(client.user.id);

      if(isClientMentioned) {
        if(contents[1] === "link") {
          if(typeof(contents[2]) !== "string") {
            await channel.send("You need to specify your osu! user ID: `@" + process.env.BOT_NAME + " link [osu! user ID]`");
            return;
          }

          const osuId = parseInt(contents[2], 10);

          if(isNaN(osuId)) {
            await channel.send("**Error:** ID must be in numbers. Open your osu! profile and copy ID from the last part of the example in the URL:\nhttps://osu.ppy.sh/users/2581664, then 2581664 is your ID.");
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

          const result = await insertUserData(channel, pool, msg.author.id, osuId, osuUser.username);
          if(!result) {
            return;
          }

          if(typeof(process.env.VERIFIED_ROLE_ID) !== "string" || process.env.VERIFIED_ROLE_ID === "") {
            console.log("[LOG] VERIFIED_ROLE_ID not set. Role granting skipped.");
            return;
          }

          await addRole(client, channel, msg.author.id, process.env.VERIFIED_ROLE_ID);
        }
        else if(contents[1] === "count") {
          await channel.send("Retrieving user top counts...");

          const user = await fetchUser(channel, pool, msg.author.id);
          if(!user) {
            return;
          }

          let tempToken = await getToken();
          if(tempToken === 0) {
            await channel.send("**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?");
            return;
          }

          const osuUser = await fetchOsuUser(channel, tempToken, user.osuId);
          if(!osuUser) {
            return;
          }
          
          const topCounts = await fetchOsuStats(channel, osuUser.username);
          if(!topCounts) {
            return;
          }

          const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
          const message = await countPoints(channel, osuUser.username, topCounts);
          await addWysiReaction(client, message, topCounts, points);

          tempToken = await getToken();
          if(tempToken === 0) {
            await channel.send("**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?");
            return;
          }

          await updateUserData(tempToken, client, channel, pool, user.osuId, points);
        }
        else {
          let reply = "";

          if(contents[1] === "hi" || contents[1] === "hello") { // TODO: move elses and below to parent if (accept all channels)
            reply = greet();
          }
          else if(contents[1].includes("right")) {
            const val = Math.random();
            if(val >= 0.5) {
              reply = agree();
            }
            else {
              reply = disagree();
            }
          }
          else {
            reply = notUnderstood();
          }
  
          await channel.send(reply);
        }
      }
    }
  }
}

if(!validateEnvironmentVariables()) {
  process.exit(0);
}

client.login(process.env.BOT_TOKEN);
