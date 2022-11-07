import { Client, TextChannel, Message } from "discord.js";
import { Pool } from "pg";
import { getUserByOsuId } from "../api/osu";
import { getTopCounts, getTopCountsFromRespektive } from "../api/osustats";
import { getServerByDiscordId } from "../db/servers";
import { getDiscordUserByDiscordId } from "../db/users";
import { addWysiReaction } from "./reactions";
import { updateUserData } from "./userdata";
import { calculatePoints, calculateRespektivePoints, counter, counterRespektive } from "../messages/counter";
import { OsuUserStatus, OsuStatsSuccessStatus, OsuStatsErrorStatus, DatabaseErrors, DatabaseSuccess, OsuApiSuccessStatus, OsuApiErrorStatus } from "../utils/common";
import { WhatIfParserStatus, parseUsername, parseOsuIdFromLink, parseTopCountDescription, parseWhatIfCount } from "../utils/parser";
import { LogSeverity, log } from "../utils/log";

// <osc, using Bathbot message response
/**
 * Sends calculated points from Bathbot `<osc` command.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Channel to send points result to.
 * @param { Pool } db Database connection pool.
 * @param { string } osuToken osu! API token.
 * @param { Message } message Message that triggered the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userLeaderboardsCountFromBathbot(client: Client, channel: TextChannel, db: import("pg").Pool, osuToken: string, message: Message): Promise<void> {
  const index = message.embeds.findIndex(
    embed => typeof(embed.title) === "string" && embed.title.toLowerCase().startsWith("in how many top x map leaderboards is")
  ); // <osc command should return at index 0, else it's not the specified command

  if(index === -1) {
    return;
  }

  let title: string;
  let desc: string;
  let link: string;

  {
    const tempTitle = message.embeds[index].title;
    const tempDesc = message.embeds[index].description;
    const author = message.embeds[index].author;

    if(tempTitle === null || tempDesc === null) {
      return;
    }

    if(author === null || author.url === undefined) {
      return;
    }

    title = tempTitle;
    desc = tempDesc;
    link = author.url;
  }

  const username = parseUsername(title);
  const osuId = parseOsuIdFromLink(link);

  // [ top_1, top_8, top_15, top_25, top_50 ]
  const topCounts = parseTopCountDescription(desc);
  const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
  await countPoints(client, channel, username, topCounts);

  await updateUserData(osuToken, client, channel, db, osuId, points);
}

// @[BOT_NAME] count
/**
 * Sends top leaderboard count to specified channel.
 * Basically, this is Bathbot's `<osc` command.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Channel to send points result to.
 * @param { Pool } db Database connection pool.
 * @param { string } osuToken osu! API token.
 * @param { string } discordId Discord ID of the user who sent the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userLeaderboardsCount(client: Client, channel: TextChannel, db: Pool, osuToken: string, discordId: string): Promise<void> {
  const user = await getDiscordUserByDiscordId(db, discordId);

  if(user.status !== DatabaseSuccess.OK) {
    const serverData = await getServerByDiscordId(db, channel.id);

    if(serverData.status !== DatabaseSuccess.OK) {
      log(LogSeverity.WARN, "userLeaderboardsCount", "Someone asked for leaderboard count, but not in server.");
      return;
    }

    switch(user.status) {
      case DatabaseErrors.USER_NOT_FOUND:
        await channel.send(`**Error:** You haven't linked your account. Link using \`${ client.user?.username } [osu! user ID]\`${ serverData.data.verifyChannelId !== null ? ` in <#${ serverData.data.verifyChannelId }> channel` : "" }.`);
        break;
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error:** Database connection error occurred. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  const osuUser = await getUserByOsuId(osuToken, user.data.osuId);
  {
    if(osuUser.status !== OsuApiSuccessStatus.OK) {
      switch(osuUser.status) {
        case OsuApiErrorStatus.NON_OK:
          await channel.send("**Error:** osu! user not found.");
          break;
        case OsuApiErrorStatus.CLIENT_ERROR:
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          break;
      }

      return;
    }

    if(osuUser.data.status !== OsuUserStatus.USER || osuUser.data.user === undefined) { // TODO: use conditional type for user
      switch(osuUser.data.status) {
        case OsuUserStatus.BOT:
          await channel.send("**Error:** Suddenly, you turned into a skynet...");
          break;
        case OsuUserStatus.DELETED: // fallthrough
        case OsuUserStatus.NOT_FOUND:
          await channel.send("**Error:** Did you do something to your osu! account?");
          break;
      }

      return;
    }
  }

  const osuUsername = osuUser.data.user.userName;
  const useRespektive = typeof(process.env.USE_RESPEKTIVE) === "string" && process.env.USE_RESPEKTIVE === "1";

  const topCounts: number[] = [];
  if(!useRespektive) { // TODO: refactor for userWhatIfCount function usage
    {
      const topCountsRequest = [
        getTopCounts(osuUsername, 1),
        getTopCounts(osuUsername, 8),
        getTopCounts(osuUsername, 15),
        getTopCounts(osuUsername, 25),
        getTopCounts(osuUsername, 50)
      ];

      const temp = await Promise.all(topCountsRequest);
      {
        let error = OsuStatsErrorStatus.OK;
        const len = temp.length;
        for(let i = 0; i < len; i++) {
          const tempCountResponse = temp[i];

          if(tempCountResponse.status !== OsuStatsSuccessStatus.OK) {
            error = tempCountResponse.status;
            break;
          }

          topCounts.push(tempCountResponse.data.count);
        }

        if(error !== OsuStatsErrorStatus.OK) {
          switch(error) {
            case OsuStatsErrorStatus.USER_NOT_FOUND:
              await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
              break;
            case OsuStatsErrorStatus.API_ERROR:
              await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
              break;
            case OsuStatsErrorStatus.CLIENT_ERROR:
              await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
              break;
          }

          return;
        }
      }
    }
  }
  else {
    const temp = await getTopCountsFromRespektive(user.data.osuId);
    if(temp.status !== OsuStatsSuccessStatus.OK) {
      switch(temp.status) {
        case OsuStatsErrorStatus.USER_NOT_FOUND:
          await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
          break;
        case OsuStatsErrorStatus.API_ERROR:
          await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
          break;
        case OsuStatsErrorStatus.CLIENT_ERROR:
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          break;
      }

      return;
    }
  }

  let points = 0;
  if(!useRespektive) {
    points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
    await countPoints(client, channel, osuUsername, topCounts);
  }
  else {
    points = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
    await countRespektivePoints(client, channel, osuUsername, topCounts);
  }

  await updateUserData(osuToken, client, channel, db, user.data.osuId, points);
}

// @[BOT_NAME] whatif [what-if expression]
/**
 * Sends user's points in the specified what-if situation.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Channel to send points result to.
 * @param { Pool } db Database connection pool.
 * @param { string } osuToken osu! API token.
 * @param { Message } message Message that triggered the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userWhatIfCount(client: Client, channel: TextChannel, db: Pool, osuToken: string, message: Message): Promise<void> {
  const serverData = await getServerByDiscordId(db, channel.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "userLeaderboardsCount", "Someone asked for leaderboard count, but not in server.");
    return;
  }

  const commands = message.content.split(/\s+/g); // split by one or more spaces
  commands.splice(0, 2); // remove first two elements, which is the mentioned bot and the command itself

  if(commands.length <= 0) {
    await channel.send("**Error:** You need to specify what-if expression.");
    return;
  }

  // TODO: add other users specification

  const whatIfsArray: number[][] = [];
  {
    let status = WhatIfParserStatus.OK;
    let errorIndex = -1;

    const len = commands.length;
    for(let i = 0; i < len; i++) {
      const temp = parseWhatIfCount(commands[i]);
      if(typeof(temp) === "number") {
        status = temp;
        errorIndex = i;
        break;
      }

      whatIfsArray.push(temp);
    }

    if(status > WhatIfParserStatus.OK) {
      switch(status) {
        case WhatIfParserStatus.INVALID_EXPRESSION: // fallthrough
        case WhatIfParserStatus.TYPE_ERROR:
          await channel.send(`**Error:** Invalid what if expression${ len > 1 ? "s" : "" } [at command index ${ errorIndex + 2 }].`);
          return;
        case WhatIfParserStatus.TOP_RANK_ERROR:
          await channel.send(`**Error:** Top rank must be higher than or equal to 1 [at command index ${ errorIndex + 2 }].`);
          return;
        case WhatIfParserStatus.NUMBER_OF_RANKS_ERROR:
          await channel.send(`**Error:** Number of ranks must be higher than or equal to 0 [at command index ${ errorIndex + 2 }].`);
          return;
        default:
          await channel.send("**Error:** Unhandled error occurred. Please contact bot administrator.");
          return;
      }
    }
  }

  const tops = [ 1, 8, 15, 25, 50 ]; // match bathbot <osc top ranks data

  let valid = true;
  whatIfsArray.forEach(whatif => {
    if(!tops.includes(whatif[0])) {
      valid = false;
    }
  });

  if(!valid) {
    await channel.send("**Error:** Rank query must be 1, 8, 15, 25, or 50.");
    return;
  }

  const user = await getDiscordUserByDiscordId(db, message.author.id);
  if(user.status !== DatabaseSuccess.OK) {
    switch(user.status) {
      case DatabaseErrors.USER_NOT_FOUND:
        await channel.send(`**Error:** You haven't linked your account. Link using \`${ client.user?.username } [osu! user ID]\`${ serverData.data.verifyChannelId !== null ? ` in <#${ serverData.data.verifyChannelId }> channel` : "" }.`);
        break;
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error:** Database connection error occurred. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  const osuUser = await getUserByOsuId(osuToken, user.data.osuId);
  {
    if(osuUser.status !== OsuApiSuccessStatus.OK) {
      switch(osuUser.status) {
        case OsuApiErrorStatus.NON_OK:
          await channel.send("**Error:** osu! user not found.");
          break;
        case OsuApiErrorStatus.CLIENT_ERROR:
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          break;
      }

      return;
    }

    if(osuUser.data.status !== OsuUserStatus.USER || osuUser.data.user === undefined) { // TODO: use conditional type for user
      switch(osuUser.data.status) {
        case OsuUserStatus.BOT:
          await channel.send("**Error:** Suddenly, you turned into a skynet...");
          break;
        case OsuUserStatus.DELETED: // fallthrough
        case OsuUserStatus.NOT_FOUND:
          await channel.send("**Error:** Did you do something to your osu! account?");
          break;
      }

      return;
    }
  }

  const osuUsername = osuUser.data.user.userName as string;
  const useRespektive = typeof(process.env.USE_RESPEKTIVE) === "string" || process.env.USE_RESPEKTIVE === "1";

  log(LogSeverity.LOG, "userWhatIfCount", `Calculating what-ifs for user: ${ osuUsername }`);

  const topCounts: number[] = [];
  if(!useRespektive) {
    {
      const topCountsRequest = [
        getTopCounts(osuUsername, 1),
        getTopCounts(osuUsername, 8),
        getTopCounts(osuUsername, 15),
        getTopCounts(osuUsername, 25),
        getTopCounts(osuUsername, 50)
      ];

      const temp = await Promise.all(topCountsRequest);
      {
        let error = OsuStatsErrorStatus.OK;
        const len = temp.length;
        for(let i = 0; i < len; i++) {
          const tempCountResponse = temp[i];

          if(tempCountResponse.status !== OsuStatsSuccessStatus.OK) {
            error = tempCountResponse.status;
            break;
          }

          topCounts.push(tempCountResponse.data.count);
        }

        if(error !== OsuStatsErrorStatus.OK) {
          switch(error) {
            case OsuStatsErrorStatus.USER_NOT_FOUND:
              await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
              break;
            case OsuStatsErrorStatus.API_ERROR:
              await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
              break;
            case OsuStatsErrorStatus.CLIENT_ERROR:
              await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
              break;
          }

          return;
        }
      }
    }
  }
  else {
    const temp = await getTopCountsFromRespektive(user.data.osuId);
    if(temp.status !== OsuStatsSuccessStatus.OK) {
      switch(temp.status) {
        case OsuStatsErrorStatus.USER_NOT_FOUND:
          await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
          break;
        case OsuStatsErrorStatus.API_ERROR:
          await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
          break;
        case OsuStatsErrorStatus.CLIENT_ERROR:
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          break;
      }

      return;
    }
  }

  let originalPoints = 0;
  if(!useRespektive) {
    originalPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
  }
  else {
    originalPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
  }

  {
    let error = false;

    const len = whatIfsArray.length;
    for(let i = 0; i < len; i++) {
      const topIndex = tops.findIndex(top => top === whatIfsArray[i][0]);
      if(topIndex < 0) { // top count index not found
        error = true;
        break;
      }

      topCounts[topIndex] = whatIfsArray[i][1];
    }

    if(error) {
      await channel.send("**Error:** Rank query must be 1, 8, 15, 25, or 50.");
      return;
    }
  }

  let newPoints = 0;
  if(!useRespektive) {
    newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);

    await countPoints(client, channel, osuUsername, topCounts);
  }
  else {
    newPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);

    await countRespektivePoints(client, channel, osuUsername, topCounts);
  }

  const difference = newPoints - originalPoints;
  if(difference === 0) {
    await channel.send(`<@${ message.author.id }> would increase nothing!`);
    return;
  }

  await channel.send(`<@${ message.author.id }> would **${ difference > 0 ? "increase" : "decrease" } ${ Math.abs(difference) }** points from current top count.`);
}

/**
 * Sends calculated points and embed to specified channel.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Discord channel to send message to.
 * @param { string } username osu! username.
 * @param { number[] } topCounts Array of top counts.
 *
 * @returns { Promise<Message> } Promise object with `Discord.Message` sent message object.
 */
