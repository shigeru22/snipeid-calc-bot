import { Client, TextChannel, GuildMember } from "discord.js";
import { Pool } from "pg";
import { DBAssignments, DBServers } from "../db";
import { Log } from "../utils/log";
import { DuplicatedRecordError, UserNotFoundError, ServerNotFoundError } from "../errors/db";

/**
 * Roles-related actions class.
 */
class Roles {
  /**
   * Add specified role to user specified.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { string } discordId Discord ID of user to add role to.
   * @param { string } serverId Discord ID of server.
   * @param { string } roleId Discord ID of role to add.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async addRole(client: Client, channel: TextChannel, discordId: string, serverId: string, roleId: string): Promise<void> {
    try {
      const server = await client.guilds.fetch(serverId);

      const role = await server.roles.fetch(roleId);
      const member = await server.members.fetch(discordId);

      if(role === null) {
        Log.warn("addRole", `Role with ID ${ roleId } on server ID ${ serverId } (${ server.name }) can't be found.`);
        return;
      }

      Log.info("addRole", `Granting role for server member: ${ member.user.username }#${ member.user.discriminator }.`);

      await member.roles.add(role);
    }
    catch (e) {
      if(e instanceof Error) {
        Log.error("addRole", `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("addRole", "Unknown error occurred.");
      }

      await channel.send("**Error:** Unable to assign your role. Please contact bot administrator.");
    }
  }

  /**
   * Assigns back rejoined server member's roles.
   *
   * @param { Pool } db Database connection pool.
   * @param { GuildMember } member Rejoined server member.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async reassignRole(db: Pool, member: GuildMember): Promise<void> {
    let verifiedRoleId;
    let currentPointsRoleId;

    try {
      const serverVerifiedRole = await DBServers.getServerByDiscordId(db, member.guild.id);
      verifiedRoleId = serverVerifiedRole.verifiedRoleId;
    }
    catch (e) {
      if(e instanceof ServerNotFoundError) {
        Log.error("reassignRole", `${ member.guild.id }: Server not found in database.`);
      }
      else if(e instanceof DuplicatedRecordError) {
        Log.error("reassignRole", `Duplicated records found with ID ${ member.guild.id }`);
      }

      return;
    }

    try {
      const memberRoleAssignment = await DBAssignments.getAssignmentRoleDataByDiscordId(db, member.guild.id, member.user.id);
      currentPointsRoleId = memberRoleAssignment.discordId;
    }
    catch (e) {
      if(!(e instanceof UserNotFoundError)) {
        if(e instanceof DuplicatedRecordError) {
          Log.error("reassignRole", `Duplicated records found with ID ${ member.guild.id }`);
        }
        return;
      }

      currentPointsRoleId = null;
    }

    if(verifiedRoleId !== null) {
      const role = await member.guild.roles.fetch(verifiedRoleId);

      if(role === null) {
        Log.warn("reassignRole", `${ member.guild.id }: Role with ID ${ verifiedRoleId } not found.`);
      }
      else {
        await member.roles.add(role);
        Log.info("reassignRole", `${ member.guild.id }: Granted ${ role.name } role to ${ member.user.username }#${ member.user.discriminator }.`);
      }
    }

    if(currentPointsRoleId !== null) {
      const role = await member.guild.roles.fetch(currentPointsRoleId);

      if(role === null) {
        Log.warn("reassignRole", `${ member.guild.id }: Role with ID ${ currentPointsRoleId } not found.`);
      }
      else {
        await member.roles.add(role);
        Log.info("reassignRole", `${ member.guild.id }: Granted ${ role.name } role to ${ member.user.username }#${ member.user.discriminator }.`);
      }
    }
  }
}

export default Roles;
