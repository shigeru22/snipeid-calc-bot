import { Client, TextChannel, Message } from "discord.js";
import { Pool } from "pg";
import Config from "./config";
import Conversations from "./conversations";
import Count from "./count";
import Leaderboards from "./leaderboards";
import Roles from "./roles";
import Reactions from "./reactions";
import UserData from "./userdata";
import Verification from "./verification";

// Bathbot ID
const BATHBOT_USER_ID = "297073686916366336";

/**
 * Handles all commands.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Discord channel to handle commands in.
 * @param { Pool } db Database connection pool.
 * @param { string } osuToken osu! API token.
 * @param { boolean } isClientMentioned Whether the bot was mentioned in the message.
 * @param { Message } message Discord message object.
 *
 * @returns { Promise<boolean> } Whether the command was handled.
 */
async function handleCommands(client: Client, channel: TextChannel, db: Pool, osuToken: string, isClientMentioned: boolean, message: Message): Promise<boolean> {
  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let ret = false;

  if(message.author.id === BATHBOT_USER_ID) {
    await Count.userLeaderboardsCountFromBathbot(client, channel, db, osuToken, message);
    ret = true;
  }
  else if(isClientMentioned) {
    await channel.sendTyping();

    switch(contents[1]) {
      case "link":
        await Verification.verifyUser(client, channel, db, osuToken, message);
        ret = true;
        break;
      case "count":
        await Count.userLeaderboardsCount(client, channel, db, osuToken, message.author.id);
        ret = true;
        break;
      case "whatif":
        await Count.userWhatIfCount(client, channel, db, osuToken, message);
        ret = true;
        break;
      case "lb": // fallthrough
      case "leaderboard":
        await Leaderboards.sendPointLeaderboard(channel, db);
        ret = true;
        break;
      case "config":
        await Config.handleConfigCommands(client, channel, db, message);
        ret = true;
        break;
    }
  }

  return ret;
}

export { handleCommands, Config, Conversations, Count, Leaderboards, Reactions, Roles, UserData, Verification };
