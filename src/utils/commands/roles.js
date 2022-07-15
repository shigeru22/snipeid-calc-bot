const { LogSeverity, log } = require("../log");

/**
 * Add specified role to user specified.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").TextChannel } channel - Discord channel to send message to.
 * @param { string } discordId - Discord ID of user to add role to.
 * @param { string } serverId - Discord ID of server.
 * @param { string } roleId - Discord ID of role to add.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function addRole(client, channel, discordId, serverId, roleId) {
  try {
    const server = await client.guilds.fetch(serverId);

    const role = await server.roles.fetch(roleId);
    const member = await server.members.fetch(discordId);

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

module.exports = {
  addRole
};
