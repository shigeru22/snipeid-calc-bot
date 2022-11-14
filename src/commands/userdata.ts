import { Client, TextChannel } from "discord.js";
import { getUserByOsuId } from "../api/osu";
import { getTopCounts, getTopCountsFromRespektive } from "../api/osustats";
import { DatabaseWrapper } from "../db";
import { Log } from "../utils/log";
import { AssignmentType, OsuUserStatus } from "../utils/common";
import { TimeUtils } from "../utils/time";
import { NonOKError, NotFoundError } from "../errors/api";
import { UserNotFoundError, ServerNotFoundError, RolesEmptyError, ConflictError } from "../errors/db";
import { isOsuUser } from "../types/api/osu";
import { IDBServerUserData } from "../types/db/users";
import { IOsuUserData } from "../types/commands/userdata";

/**
 * User data actions class.
 */
class UserData {
  /**
   * Updates user data in the database and assigns roles based on points received.
   *
   * @param { string } osuToken osu! API token
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { number | string } osuId osu! user ID.
   * @param { number } points Calculated points.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async updateUserData(osuToken: string, client: Client, channel: TextChannel, osuId: number | string, points: number): Promise<void> {
    let serverData;
    let osuUser;
    let assignmentResult;

    try {
      serverData = await DatabaseWrapper.getInstance()
        .getServersModule()
        .getServerByDiscordId(channel.guild.id);
    }
    catch (e) {
      if(e instanceof ServerNotFoundError) {
        Log.error("updateUserData", `Server with ID ${ channel.guild.id } not found in database.`);
        await channel.send("**Error:** Server not in database.");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return;
    }

    Log.debug("updateUserData", `Updating user data for osu! ID ${ osuId }.`);

    try {
      osuUser = await getUserByOsuId(osuToken, typeof(osuId) === "string" ? parseInt(osuId, 10) : osuId);

      if(!isOsuUser(osuUser)) {
        switch(osuUser.status) {
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
    catch (e) {
      if(e instanceof NotFoundError) {
        await channel.send("**Error:** osu! user not found.");
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return;
    }

    try {
      assignmentResult = await DatabaseWrapper.getInstance()
        .getAssignmentsModule()
        .insertOrUpdateAssignment(
          channel.guildId,
          typeof(osuId) === "number" ? osuId : parseInt(osuId, 10),
          osuUser.user.userName,
          osuUser.user.country,
          points
        );
    }
    catch (e) {
      if(e instanceof RolesEmptyError) {
        // await channel.send("**Error:** No roles defined for this server.");
      }
      else {
        await channel.send("**Error:** Data update error occurred. Please contact bot administrator.");
      }

      return;
    }

    try {
      await DatabaseWrapper.getInstance()
        .getUsersModule()
        .updateUser(
          typeof(osuId) === "number" ? osuId : parseInt(osuId, 10)
          , points
        );
    }
    catch (e) {
      await channel.send("**Error:** Data update error occurred. Please contact bot administrator.");
      return;
    }

    const today = new Date();

    switch(assignmentResult.type) {
      case AssignmentType.INSERT:
        await channel.send(
          `<@${ assignmentResult.discordId }> achieved **${ assignmentResult.delta }** point${ assignmentResult.delta !== 1 ? "s" : "" }. Go for those leaderboards!`
        );
        break;
      case AssignmentType.UPDATE:
        await channel.send(
          `<@${ assignmentResult.discordId }> has ${ assignmentResult.delta >= 0 ? "gained" : "lost" } **${ assignmentResult.delta }** point${ assignmentResult.delta !== 1 ? "s" : "" } since ${ TimeUtils.deltaTimeToString(today.getTime() - (assignmentResult.lastUpdate as Date).getTime()) } ago.` // lastUpdate in update assignment type returns not null
        );
        break;
    }

    if(assignmentResult.role.newRoleId === "0" && (typeof(assignmentResult.role.oldRoleId === "undefined") || (typeof(assignmentResult.role.oldRoleId) === "string" && assignmentResult.role.oldRoleId === "0"))) { // no role
      Log.info("updateUserData", "newRoleId is either zero or oldRoleId is not available. Skipping role granting.");
      return;
    }

    if(assignmentResult.role.oldRoleId === assignmentResult.role.newRoleId) {
      Log.info("updateUserData", "Role is currently the same. Skipping role granting.");
      return;
    }

    const server = await client.guilds.fetch(serverData.discordId as string);
    const member = await server.members.fetch(assignmentResult.discordId);
    let updated = false;
    let warned = false;

    switch(assignmentResult.type) {
      case AssignmentType.UPDATE:
        if(assignmentResult.role.oldRoleId !== undefined && assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId) {
          if(assignmentResult.role.oldRoleId !== "0") {
            const oldRole = await server.roles.fetch(assignmentResult.role.oldRoleId);

            if(oldRole === null) {
              // TODO: handle role re-addition after failed on next query

              Log.warn("updateUserData", `Role with ID ${ assignmentResult.role.oldRoleId } from server with ID ${ serverData.discordId } (${ server.name }) can't be found. Informing server channel.`);
              await channel.send("**Note:** Roles might have been changed. Check configurations for this server.");

              warned = true;
            }
            else {
              await member.roles.remove(oldRole);
              Log.info("updateUserData", `Role ${ oldRole.name } removed from user ${ member.user.username }#${ member.user.discriminator }.`);
            }
          }

          if(assignmentResult.role.newRoleId === "0") {
            Log.info("updateUserData", "newRoleId is zero. Skipping role granting.");
            await channel.send("You have been demoted to no role. Fight back at those leaderboards!");

            break; // break if new role is no role
          }
          updated = true;
        } // use fallthrough to continue new role addition
      case AssignmentType.INSERT:
        if(
          assignmentResult.type === AssignmentType.INSERT || (assignmentResult.type === AssignmentType.UPDATE && assignmentResult.role.newRoleId !== assignmentResult.role.oldRoleId)
        ) {
          const newRole = await server.roles.fetch(assignmentResult.role.newRoleId);

          if(newRole === null) {
            Log.warn("updateUserData", `Role with ID ${ assignmentResult.role.oldRoleId } from server with ID ${ serverData.serverId } (${ server.name }) can't be found. Informing server channel.`);

            if(!warned) {
              await channel.send("**Note:** Roles might have been changed. Check configurations for this server.");
            }
          }
          else {
            await member.roles.add(newRole);
            Log.info("updateUserData", `Role ${ newRole.name } added to user ${ member.user.username }#${ member.user.discriminator }.`);
            updated = true;
          }
        }
        break;
    }

    if(updated) {
      await channel.send(
        `You have been ${ assignmentResult.delta > 0 ? "promoted" : "demoted" } to **${ assignmentResult.role.newRoleName }** role. ${ assignmentResult.delta > 0 ? "Awesome!" : "Fight back at those leaderboards!" }`
      );
    }
  }

  /**
   * Fetches user from the database.
   *
   * @param { TextChannel } channel Channel to send message to.
   * @param { string } discordId Discord ID of the user.
   *
   * @returns { Promise<IDBServerUserData | null> } Promise object with `userId`, `discordId`, and `osuId`, or `null` if user was not found.
   */
  static async fetchUser(channel: TextChannel, discordId: string): Promise<IDBServerUserData | null> {
    Log.debug("fetchUser", `Fetching user with ID ${ discordId }.`);

    let user;

    try {
      user = await DatabaseWrapper.getInstance()
        .getUsersModule()
        .getDiscordUserByDiscordId(discordId);
    }
    catch (e) {
      if(e instanceof UserNotFoundError) {
        await channel.send("**Error**: You haven't connected your osu! ID. Use Bathbot's `<osc` command instead or link your osu! ID using `@SnipeID link [osu! ID]`.");
      }
      else {
        await channel.send("**Error**: An error occurred. Please contact bot administrator.");
      }

      return null;
    }

    return user;
  }

