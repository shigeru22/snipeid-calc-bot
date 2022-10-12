const { getUserByOsuId } = require("../api/osu");
const { getTopCounts, getTopCountsFromRespektive } = require("../api/osustats");
const { getDiscordUserByDiscordId } = require("../db/users");
const { addWysiReaction } = require("./reactions");
const { updateUserData } = require("./userdata");
const { calculatePoints, calculateRespektivePoints, counter, counterRespektive } = require("../messages/counter");
const { OsuUserStatus, OsuApiStatus, OsuStatsStatus, DatabaseErrors } = require("../common");
const { WhatIfParserStatus, parseUsername, parseOsuIdFromLink, parseTopCountDescription, parseWhatIfCount } = require("../parser");
const { LogSeverity, log } = require("../log");

// <osc, using Bathbot message response
/**
 * Sends calculated points from Bathbot `<osc` command.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Channel to send points result to.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { import("discord.js").Message } message - Message that triggered the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userLeaderboardsCountFromBathbot(client, channel, db, osuToken, message) {
  const index = message.embeds.findIndex(
    embed => typeof(embed.title) === "string" && embed.title.toLowerCase().startsWith("in how many top x map leaderboards is")
  ); // <osc command should return at index 0, else it's not the specified command

  if(index === -1) {
    return;
  }

  const title = message.embeds[index].title;
  const desc = message.embeds[index].description;
  const link = message.embeds[index].author.url;

  const username = parseUsername(title);
  const osuId = parseOsuIdFromLink(link);

  // [ top_1, top_8, top_15, top_25, top_50 ]
  const topCounts = parseTopCountDescription(desc); // TODO: handle respektive's API response in the future?
  const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
  await countPoints(client, channel, username, topCounts);

  await updateUserData(osuToken, client, channel, db, osuId, points);
}

// @[BOT_NAME] count
/**
 * Sends top leaderboard count to specified channel.
 * Basically, this is Bathbot's `<osc` command.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Channel to send points result to.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { string } discordId - Discord ID of the user who sent the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userLeaderboardsCount(client, channel, db, osuToken, discordId) {
  const user = await getDiscordUserByDiscordId(db, discordId);
  if(typeof(user) === "number") {
    switch(user) {
      case DatabaseErrors.USER_NOT_FOUND:
        await channel.send("**Error:** How you've been here? You haven't linked your account.");
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

  const osuUser = await getUserByOsuId(osuToken, user.osuId);
  if(typeof(osuUser) === "number") {
    switch(osuUser) {
      case OsuApiStatus.NON_OK:
        await channel.send("**Error:** osu! API error. Check osu!status?");
        break;
      case OsuApiStatus.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  switch(osuUser.status) {
    case OsuUserStatus.BOT:
      await channel.send("**Error:** Suddenly, you turned into a skynet...");
      return;
    case OsuUserStatus.DELETED: // falltrough
    case OsuUserStatus.NOT_FOUND:
      await channel.send("**Error:** Did you do something to your osu! account?");
      return;
  }

  const useRespektive = typeof(process.env.USE_RESPEKTIVE) === "string" && process.env.USE_RESPEKTIVE === "1";

  let topCounts;
  if(!useRespektive) {
    {
      const topCountsRequest = [
        getTopCounts(osuUser.username, 1),
        getTopCounts(osuUser.username, 8),
        getTopCounts(osuUser.username, 15),
        getTopCounts(osuUser.username, 25),
        getTopCounts(osuUser.username, 50)
      ];

      const temp = await Promise.all(topCountsRequest);
      {
        let error = 0;
        const len = temp.length;
        for(let i = 0; i < len; i++) {
          if(typeof(temp[i]) === "number") {
            // @ts-ignore - should be number
            error = temp[i];
            break;
          }
        }

        if(error !== 0) {
          switch(error) {
            case OsuStatsStatus.USER_NOT_FOUND:
              await channel.send("**Error:** osu!stats API said you're not found. Check osu!Stats manually?");
              break;
            case OsuStatsStatus.CLIENT_ERROR:
              await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
              break;
          }

          return;
        }
      }

      // @ts-ignore - no longer number at this point
      topCounts = [ temp[0].count, temp[1].count, temp[2].count, temp[3].count, temp[4].count ];
    }
  }
  else {
    topCounts = await getTopCountsFromRespektive(user.osuId);
  }

  if(typeof(topCounts) === "number") {
    switch(topCounts) {
      case OsuStatsStatus.USER_NOT_FOUND:
        await channel.send("**Error:** osu!stats API said you're not found. Check osu!Stats manually?");
        break;
      case OsuStatsStatus.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  let points = 0;
  if(!useRespektive) {
    points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
    await countPoints(client, channel, osuUser.username, topCounts);
  }
  else {
    points = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
    await countRespektivePoints(client, channel, osuUser.username, topCounts);
  }

  await updateUserData(osuToken, client, channel, db, user.osuId, points);
}

// @[BOT_NAME] whatif [what-if expression]
/**
 * Sends user's points in the specified what-if situation.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Channel to send points result to.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { import("discord.js").Message } message - Message that triggered the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userWhatIfCount(client, channel, db, osuToken, message) {
  const commands = message.content.split(/\s+/g); // split by one or more spaces
  commands.splice(0, 2); // remove first two elements, which is the mentioned bot and the command itself

  if(commands.length <= 0) {
    await channel.send("**Error:** You need to specify what-if expression.");
    return;
  }

  const whatIfsArray = [];
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
  if(typeof(user) === "number") {
    switch(user) {
      case DatabaseErrors.USER_NOT_FOUND:
        await channel.send("**Error:** How you've been here? You haven't linked your account.");
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

  const osuUser = await getUserByOsuId(osuToken, user.osuId);
  if(typeof(osuUser) === "number") {
    switch(osuUser) {
      case OsuApiStatus.NON_OK:
        await channel.send("**Error:** osu! API error. Check osu!status?");
        break;
      case OsuApiStatus.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  switch(osuUser.status) {
    case OsuUserStatus.BOT:
      await channel.send("**Error:** Suddenly, you turned into a skynet...");
      return;
    case OsuUserStatus.DELETED: // falltrough
    case OsuUserStatus.NOT_FOUND:
      await channel.send("**Error:** Did you do something to your osu! account?");
      return;
  }

  const useRespektive = typeof(process.env.USE_RESPEKTIVE) === "string" || process.env.USE_RESPEKTIVE === "1";

  let topCounts;
  if(!useRespektive) {
    const topCountsRequest = [
      getTopCounts(osuUser.username, 1),
      getTopCounts(osuUser.username, 8),
      getTopCounts(osuUser.username, 15),
      getTopCounts(osuUser.username, 25),
      getTopCounts(osuUser.username, 50)
    ];

    const temp = await Promise.all(topCountsRequest);

    {
      let error = -1;
      const len = topCountsRequest.length;
      for(let i = 0; i < len; i++) {
        if(typeof(temp[i]) === "number") {
          // @ts-ignore
          error = temp[i]; // it's a number, ignore the error
          break;
        }
      }

      if(error >= 0) {
        switch(error) {
          case OsuStatsStatus.USER_NOT_FOUND:
            await channel.send("**Error:** osu!Stats said you're not found on their database.");
            return;
          case OsuStatsStatus.CLIENT_ERROR:
            await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
            return;
        }
      }
    }

    // @ts-ignore
    topCounts = [ temp[0].count, temp[1].count, temp[2].count, temp[3].count, temp[4].count ]; // temp shouldn't be number now
  }
  else {
    const temp = await getTopCountsFromRespektive(user.osuId);

    if(typeof(temp) === "number") {
      switch(temp) {
        case OsuStatsStatus.USER_NOT_FOUND:
          await channel.send("**Error:** Respektive said you're not found on their database.");
          return;
        case OsuStatsStatus.CLIENT_ERROR:
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          return;
      }
    }

    topCounts = temp;
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

    // @ts-ignore - it's a number, ignore the error
    await countPoints(client, channel, osuUser.username, topCounts);
  }
  else {
    newPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);

    // @ts-ignore - it's a number, ignore the error
    await countRespektivePoints(client, channel, osuUser.username, topCounts);
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
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Discord channel to send message to.
 * @param { string } username - osu! username.
 * @param { number[] } topCounts - Array of top counts.
 *
 * @returns { Promise<import("discord.js").Message> } Promise object with `Discord.Message` sent message object.
 */
async function countPoints(client, channel, username, topCounts) {
  log(LogSeverity.LOG, "countPoints", "Calculating points for username: " + username);

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
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Discord channel to send message to.
 * @param { string } username - osu! username.
 * @param { number[] } topCounts - Array of top counts.
 *
 * @returns { Promise<import("discord.js").Message> } Promise object with `Discord.Message` sent message object.
 */
async function countRespektivePoints(client, channel, username, topCounts) {
  log(LogSeverity.LOG, "countRespektivePoints", "Calculating points for username: " + username);

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

module.exports = {
  userLeaderboardsCountFromBathbot,
  userLeaderboardsCount,
  userWhatIfCount,
  countPoints,
  countRespektivePoints
};

