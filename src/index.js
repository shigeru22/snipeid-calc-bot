"use strict"

const dotenv = require("dotenv");
const Discord = require("discord.js");
const counter = require("./utils/counter.js");

dotenv.config();
const client = new Discord.Client({ intents: [ "GUILDS", "GUILD_MESSAGES" ]});

client.on("ready", async () => await onStartup());

async function onStartup() {
  console.log("SnipeID is now running.");

  const channel = await client.channels.cache.get(process.env.CHANNEL_ID);
  // await channel.send("Hello, world!");

  counter(727, 727, 727, 727, 727);
}

client.login(process.env.BOT_TOKEN);
