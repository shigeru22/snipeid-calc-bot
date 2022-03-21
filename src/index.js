"use strict"

const dotenv = require("dotenv");
const Discord = require("discord.js");
const { Pool } = require("pg");
const { validateEnvironmentVariables } = require("./utils/env");
const { calculatePoints, counter } = require("./utils/messages/counter");
const { parseTopCountDescription, parseUsername, parseOsuIdFromLink } = require("./utils/parser");
const { greet, agree, disagree, notUnderstood } = require("./utils/messages/msg");
const { getAccessToken, getUserByOsuId } = require("./utils/api/osu");
const { OsuUserStatus, DatabaseErrors, AssignmentType, AssignmentSort } = require("./utils/common");
const { deltaTimeToString } = require("./utils/time");
const { insertUser } = require("./utils/db/users");
const { getAllAssignments, getLastAssignmentUpdate, insertOrUpdateAssignment } = require("./utils/db/assignments");
const { createLeaderboardEmbed } = require("./utils/messages/leaderboard");

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
    const isClientMentioned = mentionedUsers.has(client.user.id);

    const contents = msg.content.split(/\s+/g); // split by one or more spaces

    if(isClientMentioned && contents[0].includes(client.user.id)) {
      if(contents[1] === "lb" || contents[1] === "leaderboard") {
        const rankings = await getAllAssignments(pool, AssignmentSort.POINTS, true);
        const lastUpdated = new Date(await getLastAssignmentUpdate(pool));
        const draft = createLeaderboardEmbed(rankings, lastUpdated);

        await channel.send({ embeds: [ draft ] });
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

      console.log("[LOG] Calculating points for username: " + username);

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

      const tempToken = await getToken();
      if(tempToken === 0) {
        await channel.send("**Error:** Unable to retrieve osu! client authorizations. Maybe the API is down?");
        return;
      }

      const response = await getUserByOsuId(tempToken, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10)); // TODO: handle deleted user

      const assignmentResult = await insertOrUpdateAssignment(pool, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10), points, response.username);
      if(typeof(assignmentResult) === "number") {
        switch(assignmentResult) {
          case DatabaseErrors.USER_NOT_FOUND: break;
          default:
            await channel.send("**Error:** An error occurred while updating your points data. Please contact bot administrator.");
        }
      }
      else {
        const today = new Date();

        switch(assignmentResult.type) {
          case AssignmentType.INSERT:
            await channel.send(
              "<@" + assignmentResult.discordId + "> achieved " +
              "**" + assignmentResult.delta + "** points. Go for those leaderboards!"
            );
            break;
          case AssignmentType.UPDATE:
            await channel.send(
              "<@" + assignmentResult.discordId + "> have " +
              (assignmentResult.delta >= 0 ? "gained" : "lost") +
              " **" + assignmentResult.delta + "** points " + 
              "since " + deltaTimeToString(today.getTime() - assignmentResult.lastUpdate.getTime()) +
              " ago."
            );
            break;
        }

        try {
          const server = await client.guilds.fetch(process.env.SERVER_ID);
          let updated = false;

          switch(assignmentResult.type) {
            case AssignmentType.UPDATE:
              if(assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId) {
                const oldRole = await server.roles.fetch(assignmentResult.role.oldRoleId);
                (await server.members.fetch(assignmentResult.discordId)).roles.remove(oldRole);
                updated = true;
              } // use fallthrough
            case AssignmentType.INSERT:
              if(
                assignmentResult.type === AssignmentType.INSERT ||
                (assignmentResult.type === AssignmentType.UPDATE &&
                  assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId)
              ) {
                const newRole = await server.roles.fetch(assignmentResult.role.newRoleId);
                (await server.members.fetch(assignmentResult.discordId)).roles.add(newRole);
                updated = true;
              }
              break;
          }

          if(updated) {
            await channel.send(
              "You have been " + (assignmentResult.delta > 0 ? "promoted" : "demoted") +
              " to **" + assignmentResult.role.newRoleName + "** role. " +
              (assignmentResult.delta > 0 ? "Awesome!" : "Fight back at those leaderboards!")
            );
          }
        }
        catch (e) {
          if(e instanceof Error) {
            console.log("[ERROR] onNewMessage :: " + e.name + ": " + e.message + "\n" + e.stack);
          }
          else {
            console.log("[ERROR] onNewMessage :: Unknown error occurred.");
          }

          reply = "**Error:** Unable to assign your role. Please contact bot administrator.";
        }
      }

      // if(assignmentResult.delta >= 0) {
      //   if(assignmentResult.roleId !== 0) {
      //     await channel.send("")
      //   }
      // }
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
                  const result = await insertUser(pool, discordId, osuId, osuUsername);

                  switch(result) {
                    case DatabaseErrors.OK: 
                      reply = "Linked Discord user <@" + discordId + "> to osu! user **" + osuUsername + "**.";
                      break;
                    case DatabaseErrors.CONNECTION_ERROR: {
                      reply = "**Error:** Unable to link ID: An error occurred with the database connection. Please contact bot administrator.";
                      break;
                    }
                    case DatabaseErrors.DUPLICATED_DISCORD_ID: {
                      reply = "**Error:** Unable to link ID: You already linked your osu! ID. Please contact server moderators to make changes.";
                      break;
                    }
                    case DatabaseErrors.DUPLICATED_OSU_ID: {
                      reply = "**Error:** Unable to link ID: osu! ID already linked to other Discord user.";
                      break;
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

                  if(result === DatabaseErrors.OK) {
                    // assign role if available
                    if(typeof(process.env.VERIFIED_ROLE_ID) === "string" && process.env.VERIFIED_ROLE_ID !== "") {
                      try {
                        const server = await client.guilds.fetch(process.env.SERVER_ID);
                        const role = await server.roles.fetch(process.env.VERIFIED_ROLE_ID);

                        (await server.members.fetch(discordId)).roles.add(role);
                      }
                      catch (e) {
                        if(e instanceof Error) {
                          console.log("[ERROR] onNewMessage :: " + e.name + ": " + e.message + "\n" + e.stack);
                        }
                        else {
                          console.log("[ERROR] onNewMessage :: Unknown error occurred.");
                        }

                        reply = "**Error:** Unable to assign your role. Please contact bot administrator.";
                      }
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
