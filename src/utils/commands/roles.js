const Discord = require("discord.js");

async function addRole(client, channel, discordId, roleId) {
  if(!(client instanceof Discord.Client)) {
    console.log("[ERROR] addRole :: client must be a Discord.Client object instance.");
    process.exit(1);
  }

  if(!(channel instanceof Discord.Channel)) {
    console.log("[ERROR] addRole :: channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(typeof(discordId) !== "string") {
    console.log("[ERROR] addRole :: discordId must be string in Snowflake ID format.");
    process.exit(1);
  }

  if(typeof(roleId) !== "string") {
    console.log("[ERROR] addRole :: roleId must be string in Snowflake ID format.");
    process.exit(1);
  }

  try {
    const server = await client.guilds.fetch(process.env.SERVER_ID);
    const role = await server.roles.fetch(process.env.VERIFIED_ROLE_ID);

    (await server.members.fetch(discordId)).roles.add(role);
  }
  catch (e) {
    if(e instanceof Error) {
      console.log("[ERROR] onNewMessage :: " + e.name + ": " + e.message + "\n" + e.stack);
    }
    else {
      console.log("[ERROR] onNewMessage :: Unknown error occurred.");
    }

    await channel.send("**Error:** Unable to assign your role. Please contact bot administrator.");
  }
}

module.exports = {
  addRole
};
