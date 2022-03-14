"use strict"

const dotenv = require("dotenv");
const Discord = require("discord.js");
const { Pool } = require("pg");
const { validateEnvironmentVariables } = require("./utils/env");
const { calculatePoints, counter } = require("./utils/counter");
const { parseTopCountDescription, parseUsername } = require("./utils/parser");
const { greet, agree, disagree, notUnderstood } = require("./utils/message");
const { getAccessToken, getUserByOsuId } = require("./utils/osu");
const { OsuUserStatus } = require("./utils/common");
const { DatabaseErrors, insertUser } = require("./utils/db");

dotenv.config();

const pool = new Pool({
  host: process.env.DB_HOST,
  port: parseInt(process.env.DB_PORT, 10),
  user: process.env.DB_USERNAME,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE
});

const client = new Discord.Client({ intents: [ "GUILDS", "GUILD_MESSAGES" ]});

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

  if(msg.channelId === process.env.CHANNEL_ID) {
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

      const username = parseUsername(title);

      // [ top_1, top_8, top_15, top_25, top_50 ]
      const topCounts = parseTopCountDescription(desc);
      const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
      const draft = counter(
        topCounts[0],
        topCounts[1],
        topCounts[2],
        topCounts[3],
        topCounts[4],
        username
      );

      const sentMessage = await channel.send({ embeds: [ draft ] });

      if(typeof(process.env.OSUHOW_EMOJI_ID) === "string") {
        if(points.toString().includes("727")) {
          const emoji = client.emojis.cache.get(process.env.OSUHOW_EMOJI_ID);
          sentMessage.react(emoji);
        }
      }

      console.log("[LOG] Calculating points for username: " + username);
    }
    else {
      const mentionedUsers = msg.mentions.users;
      const isClientMentioned = mentionedUsers.has(client.user.id);

      const contents = msg.content.split(/\s+/g); // split by one or more spaces

      if(isClientMentioned && contents[0].includes(client.user.id)) {
        let reply = "";

        if(contents[1] === "link") {
          if(typeof(contents[2]) === "string") {
            const osuId = parseInt(contents[2], 10);

            if(osuId > 0) {
              const tempToken = await getToken();
  
              if(tempToken === 0) {
                reply = "**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?";
              }
              else {
                const response = await getUserByOsuId(tempToken, osuId);

                if(response.status === OsuUserStatus.BOT) {
                  reply = "**Error:** Unable to link ID: User type is Bot.";
                }
                else if(response.status === OsuUserStatus.NOT_FOUND) {
                  reply = "**Error:** Unable to link ID: User not found.";
                }
                else if(response.status === OsuUserStatus.DELETED) {
                  reply = "**Error:** Unable to link ID: User is deleted.";
                }
                else {
                  const discordId = msg.author.id;
                  const osuUsername = response.username;
                  const result = await insertUser(pool, discordId, osuId)

                  switch(result) {
                    case DatabaseErrors.OK: 
                      reply = "Linked Discord user <@" + discordId + "> to osu! user **" + osuUsername + "**.";
                      break;
                    case DatabaseErrors.CONNECTION_ERROR: {
                      reply = "**Error:** Unable to link ID: An error occurred with the database connection. Please contact bot administrator.";
                      break;
                    }
                    case DatabaseErrors.DUPLICATED: {
                      reply = "**Error:** Unable to link ID: osu! ID already linked to other Discord user.";
                    }
                    case DatabaseErrors.CLIENT_ERROR:
                    case DatabaseErrors.TYPE_ERROR: {
                      reply = "**Error:** Client error has occurred. Please contact bot administrator.";
                      break;
                    }
                    default: {
                      reply = "**Error**: Unknown return value. Please contact bot administrator.";
                    }
                  }
                }
              }
            }
            else {
              reply = "**Error:** ID must be in numbers. Open your osu! profile and copy ID from the last part of the example in the URL:\nhttps://osu.ppy.sh/users/2581664, then 2581664 is your ID.";
            }
          }
          else {
            reply = "You need to specify your osu! user ID: `@" + process.env.BOT_NAME + " link [osu! user ID]`";
          }
        }
        else if(contents[1] === "hi" || contents[1] === "hello") { // TODO: move elses and below to parent if (accept all channels)
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

if(!validateEnvironmentVariables()) {
  process.exit(0);
}

client.login(process.env.BOT_TOKEN);
