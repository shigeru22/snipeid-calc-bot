import { Client, TextChannel } from "discord.js";
import { Pool } from "pg";
import { LogSeverity, log } from "../utils/log";
import { getUserByOsuId } from "../api/osu";
import { getTopCounts } from "../api/osustats";
import { insertOrUpdateAssignment } from "../db/assignments";
import { getDiscordUserByDiscordId, insertUser } from "../db/users";
import { DatabaseErrors, AssignmentType, OsuUserStatus, OsuStatsStatus } from "../utils/common";
import { deltaTimeToString } from "../utils/time";

/**
 * Updates user data in the database and assigns roles based on points received.
 *
 * @param { string } osuToken - osu! API token
 * @param { Client } client - Discord bot client.
 * @param { TextChannel } channel - Discord channel to send message to.
 * @param { Pool } db - Database connection pool.
 * @param { number | string } osuId - osu! user ID.
 * @param { number } points - Calculated points.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function updateUserData(osuToken: string, client: Client, channel: TextChannel, db: Pool, osuId: number | string, points: number): Promise<void> {
  const osuUser = await getUserByOsuId(osuToken, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10));
  if(osuUser.user === undefined) {
    switch(osuUser.status) {
      case OsuUserStatus.BOT:
        await channel.send("**Error:** Suddenly, you turned into a skynet...");
        break;
      case OsuUserStatus.DELETED: // falltrough
      case OsuUserStatus.NOT_FOUND:
        await channel.send("**Error:** Did you do something to your osu! account?");
        break;
      case OsuUserStatus.API_ERROR:
        await channel.send("**Error:** osu! API error. Check osu!status?");
        break;
      case OsuUserStatus.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  const assignmentResult = await insertOrUpdateAssignment(db, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10), points, osuUser.user.username);
  if(assignmentResult.status !== DatabaseErrors.OK || assignmentResult.data === undefined) {
    switch(assignmentResult.status) {
      case DatabaseErrors.USER_NOT_FOUND: break;
      case DatabaseErrors.ROLES_EMPTY:
        await channel.send("**Error:** No roles defined for this server.");
        break;
      default:
        await channel.send("**Error:** Data update error occurred. Please contact bot administrator.");
    }

    return;
  }

  const today = new Date();

  switch(assignmentResult.data.type) {
    case AssignmentType.INSERT:
      await channel.send(
        "<@" + assignmentResult.data.discordId + "> achieved " + "**" + assignmentResult.data.delta + "** " + (assignmentResult.data.delta === 1 ? "point" : "points" ) + ". Go for those leaderboards!"
      );
      break;
    case AssignmentType.UPDATE:
      await channel.send(
        "<@" + assignmentResult.data.discordId + "> has " + (assignmentResult.data.delta >= 0 ? "gained" : "lost") + " **" + assignmentResult.data.delta + "** " + (assignmentResult.data.delta === 1 ? "point" : "points" ) + " since " + deltaTimeToString(today.getTime() - (assignmentResult.data.lastUpdate as Date).getTime()) + " ago." // TODO: check lastUpdate type correctness
      );
      break;
  }

  try {
    if(
      assignmentResult.data.role.newRoleId === "0" && (typeof(assignmentResult.data.role.oldRoleId === "undefined") || (typeof(assignmentResult.data.role.oldRoleId) === "string" && assignmentResult.data.role.oldRoleId === "0"))
    ) { // no role
      log(LogSeverity.LOG, "updateUserData", "newRoleId is either zero or oldRoleId is not available. Skipping role granting.");
      return;
    }

    if(assignmentResult.data.role.oldRoleId === assignmentResult.data.role.newRoleId) {
      log(LogSeverity.LOG, "updateUserData", "Role is currently the same. Skipping role granting.");
      return;
    }

    const server = await client.guilds.fetch(process.env.SERVER_ID as string);
    const member = await server.members.fetch(assignmentResult.data.discordId);
    let updated = false;
    let warned = false;

    switch(assignmentResult.data.type) {
      case AssignmentType.UPDATE:
        if(assignmentResult.data.role.oldRoleId !== undefined && assignmentResult.data.role.newRoleId !== assignmentResult.data.role.oldRoleId) {
          if(assignmentResult.data.role.oldRoleId !== "0") {
            const oldRole = await server.roles.fetch(assignmentResult.data.role.oldRoleId);

            if(oldRole === null) {
              // TODO: handle role re-addition after failed on next query

              log(LogSeverity.WARN, "updateUserData", `Role with ID ${ assignmentResult.data.role.oldRoleId } from server with ID ${ process.env.SERVER_ID } (${ server.name }) can't be found. Informing server channel.`);
              await channel.send("**Note:** Roles might have been changed!");

              warned = true;
            }
            else {
              await member.roles.remove(oldRole);
              log(LogSeverity.LOG, "updateUserData", "Role " + oldRole.name + " removed from user: " + member.user.username + "#" + member.user.discriminator);
            }
          }

          if(assignmentResult.data.role.newRoleId === "0") {
            log(LogSeverity.LOG, "updateUserData", "newRoleId is zero. Skipping role granting.");
            await channel.send("You have been demoted to no role. Fight back at those leaderboards!");

            break; // break if new role is no role
          }
          updated = true;
        } // use fallthrough to continue new role addition
      case AssignmentType.INSERT:
        if(
          assignmentResult.data.type === AssignmentType.INSERT || (assignmentResult.data.type === AssignmentType.UPDATE && assignmentResult.data.role.newRoleId !== assignmentResult.data.role.oldRoleId)
        ) {
          const newRole = await server.roles.fetch(assignmentResult.data.role.newRoleId);

          if(newRole === null) {
            log(LogSeverity.WARN, "updateUserData", `Role with ID ${ assignmentResult.data.role.oldRoleId } from server with ID ${ process.env.SERVER_ID } (${ server.name }) can't be found. Informing server channel.`);

            if(!warned) {
              await channel.send("**Note:** Roles might have been changed!");
            }
          }
          else {
            await member.roles.add(newRole);
            log(LogSeverity.LOG, "updateUserData", "Role " + newRole.name + " added to user: " + member.user.username + "#" + member.user.discriminator);
            updated = true;
          }
        }
        break;
    }

    if(updated) {
      await channel.send(
        "You have been " + (assignmentResult.data.delta > 0 ? "promoted" : "demoted") + " to **" + assignmentResult.data.role.newRoleName + "** role. " + (assignmentResult.data.delta > 0 ? "Awesome!" : "Fight back at those leaderboards!")
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

/**
 * Fetches user from the database.
 *
 * @param { TextChannel } channel - Channel to send message to.
 * @param { Pool } db - Database connection.
 * @param { string } discordId - Discord ID of the user.
 *
 * @returns { Promise<{ userId: number; discordId: string; osuId: number; } | false> } Promise object with `userId`, `discordId`, and `osuId`, or `false` if user was not found.
 */
