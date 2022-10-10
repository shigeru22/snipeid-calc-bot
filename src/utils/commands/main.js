const { sendPointLeaderboard } = require("./leaderboards");
const { userLeaderboardsCountFromBathbot, userLeaderboardsCount, userWhatIfCount } = require("./count");
const { verifyUser } = require("./verification");

// Bathbot ID
const BATHBOT_USER_ID = "297073686916366336";

/**
 * Handles all commands in the verification channel.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Discord channel to handle commands in.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { boolean } isClientMentioned - Whether the bot was mentioned in the message.
 * @param { import("discord.js").Message } message - Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handleVerificationChannelCommands(client, channel, db, osuToken, isClientMentioned, message) {
  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let ret = false;

  if(isClientMentioned) {
    await channel.sendTyping();

    switch(contents[1]) {
      case "link":
        await verifyUser(client, channel, db, osuToken, message);
        ret = true;
        break;
    }
  }

  return ret;
}

/**
 * Handles all commands in the the points channel.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Discord channel to handle commands in.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { boolean } isClientMentioned - Whether the bot was mentioned in the message.
 * @param { import("discord.js").Message } message - Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handlePointsChannelCommands(client, channel, db, osuToken, isClientMentioned, message) {
  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let ret = false;

  if(message.author.id === BATHBOT_USER_ID) {
    await userLeaderboardsCountFromBathbot(client, channel, db, osuToken, message);
    ret = true;
  }
  else if(isClientMentioned) {
    await channel.sendTyping();

    switch(contents[1]) {
      case "count":
        await userLeaderboardsCount(client, channel, db, osuToken, message.author.id);
        ret = true;
        break;
      case "whatif":
        await userWhatIfCount(client, channel, db, osuToken, message);
        ret = true;
        break;
    }
  }

  return ret;
}

/**
 * Handles all commands in the leaderboards channel.
 *
 * @param { import("discord.js").TextChannel } channel - Discord channel to handle commands in.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { boolean } isClientMentioned - Whether the bot was mentioned in the message.
 * @param { import("discord.js").Message } message - Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handleLeaderboardChannelCommands(channel, db, isClientMentioned, message) {
  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let ret = false;

  if(isClientMentioned) {
    await channel.sendTyping();

    switch(contents[1]) {
      case "lb": // fallthrough
      case "leaderboard":
        await sendPointLeaderboard(channel, db);
        ret = true;
        break;
    }
  }

  return ret;
}

module.exports = {
  handleVerificationChannelCommands,
  handlePointsChannelCommands,
  handleLeaderboardChannelCommands
};
