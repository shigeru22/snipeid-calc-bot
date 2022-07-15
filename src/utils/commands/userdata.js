const { LogSeverity, log } = require("../log");
const { getUserByOsuId } = require("../api/osu");
const { getTopCounts } = require("../api/osustats");
const { insertOrUpdateAssignment } = require("../db/assignments");
const { getDiscordUserByDiscordId, insertUser } = require("../db/users");
const { DatabaseErrors, AssignmentType, OsuUserStatus, OsuApiStatus, OsuStatsStatus } = require("../common");
const { deltaTimeToString } = require("../time");

/**
 * Updates user data in the database and assigns roles based on points received.
 *
 * @param { string } osuToken - osu! API token
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Discord channel to send message to.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { number | string } osuId - osu! user ID.
 * @param { number } points - Calculated points.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function updateUserData(osuToken, client, channel, db, osuId, points) {
  const osuUser = await getUserByOsuId(osuToken, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10));
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

  const assignmentResult = await insertOrUpdateAssignment(db, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10), points, osuUser.username);
  if(typeof(assignmentResult) === "number") {
    switch(assignmentResult) {
      case DatabaseErrors.USER_NOT_FOUND: break;
      default:
        await channel.send("**Error:** Data update error occurred. Please contact bot administrator.");
    }
  }
  else {
    const today = new Date();

    switch(assignmentResult.type) {
      case AssignmentType.INSERT:
        await channel.send(
          "<@" + assignmentResult.discordId + "> achieved " + "**" + assignmentResult.delta + "** " + (assignmentResult.delta === 1 ? "point" : "points" ) + ". Go for those leaderboards!"
        );
        break;
      case AssignmentType.UPDATE:
        await channel.send(
          "<@" + assignmentResult.discordId + "> has " + (assignmentResult.delta >= 0 ? "gained" : "lost") + " **" + assignmentResult.delta + "** " + (assignmentResult.delta === 1 ? "point" : "points" ) + " since " + deltaTimeToString(today.getTime() - assignmentResult.lastUpdate.getTime()) + " ago."
        );
        break;
    }

    try {
      if(
        assignmentResult.role.newRoleId === "0" && (typeof(assignmentResult.role.oldRoleId === "undefined") || (typeof(assignmentResult.role.oldRoleId) === "string" && assignmentResult.role.oldRoleId === "0"))
      ) { // no role
        log(LogSeverity.LOG, "updateUserData", "newRoleId is either zero or oldRoleId is not available. Skipping role granting.");
        return;
      }

      if(assignmentResult.role.oldRoleId === assignmentResult.role.newRoleId) {
        log(LogSeverity.LOG, "updateUserData", "Role is currently the same. Skipping role granting.");
        return;
      }

      const server = await client.guilds.fetch(process.env.SERVER_ID);
      const member = await server.members.fetch(assignmentResult.discordId);
      let updated = false;

      switch(assignmentResult.type) {
        case AssignmentType.UPDATE:
          if(assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId) {
            const oldRole = await server.roles.fetch(assignmentResult.role.oldRoleId);
            if(assignmentResult.role.oldRoleId !== "0") {
              await member.roles.remove(oldRole);
              log(LogSeverity.LOG, "updateUserData", "Role " + oldRole.name + " removed from user: " + member.user.username + "#" + member.user.discriminator);
            }

            if(assignmentResult.role.newRoleId === "0") {
              log(LogSeverity.LOG, "updateUserData", "newRoleId is zero. Skipping role granting.");
              await channel.send("You have been demoted to no role. Fight back at those leaderboards!");
              break; // break if new role is no role
            }
            updated = true;
          } // use fallthrough
        case AssignmentType.INSERT:
          if(
            assignmentResult.type === AssignmentType.INSERT || (assignmentResult.type === AssignmentType.UPDATE && assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId)
          ) {
            const newRole = await server.roles.fetch(assignmentResult.role.newRoleId);
            await member.roles.add(newRole);
            log(LogSeverity.LOG, "updateUserData", "Role " + newRole.name + " added to user: " + member.user.username + "#" + member.user.discriminator);
            updated = true;
          }
          break;
      }

      if(updated) {
        await channel.send(
          "You have been " + (assignmentResult.delta > 0 ? "promoted" : "demoted") + " to **" + assignmentResult.role.newRoleName + "** role. " + (assignmentResult.delta > 0 ? "Awesome!" : "Fight back at those leaderboards!")
        );
      }
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.ERROR, "updateUserData", e.name + ": " + e.message + "\n" + e.stack);
      }
      else {
        log(LogSeverity.ERROR, "updateUserData", "Unknown error occurred.");
      }

      await channel.send("**Error:** Unable to assign your role. Please contact bot administrator.");
    }
  }
}

/**
 * Fetches user from the database.
 *
 * @param { import("discord.js").TextChannel } channel - Channel to send message to.
 * @param { import("pg").Pool } db - Database connection.
 * @param { string } discordId - Discord ID of the user.
 *
 * @returns { Promise<{ userId: number; discordId: string; osuId: number; } | boolean> } Promise object with `userId`, `discordId`, and `osuId`, or `false` if user was not found.
 */