  /**
   * Fetches osu! user from osu! ID.
   *
   * @param { TextChannel } channel Channel to send message to.
   * @param { string } token osu! API token.
   * @param { number | string } osuId osu! user ID.
   *
   * @returns { Promise<IOsuUserData | null> } Promise object with `status` and `username`, or `null` in case of errors.
   */
  static async fetchOsuUser(channel: TextChannel, token: string, osuId: number | string): Promise<IOsuUserData | null> {
    Log.debug("fetchOsuUser", `Fetching osu! user with ID ${ osuId }.`);

    let osuUser;

    try {
      osuUser = await getUserByOsuId(token, typeof(osuId) === "number" ? osuId : parseInt(osuId, 10));

      if(!isOsuUser(osuUser)) {
        switch(osuUser.status) {
          case OsuUserStatus.BOT:
            await channel.send("**Error:** Suddenly, you turned into a skynet...");
            break;
          case OsuUserStatus.DELETED: // fallthrough
          case OsuUserStatus.NOT_FOUND:
            await channel.send("**Error:** Did you do something to your osu! account?");
            break;
        }

        return null;
      }
    }
    catch (e) {
      if(e instanceof NotFoundError) {
        await channel.send("**Error:** osu! user not found.");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return null;
    }

    return {
      status: osuUser.status,
      userName: osuUser.user.userName,
      country: osuUser.user.country
    };
  }

  /**
   * Fetches osu!Stats' number of top ranks.
   *
   * @param { TextChannel } channel Channel to send message to.
   * @param { string } osuUsername osu! username.
   *
   * @returns { Promise<[ number, number, number, number ] | null> } Promise object with number of ranks array (top 1, 8, 15, 25, and 50), or `null` in case of errors.
   */
  static async fetchOsuStats(channel: TextChannel, osuUsername: string): Promise<[ number, number, number, number, number ] | null> {
    Log.debug("fetchOsuStats", `Fetching osu!Stats data for username ${ osuUsername }.`);

    const topCountsRequest = [
      getTopCounts(osuUsername, 1),
      getTopCounts(osuUsername, 8),
      getTopCounts(osuUsername, 15),
      getTopCounts(osuUsername, 25),
      getTopCounts(osuUsername, 50)
    ];

    let tempResponse;

    try {
      tempResponse = await Promise.all(topCountsRequest);
    }
    catch (e) {
      if(e instanceof NotFoundError) {
        await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
      }
      else if(e instanceof NonOKError) {
        await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return null;
    }

    return [
      tempResponse[0].count,
      tempResponse[1].count,
      tempResponse[2].count,
      tempResponse[3].count,
      tempResponse[4].count
    ];
  }

  /**
   * Fetches respektive osu!Stats' number of top ranks.
   *
   * @param { TextChannel } channel Channel to send message to.
   * @param { string | number } osuId osu! user ID.
   *
   * @returns { Promise<[ number, number, number, number ] | null> } Promise object with number of ranks array (top 1, 8, 25, and 50), or `null` in case of errors.
   */
  static async fetchRespektiveOsuStats(channel: TextChannel, osuId: string | number): Promise<[ number, number, number, number ] | null> {
    Log.debug("fetchRespektiveOsuStats", `Fetching respektive osu!Stats data for osu! ID ${ osuId }.`);

    let tempResponse;

    try {
      tempResponse = await getTopCountsFromRespektive(typeof(osuId) === "string" ? parseInt(osuId, 10) : osuId);
    }
    catch (e) {
      if(e instanceof NotFoundError) {
        await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
      }
      else if(e instanceof NonOKError) {
        await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return null;
    }

    return tempResponse;
  }

  /**
   * Links and inserts user to database.
   *
   * @param { TextChannel } channel Channel to send message to.
   * @param { Pool } db Database connection pool.
   * @param { string } discordId Discord ID of the user.
   * @param { number | string } osuId osu! user ID.
   * @param { string } osuUsername osu! username.
   * @param { string } countryCode Country code.
   *
   * @returns { Promise<boolean> } Promise object with `true` if user was linked, or `false` in case of errors.
   */
  static async insertUserData(channel: TextChannel, discordId: string, osuId: number | string, osuUsername: string, countryCode: string): Promise<boolean> {
    Log.debug("insertUserData", `Inserting user data for osu! ID ${ osuId } with Discord user ID ${ discordId }.`);

    try {
      await DatabaseWrapper.getInstance()
        .getUsersModule()
        .insertUser(
          discordId,
          typeof(osuId) === "number" ? osuId : parseInt(osuId, 10),
          osuUsername,
          countryCode
        );
    }
    catch (e) {
      if(e instanceof ConflictError) {
        if(e.column !== null) {
          if(e.column === "discordId") {
            await channel.send("**Error:** You have linked your osu! ID. Contact bot administrator to make changes.");
          }
          else if(e.column === "osuId") {
            await channel.send("**Error:** osu! ID linked to other Discord user.");
          }
          else {
            await channel.send("**Error:** Unknown data conflict occurred.");
          }
        }
        else {
          await channel.send("**Error:** Either you have linked an osu! ID or osu! ID already linked to other Discord user.");
        }
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return false;
    }

    return true;
  }
}

export default UserData;
