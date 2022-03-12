"use strict"

const dotenv = require("dotenv");
const Discord = require("discord.js");
const { validateEnvironmentVariables } = require("./utils/env");
const { calculatePoints, counter } = require("./utils/counter");
const { parseTopCountDescription, parseUsername } = require("./utils/parser");
const { greet, agree, disagree, notUnderstood } = require("./utils/message");

dotenv.config();
const client = new Discord.Client({ intents: [ "GUILDS", "GUILD_MESSAGES" ]});

const BATHBOT_USER_ID = "297073686916366336";

client.on("ready", async () => await onStartup());
client.on("messageCreate", async (msg) => await onNewMessage(msg))

async function onStartup() {
  client.user.setActivity("Bathbot everyday", { type: "WATCHING" });
  console.log(process.env.BOT_NAME + " is now running.");
}

async function onNewMessage(msg) {
  const channel = await client.channels.cache.get(process.env.CHANNEL_ID);

  if(msg.channelId === process.env.CHANNEL_ID) {
    if(msg.author.id === BATHBOT_USER_ID) {
      // await channel.send("Calculating score...")
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
            reply = "Processing ID: " + parseInt(contents[2], 10);
          }
          else {
            reply = "You need to specify your osu! user ID: `@" + process.env.BOT_NAME + " link [osu! user ID]`";
          }
        }
        else if(contents[1] === "hi" || contents[1] === "hello") {
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