async function fetchUser(channel, db, discordId) {
  const user = await getDiscordUserByDiscordId(db, discordId);

  if(typeof(user) === "number") {
    switch(user) {
      case DatabaseErrors.USER_NOT_FOUND:
        await channel.send("**Error**: You haven't connected your osu! ID. Use Bathbot's `<osc` command instead or link your osu! ID using `@SnipeID link [osu! ID]`.");
        return false;
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error**: Database connection failed. Please contact bot administrator.");
        return false;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error**: Client error has occurred. Please contact bot administrator.");
        return false;
      default:
        log(LogSeverity.ERROR, "fetchUser", "Unknown user fetch return value.");
        return false;
    }
  }

  return user;
}

/**
 * Fetches osu! user from osu! ID.
 *
 * @param { import("discord.js").TextChannel } channel - Channel to send message to.
 * @param { string } token - osu! API token.
 * @param { number | string } osuId - osu! user ID.
 *
 * @returns { Promise<{ status: number; username?: string; isCountryCodeAllowed?: boolean } | boolean> } Promise object with `status` and `username`, or `false` in case of errors.
 */
async function fetchOsuUser(channel, token, osuId) {
  const osuUser = await getUserByOsuId(token, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10));

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

  let error = false;
  switch(osuUser.status) {
    case OsuUserStatus.BOT:
      await channel.send("**Error:** Unable to retrieve osu! user: User type is Bot.");
      error = true;
      break;
    case OsuUserStatus.NOT_FOUND:
      await channel.send("**Error:** Unable to retrieve osu! user: User not found.");
      error = true;
      break;
    case OsuUserStatus.DELETED:
      await channel.send("**Error:** Unable to retrieve osu! user: User is deleted.");
      error = true;
      break;
  }

  if(error) {
    /* eslint-disable consistent-return */
    return false; // yeah, it's boolean
  }

  /* eslint-disable consistent-return */
  return osuUser; // while this is getUserByOsuId return object
}

/**
 * Fetches osu!Stats' number of top ranks.
 *
 * @param { import("discord.js").TextChannel } channel - Channel to send message to.
 * @param { string } osuUsername - osu! username.
 *
 * @returns { Promise<number[] | boolean> } Promise object with number of ranks array (top 1, 8, 15, 25, and 50), or `false` in case of errors.
 */
async function fetchOsuStats(channel, osuUsername) {
  const topCountsRequests = [];
  [ 1, 8, 15, 25, 50 ].forEach(rank => {
    topCountsRequests.push(getTopCounts(osuUsername, rank));
  });

  const topCountsResponses = await Promise.all(topCountsRequests);
  {
    let error = 0;
    const len = topCountsResponses.length;
    for(let i = 0; i < len; i++) {
      if(typeof(topCountsResponses[i]) === "number") {
        error = topCountsResponses[i];
        break;
      }
    }

    switch(error) {
      case OsuStatsStatus.USER_NOT_FOUND:
        await channel.send("**Error**: Username not found. Maybe osu!Stats hasn't updated your username?");
        return false;
      case OsuStatsStatus.TYPE_ERROR: // fallthrough
      case OsuStatsStatus.CLIENT_ERROR:
        await channel.send("**Error**: Client error has occurred. Please contact bot administrator.");
        return false;
    }
  }

  const topCounts = [ 0, 0, 0, 0, 0 ];
  topCountsResponses.forEach(res => {
    let idx = -1;

    switch(res.maxRank) {
      case 1: idx = 0; break;
      case 8: idx = 1; break;
      case 15: idx = 2; break;
      case 25: idx = 3; break;
      case 50: idx = 4; break;
    }

    topCounts[idx] = res.count;
  });

  return topCounts;
}

/**
 * Links and inserts user to database.
 *
 * @param { import("discord.js").TextChannel } channel - Channel to send message to.
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } discordId - Discord ID of the user.
 * @param { number | string } osuId - osu! user ID.
 * @param { string } osuUsername - osu! username.
 *
 * @returns { Promise<boolean> } Promise object with `true` if user was linked, or `false` in case of errors.
 */
async function insertUserData(channel, db, discordId, osuId, osuUsername) {
  const result = await insertUser(
    db,
    discordId,
    typeof(osuId) === "number" ? osuId : parseInt(osuId, 10),
    osuUsername
  );

  switch(result) {
    case DatabaseErrors.OK:
      await channel.send("Linked Discord user <@" + discordId + "> to osu! user **" + osuUsername + "**.");
      return true;
    case DatabaseErrors.CONNECTION_ERROR: {
      await channel.send("**Error:** Database connection error occurred. Please contact bot administrator.");
      return false;
    }
    case DatabaseErrors.DUPLICATED_DISCORD_ID: {
      await channel.send("**Error:** You already linked your osu! ID. Please contact server moderators to make changes.");
      return false;
    }
    case DatabaseErrors.DUPLICATED_OSU_ID: {
      await channel.send("**Error:** osu! ID already linked to other Discord user.");
      return false;
    }
    case DatabaseErrors.CLIENT_ERROR:
    case DatabaseErrors.TYPE_ERROR: {
      await channel.send("**Error:** Client error has occurred. Please contact bot administrator.");
      return false;
    }
    default: {
      await channel.send("**Error**: Unknown error occurred. Please contact bot administrator.");
      return false;
    }
  }
}

module.exports = {
  updateUserData,
  fetchUser,
  fetchOsuUser,
  fetchOsuStats,
  insertUserData
};
