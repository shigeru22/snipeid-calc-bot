const { getUserByOsuId } = require("../api/osu");
const { getTopCounts } = require("../api/osustats");
const { getDiscordUserByDiscordId } = require("../db/users");
const { countPoints } = require("./points");
const { addWysiReaction } = require("./reactions");
const { updateUserData } = require("./userdata");
const { calculatePoints } = require("../messages/counter");
const { OsuUserStatus, OsuApiStatus, OsuStatsStatus, DatabaseErrors } = require("../common");

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

  let topCounts = [];
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
          // @ts-ignore
          error = temp[i]; // TODO: check whether this is number at this point
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

    // @ts-ignore
    topCounts = [ temp[0].count, temp[1].count, temp[2].count, temp[3].count, temp[4].count ]; // no longer number
  }

  const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
  const message = await countPoints(channel, osuUser.username, topCounts);
  await addWysiReaction(client, message, topCounts, points);

  await updateUserData(osuToken, client, channel, db, user.osuId, points);
}

/**
 * Sends user's points in the specified what-if situation.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Channel to send points result to.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { string } discordId - Discord ID of the user who sent the command.
 * @param { number[][] } whatIfsArray - Array of what-if rank situations (for example, `["1=8", "8=15"]`).
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function userWhatIfCount(client, channel, db, osuToken, discordId, whatIfsArray) {
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

  let topCounts = [];
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

  const originalPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);

  const tops = [ 1, 8, 15, 25, 50 ];

  whatIfsArray.forEach(whatif => {
    const topIndex = tops.findIndex(top => top === whatif[0]);

    // TODO: handle index not found

    topCounts[topIndex] = whatif[1];
  });

  const newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);

  const message = await countPoints(channel, osuUser.username, topCounts);
  await addWysiReaction(client, message, topCounts, newPoints); // TODO: move to countPoints command

  const difference = newPoints - originalPoints;
  if(difference === 0) {
    await channel.send(`<@${ discordId }> would increase nothing!`);
    return;
  }

  await channel.send(`<@${ discordId }> would **${ difference > 0 ? "increase" : "decrease" } ${ Math.abs(difference) }** points from current top count.`);
}

module.exports = {
  userLeaderboardsCount,
  userWhatIfCount
};

