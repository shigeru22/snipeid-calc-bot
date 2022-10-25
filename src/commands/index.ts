import { Client, TextChannel, Message } from "discord.js";
import { Pool } from "pg";
import { sendPointLeaderboard } from "./leaderboards";
import { userLeaderboardsCountFromBathbot, userLeaderboardsCount, userWhatIfCount } from "./count";
import { verifyUser } from "./verification";

// Bathbot ID
const BATHBOT_USER_ID = "297073686916366336";

/**
 * Handles all commands in the verification channel.
 *
 * @param { Client } client - Discord bot client.
 * @param { TextChannel } channel - Discord channel to handle commands in.
 * @param { Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { boolean } isClientMentioned - Whether the bot was mentioned in the message.
 * @param { Message } message - Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handleVerificationChannelCommands(client: Client, channel: TextChannel, db: Pool, osuToken: string, isClientMentioned: boolean, message: Message): Promise<boolean> {
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
 * @param { Client } client - Discord bot client.
 * @param { TextChannel } channel - Discord channel to handle commands in.
 * @param { Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { boolean } isClientMentioned - Whether the bot was mentioned in the message.
 * @param { Message } message - Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handlePointsChannelCommands(client: Client, channel: TextChannel, db: Pool, osuToken: string, isClientMentioned: boolean, message: Message): Promise<boolean> {
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
 * @param { TextChannel } channel - Discord channel to handle commands in.
 * @param { Pool } db - Database connection pool.
 * @param { boolean } isClientMentioned - Whether the bot was mentioned in the message.
 * @param { Message } message - Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handleLeaderboardChannelCommands(channel: TextChannel, db: Pool, isClientMentioned: boolean, message: Message): Promise<boolean> {
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

export { handleVerificationChannelCommands, handlePointsChannelCommands, handleLeaderboardChannelCommands };
