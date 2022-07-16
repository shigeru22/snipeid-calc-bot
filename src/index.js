"use strict";

const dotenv = require("dotenv");
const Discord = require("discord.js");
const { Pool } = require("pg");
const { createInterface } = require("readline");
const { validateEnvironmentVariables } = require("./utils/env");
const { LogSeverity, log } = require("./utils/log");
const { OsuToken } = require("./utils/osu-token");
const { handleVerificationChannelCommands, handlePointsChannelCommands, handleLeaderboardChannelCommands } = require("./utils/commands/main");
const { sendMessage } = require("./utils/commands/conversations");

// configure environment variable file (if any)
dotenv.config();

// database pool
const db = new Pool({
  host: process.env.DB_HOST,
  port: parseInt(process.env.DB_PORT, 10),
  user: process.env.DB_USERNAME,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE
});

// bot client
const client = new Discord.Client({ intents: [ "GUILDS", "GUILD_MESSAGES" ] });

// osu! API token object
const token = new OsuToken(process.env.OSU_CLIENT_ID, process.env.OSU_CLIENT_SECRET);

// handle Windows interrupt event
if(process.platform === "win32") {
  const rl = createInterface({
    input: process.stdin,
    output: process.stdout
  });

  rl.on("SIGINT", () => process.emit("SIGINT"));
}

// interrupt and termination signal handling
process.on("SIGINT", () => onExit());
process.on("SIGTERM", () => onExit());

// bot client event handling
client.on("ready", async () => await onStartup());
client.on("messageCreate", async (msg) => await onNewMessage(msg));

/**
 * Startup event function.
 */
async function onStartup() {
  await token.getToken();

  client.user.setActivity("Bathbot everyday", { type: "WATCHING" });
  log(LogSeverity.LOG, "onStartup", process.env.BOT_NAME + " is now running.");
}

/**
 * New message event function.
 *
 * @param { Discord.Message } msg
 */
async function onNewMessage(msg) {
  const contents = msg.content.split(/\s+/g); // split by one or more spaces
  const isClientMentioned = msg.mentions.users.has(client.user.id) && contents[0].includes(client.user.id);
  let processed = false;

  const channel = msg.channel;
  if(channel.type === "GUILD_TEXT") {
    const tempToken = await token.getToken();
    if(tempToken === "") {
      await channel.send("**Error:** Unable to retrieve osu! client authorizations. Check osu!status?");
      return;
    }

    switch(msg.channelId) {
      case process.env.VERIFICATION_CHANNEL_ID:
        processed = await handleVerificationChannelCommands(client, channel, db, tempToken, isClientMentioned, msg);
        break;
      case process.env.CHANNEL_ID:
        processed = await handlePointsChannelCommands(client, channel, db, tempToken, isClientMentioned, msg);
        break;
      case process.env.LEADERBOARD_CHANNEL_ID:
        processed = await handleLeaderboardChannelCommands(channel, db, isClientMentioned, msg);
        break;
    }

    // if bot is mentioned but nothing processed, send a random message.
    (!processed && isClientMentioned) && await sendMessage(channel, contents);
  }
}

/**
 * Exit event function.
 */
function onExit() {
  log(LogSeverity.LOG, "onExit", "Exit signal received. Cleaning up process...");
  client.destroy();
  log(LogSeverity.LOG, "onExit", "Cleanup success. Exiting...");

  process.exit(0);
}

// main execution procedure

if(!validateEnvironmentVariables()) {
  process.exit(0);
}

client.login(process.env.BOT_TOKEN);
