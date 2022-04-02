const Discord = require("discord.js");
const { Pool } = require("pg");
const { LogSeverity, log } = require("../log");
const { getUserByOsuId } = require("../api/osu");
const { getTopCounts } = require("../api/osustats");
const { insertOrUpdateAssignment } = require("../db/assignments");
const { getDiscordUserByDiscordId, insertUser } = require("../db/users");
const { DatabaseErrors, AssignmentType, OsuUserStatus, OsuStatsStatus } = require("../common");
const { deltaTimeToString } = require("../time");

async function updateUserData(token, client, channel, db, osuId, points) {
  if(typeof(token) !== "string") {
    log(LogSeverity.ERROR, "updateUserData", "token must be string.");
    process.exit(1);
  }

  if(!(client instanceof Discord.Client)) {
    log(LogSeverity.ERROR, "updateUserData", "client must be a Discord.Client object instance.");
    process.exit(1);
  }

  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "updateUserData", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "updateUserData", "db must be a Pool object instance.");
    process.exit(1);
  }

  if(typeof(osuId) !== "number" && typeof(osuId) !== "string") {
    log(LogSeverity.ERROR, "updateUserData", "osuId must be number or string.");
    process.exit(1);
  }

  if(typeof(points) !== "number") {
    log(LogSeverity.ERROR, "updateUserData", "points must be number.");
    process.exit(1);
  }

  const response = await getUserByOsuId(
    token,
    typeof(osuId) === "number" ? osuId : parseInt(osuId, 10)
  ); // TODO: handle deleted user

  const assignmentResult = await insertOrUpdateAssignment(db, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10), points, response.username);
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
      if(
        assignmentResult.role.newRoleId === "0" &&
        (typeof(assignmentResult.role.oldRoleId === "undefined") || (typeof(assignmentResult.role.oldRoleId) === "string" && assignmentResult.role.oldRoleId === "0"))
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
            await member.roles.remove(oldRole);
            log(LogSeverity.LOG, "updateUserData", "Role " + oldRole.name + " removed from user: " + member.user.username + "#" + member.user.discriminator);
            if(assignmentResult.role.newRoleId === "0") {
              log(LogSeverity.LOG, "updateUserData", "newRoleId is zero. Skipping role granting.");
              break; // break if new role is no role 
            }
            updated = true;
          } // use fallthrough
        case AssignmentType.INSERT:
          if(
            assignmentResult.type === AssignmentType.INSERT ||
            (assignmentResult.type === AssignmentType.UPDATE &&
              assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId)
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
          "You have been " + (assignmentResult.delta > 0 ? "promoted" : "demoted") +
          " to **" + assignmentResult.role.newRoleName + "** role. " +
          (assignmentResult.delta > 0 ? "Awesome!" : "Fight back at those leaderboards!")
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

async function fetchUser(channel, db, discordId) {
  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "fetchUser", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "fetchUser", "db must be a Pool object instance.");
    process.exit(1);
  }

  if(typeof(discordId) !== "string") {
    log(LogSeverity.ERROR, "fetchUser", "discordId must be string in Snowflake ID format.");
    process.exit(1);
  }

  const user = await getDiscordUserByDiscordId(db, discordId);

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
      return user;
  }
}

async function fetchOsuUser(channel, token, osuId) {
  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "fetchOsuUser", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(typeof(token) !== "string") {
    log(LogSeverity.ERROR, "fetchOsuUser", "token must be string.");
    process.exit(1);
  }

  if(typeof(osuId) !== "number" && typeof(osuId) !== "string") {
    log(LogSeverity.ERROR, "fetchOsuUser", "osuId must be number or string.");
    process.exit(1);
  }

  const osuUser = await getUserByOsuId(
    token,
    typeof(osuId) === "number" ? osuId : parseInt(osuId, 10)
  );

  switch(osuUser.status) {
    case OsuUserStatus.BOT:
      await channel.send("**Error:** Unable to retrieve osu! user: User type is Bot.");
      return false;
    case OsuUserStatus.NOT_FOUND:
      await channel.send("**Error:** Unable to retrieve osu! user: User not found.");
      return false;
    case OsuUserStatus.DELETED:
      await channel.send("**Error:** Unable to retrieve osu! user: User is deleted.");
      return false;
    default:
      return osuUser;
  }
}

async function fetchOsuStats(channel, osuUsername) {
  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "fetchOsuStats", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(typeof(osuUsername) !== "string") {
    log(LogSeverity.ERROR, "fetchOsuStats", "osuUsername must be string.");
    process.exit(1);
  }
  
  const topCountsRequests = [];
  [ 1, 8, 15, 25, 50 ].forEach(rank => {
    topCountsRequests.push(getTopCounts(osuUsername, rank));
  });

  const topCountsResponses = await Promise.all(topCountsRequests);
  const len = topCountsResponses.length;
  for(let i = 0; i < len; i++) {
    switch(topCountsResponses[i]) {
      case OsuStatsStatus.USER_NOT_FOUND:
        await channel.send("**Error**: Username not found. Maybe osu! API haven't updated your username? (Use `<osc` instead)");
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

    // TODO: handle not found (which should not happen)

    topCounts[idx] = res.count;
  });

  return topCounts;
}

async function insertUserData(channel, db, discordId, osuId, osuUsername) {
  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "insertUserData", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "insertUserData", "db must be a Pool object instance.");
    process.exit(1);
  }

  if(typeof(discordId) !== "string") {
    log(LogSeverity.ERROR, "insertUserData", "discordId must be string.");
    process.exit(1);
  }

  if(typeof(osuId) !== "number" && typeof(osuId) !== "string") {
    log(LogSeverity.ERROR, "insertUserData", "osuId must be number or string.");
    process.exit(1);
  }

  if(typeof(osuUsername) !== "string") {
    log(LogSeverity.ERROR, "insertUserData", "osuUsername must be string.");
    process.exit(1);
  }

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
      await channel.send("**Error:** Unable to link ID: An error occurred with the database connection. Please contact bot administrator.");
      return false;
    }
    case DatabaseErrors.DUPLICATED_DISCORD_ID: {
      await channel.send("**Error:** Unable to link ID: You already linked your osu! ID. Please contact server moderators to make changes.");
      return false;
    }
    case DatabaseErrors.DUPLICATED_OSU_ID: {
      await channel.send("**Error:** Unable to link ID: osu! ID already linked to other Discord user.");
      return false;
    }
    case DatabaseErrors.CLIENT_ERROR:
    case DatabaseErrors.TYPE_ERROR: {
      await channel.send("**Error:** Client error has occurred. Please contact bot administrator.");
      return false;
    }
    default: {
      await channel.send("**Error**: Unknown return value. Please contact bot administrator.");
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