async function fetchUser(channel: TextChannel, db: Pool, discordId: string): Promise<{ userId: number; discordId: string; osuId: number; } | false> {
  const user = await getDiscordUserByDiscordId(db, discordId);

  if(user.status !== DatabaseErrors.OK || user.user === undefined) {
    switch(user.status) {
      case DatabaseErrors.USER_NOT_FOUND:
        await channel.send("**Error**: You haven't connected your osu! ID. Use Bathbot's `<osc` command instead or link your osu! ID using `@SnipeID link [osu! ID]`.");
        break;
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error**: Database connection failed. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error**: Client error has occurred. Please contact bot administrator.");
        break;
      default:
        log(LogSeverity.ERROR, "fetchUser", "Unknown user fetch return value.");
        break;
    }

    return false;
  }

  return user.user;
}

/**
 * Fetches osu! user from osu! ID.
 *
 * @param { TextChannel } channel - Channel to send message to.
 * @param { string } token - osu! API token.
 * @param { number | string } osuId - osu! user ID.
 *
 * @returns { Promise<{ status: number; username?: string; isCountryCodeAllowed?: boolean } | false> } Promise object with `status` and `username`, or `false` in case of errors.
 */
async function fetchOsuUser(channel: TextChannel, token: string, osuId: number | string): Promise<{ status: number; username: string; isCountryCodeAllowed: boolean; } | false> {
  const osuUser = await getUserByOsuId(token, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10));

  if(osuUser.status !== OsuUserStatus.USER || osuUser.user === undefined) {
    switch(osuUser.status) {
      case OsuUserStatus.BOT:
        await channel.send("**Error:** Unable to retrieve osu! user: User type is Bot.");
        break;
      case OsuUserStatus.NOT_FOUND:
        await channel.send("**Error:** Unable to retrieve osu! user: User not found.");
        break;
      case OsuUserStatus.DELETED:
        await channel.send("**Error:** Unable to retrieve osu! user: User is deleted.");
        break;
      case OsuUserStatus.API_ERROR:
        await channel.send("**Error:** osu! API error. Check osu!status?");
        break;
      case OsuUserStatus.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return false;
  }

  return {
    status: osuUser.status,
    username: osuUser.user.username,
    isCountryCodeAllowed: osuUser.user.isCountryCodeAllowed
  };
}

