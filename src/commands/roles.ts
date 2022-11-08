import { Client, TextChannel, GuildMember } from "discord.js";
import { Pool } from "pg";
import { getServerByDiscordId } from "../db/servers";
import { LogSeverity, log } from "../utils/log";
import { DatabaseErrors, DatabaseSuccess } from "../utils/common";
import { getAssignmentRoleDataByDiscordId } from "../db/assignments";

/**
 * Add specified role to user specified.
 *
 * @param { Client } clien Discord bot client.
 * @param { TextChannel } channel Discord channel to send message to.
 * @param { string } discordId Discord ID of user to add role to.
 * @param { string } serverId Discord ID of server.
 * @param { string } roleId Discord ID of role to add.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function addRole(client: Client, channel: TextChannel, discordId: string, serverId: string, roleId: string): Promise<void> {
  try {
    const server = await client.guilds.fetch(serverId);

    const role = await server.roles.fetch(roleId);
    const member = await server.members.fetch(discordId);

    if(role === null) {
      log(LogSeverity.WARN, "addRole", `Role with ID ${ roleId } on server ID ${ serverId } (${ server.name }) can't be found.`);
      return;
    }

    log(LogSeverity.LOG, "addRole", `Granting role for server member: ${ member.user.username }#${ member.user.discriminator }.`);

    await member.roles.add(role);
  }
  catch (e) {
    if(e instanceof Error) {
      log(LogSeverity.ERROR, "addRole", `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "addRole", "Unknown error occurred.");
    }

    await channel.send("**Error:** Unable to assign your role. Please contact bot administrator.");
  }
}

async function reassignRole(db: Pool, member: GuildMember) {
  try {
    let verifiedRoleId: string | null = null;
    let currentPointsRoleId: string | null = null;

    {
      const serverVerifiedRole = await getServerByDiscordId(db, member.guild.id);
      if(serverVerifiedRole.status !== DatabaseSuccess.OK) {
        log(LogSeverity.ERROR, "reassignRole", "Failed to fetch server in database."); // TODO: handle connection and client errors
        return;
      }

      verifiedRoleId = serverVerifiedRole.data.verifiedRoleId;
    }

    {
      const memberRoleAssignment = await getAssignmentRoleDataByDiscordId(db, member.guild.id, member.user.id);
      if(memberRoleAssignment.status !== DatabaseSuccess.OK) {
        switch(memberRoleAssignment.status) {
          case DatabaseErrors.NO_RECORD:
            break;
          default:
            log(LogSeverity.ERROR, "reassignRole", "Failed to fetch member's assignment in database."); // TODO: handle connection and client errors
            return;
        }
      }
      else {
        currentPointsRoleId = memberRoleAssignment.data.discordId;
      }
    }

    if(verifiedRoleId !== null) {
      const role = await member.guild.roles.fetch(verifiedRoleId);

      if(role === null) {
        log(LogSeverity.WARN, "reassignRole", `${ member.guild.id }: Role with ID ${ verifiedRoleId } not found.`);
      }
      else {
        await member.roles.add(role);
        log(LogSeverity.LOG, "reassignRole", `${ member.guild.id }: Granted ${ role.name } role to ${ member.user.username }#${ member.user.discriminator }.`);
      }
    }

    if(currentPointsRoleId !== null) {
      const role = await member.guild.roles.fetch(currentPointsRoleId);

      if(role === null) {
        log(LogSeverity.WARN, "reassignRole", `${ member.guild.id }: Role with ID ${ currentPointsRoleId } not found.`);
      }
      else {
        await member.roles.add(role);
        log(LogSeverity.LOG, "reassignRole", `${ member.guild.id }: Granted ${ role.name } role to ${ member.user.username }#${ member.user.discriminator }.`);
      }
    }
  }
  catch (e) {
    if(e instanceof Error) {
      log(LogSeverity.ERROR, "addRole", `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "addRole", "Unknown error occurred.");
    }
  }
}

export { addRole, reassignRole };