async function countPoints(client: Client, channel: TextChannel, username: string, topCounts: number[]): Promise<Message> {
  log(LogSeverity.LOG, "countPoints", `Calculating points for username: ${ username }`);

  const newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
  const draft = counter(
    topCounts[0],
    topCounts[1],
    topCounts[2],
    topCounts[3],
    topCounts[4],
    username
  );

  const ret = await channel.send({ embeds: [ draft ] });
  await addWysiReaction(client, ret, topCounts, newPoints);
  return ret;
}

/**
 * Sends respektive API's calculated points and embed to specified channel.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Discord channel to send message to.
 * @param { string } username osu! username.
 * @param { number[] } topCounts Array of top counts.
 *
 * @returns { Promise<Message> } Promise object with `Discord.Message` sent message object.
 */
async function countRespektivePoints(client: Client, channel: TextChannel, username: string, topCounts: number[]): Promise<Message> {
  log(LogSeverity.LOG, "countRespektivePoints", `Calculating points for username: ${ username }`);

  const newPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
  const draft = counterRespektive(
    topCounts[0],
    topCounts[1],
    topCounts[2],
    topCounts[3],
    username
  );

  const ret = await channel.send({ embeds: [ draft ] });
  await addWysiReaction(client, ret, topCounts, newPoints);
  return ret;
}

export { userLeaderboardsCountFromBathbot, userLeaderboardsCount, userWhatIfCount, countPoints, countRespektivePoints };
