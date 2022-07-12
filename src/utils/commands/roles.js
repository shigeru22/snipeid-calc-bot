const Discord = require("discord.js");
const { LogSeverity, log } = require("../log");

/**
 * Add specified role to user specified.
 *
 * @param { Discord.Client } client
 * @param { Discord.Channel } channel
 * @param { string } discordId
 * @param { string } roleId
 *
 * @returns { Promise<void> }
 */
async function addRole(client, channel, discordId, roleId) {
  if(!(client instanceof Discord.Client)) {
    log(LogSeverity.ERROR, "addRole", "client must be a Discord.Client object instance.");
    process.exit(1);
  }

  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "addRole", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(typeof(discordId) !== "string") {
    log(LogSeverity.ERROR, "addRole", "discordId must be string in Snowflake ID format.");
    process.exit(1);
  }

  if(typeof(roleId) !== "string") {
    log(LogSeverity.ERROR, "addRole", "roleId must be string in Snowflake ID format.");
    process.exit(1);
  }

  try {
    const server = await client.guilds.fetch(process.env.SERVER_ID);
    const role = await server.roles.fetch(process.env.VERIFIED_ROLE_ID);
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