/**
 * Fetches osu!Stats' number of top ranks.
 *
 * @param { TextChannel } channel - Channel to send message to.
 * @param { string } osuUsername - osu! username.
 *
 * @returns { Promise<number[] | boolean> } Promise object with number of ranks array (top 1, 8, 15, 25, and 50), or `false` in case of errors.
 */
async function fetchOsuStats(channel: TextChannel, osuUsername: string): Promise<number[] | boolean> {
  const topCountsRequests = [
    getTopCounts(osuUsername, 1),
    getTopCounts(osuUsername, 8),
    getTopCounts(osuUsername, 15),
    getTopCounts(osuUsername, 25),
    getTopCounts(osuUsername, 50)
  ];

  const topCountsResponses = await Promise.all(topCountsRequests);
  {
    let error = OsuStatsStatus.OK;
    const len = topCountsResponses.length;
    for(let i = 0; i < len; i++) {
      if(topCountsResponses[i].status !== OsuStatsStatus.OK) {
        error = topCountsResponses[i].status;
        break;
      }
    }

    switch(error) {
      case OsuStatsStatus.USER_NOT_FOUND:
        await channel.send("**Error**: Username not found. Maybe osu!Stats hasn't updated your username?");
        return false;
      case OsuStatsStatus.API_ERROR: // fallthrough
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

    topCounts[idx] = res.count as number;
  });

  return topCounts;
}

/**
 * Links and inserts user to database.
 *
 * @param { TextChannel } channel - Channel to send message to.
 * @param { Pool } db - Database connection pool.
 * @param { string } discordId - Discord ID of the user.
 * @param { number | string } osuId - osu! user ID.
 * @param { string } osuUsername - osu! username.
 *
 * @returns { Promise<boolean> } Promise object with `true` if user was linked, or `false` in case of errors.
 */
async function insertUserData(channel: TextChannel, db: Pool, discordId: string, osuId: number | string, osuUsername: string): Promise<boolean> {
  const result = await insertUser(
    db,
    discordId,
    typeof(osuId) === "number" ? osuId : parseInt(osuId, 10),
    osuUsername
  );

  if(result !== DatabaseErrors.OK) {
    switch(result) {
      case DatabaseErrors.CONNECTION_ERROR: {
        await channel.send("**Error:** Database connection error occurred. Please contact bot administrator.");
        break;
      }
      case DatabaseErrors.DUPLICATED_DISCORD_ID: {
        await channel.send("**Error:** You already linked your osu! ID. Please contact server moderators to make changes.");
        break;
      }
      case DatabaseErrors.DUPLICATED_OSU_ID: {
        await channel.send("**Error:** osu! ID already linked to other Discord user.");
        break;
      }
      case DatabaseErrors.CLIENT_ERROR: {
        await channel.send("**Error:** Client error has occurred. Please contact bot administrator.");
        break;
      }
    }

    return false;
  }

  return true;
}

export { updateUserData, fetchUser, fetchOsuUser, fetchOsuStats, insertUserData };
