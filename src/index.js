"use strict"

const dotenv = require("dotenv");
const Discord = require("discord.js");
const counter = require("./utils/counter.js");
const { parseTopCountDescription, parseUsername } = require("./utils/parser.js");

dotenv.config();
const client = new Discord.Client({ intents: [ "GUILDS", "GUILD_MESSAGES" ]});

const BATHBOT_USER_ID = "297073686916366336";

client.on("ready", async () => await onStartup());
client.on("messageCreate", async (msg) => await onNewMessage(msg))

async function onStartup() {
  console.log("SnipeID is now running.");
}

async function onNewMessage(msg) {
  const channel = await client.channels.cache.get(process.env.CHANNEL_ID);

  if(msg.channelId === process.env.CHANNEL_ID) {
    if(msg.author.id === BATHBOT_USER_ID) {
      // await channel.send("Calculating score...")
      const embeds = msg.embeds; // always 0
      const len = embeds.length;
      const index = embeds.findIndex(
        embed => (embed.title !== undefined || embed.title !== null) && embed.title.toLowerCase().startsWith("in how many top x map leaderboards is")
      );

      if(index === -1) {
        return;
      }

      const title = embeds[index].title;
      const desc = embeds[index].description;

      const username = parseUsername(title);

      // [ top_1, top_8, top_15, top_25, top_50 ]
      const topCounts = parseTopCountDescription(desc);

      const draft = counter(
        topCounts[0],
        topCounts[1],
        topCounts[2],
        topCounts[3],
        topCounts[4],
        username
      );

      await channel.send({ embeds: [ draft ] });

      console.log("[LOG] Calculating points for username: " + username);
    }
  }
}

client.login(process.env.BOT_TOKEN);
