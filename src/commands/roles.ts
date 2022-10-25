import { Client, TextChannel } from "discord.js";
import { LogSeverity, log } from "../utils/log";

/**
 * Add specified role to user specified.
 *
 * @param { Client } client - Discord bot client.
 * @param { TextChannel } channel - Discord channel to send message to.
 * @param { string } discordId - Discord ID of user to add role to.
 * @param { string } serverId - Discord ID of server.
 * @param { string } roleId - Discord ID of role to add.
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

    log(LogSeverity.LOG, "addRole", "Granting role for server member: " + member.user.username + "#" + member.user.discriminator);

    await member.roles.add(role);
  }
  catch (e) {
    if(e instanceof Error) {
      log(LogSeverity.ERROR, "addRole", e.name + ": " + e.message + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "addRole", "Unknown error occurred.");
    }

    await channel.send("**Error:** Unable to assign your role. Please contact bot administrator.");
  }
}

export { addRole };
