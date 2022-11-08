import { Client, TextChannel, Message, Permissions, Role } from "discord.js";
import { Pool } from "pg";
import { getServerByDiscordId, setCommandsChannelId, setLeaderboardsChannelId, setServerCountry, setVerifiedRoleId } from "../db/servers";
import { createConfigCommandsEmbed, createServerConfigurationEmbed } from "../messages/config";
import { LogSeverity, log } from "../utils/log";
import { DatabaseErrors, DatabaseSuccess } from "../utils/common";

async function handleConfigCommands(client: Client, channel: TextChannel, db: Pool, message: Message) {
  if(message.member === null) {
    await channel.send("**Error:** This should not happen, but you're not a member.");
    return;
  }

  if(!message.member.permissions.has(Permissions.FLAGS.ADMINISTRATOR)) {
    await channel.send("**Error:** This command is only available for administrators.");
    return;
  }

  const contents = message.content.split(/\s+/g); // split by one or more spaces

  switch(contents[2]) {
    case undefined: // fallthrough
    case "help":
      await sendConfigCommands(channel);
      break;
    case "show":
      await sendServerConfiguration(db, channel);
      break;
    case "setcountry":
      await setServerCountryConfiguration(db, channel, message);
      break;
    case "setverifiedrole":
      await setServerVerifiedRoleIdConfiguration(db, channel, message);
      break;
    case "setcommandchannel":
      await setServerCommandsChannelConfiguration(db, channel, message);
      break;
    case "setleaderboardschannel":
      await setServerLeaderboardsChannelConfiguration(db, channel, message);
      break;
    default:
      await channel.send("**Error:** Unknown command. Send `config help` for help.");
  }
}

async function sendConfigCommands(channel: TextChannel) {
  await channel.send({ embeds: [ createConfigCommandsEmbed() ] });
}

async function sendServerConfiguration(db: Pool, channel: TextChannel) {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "userLeaderboardsCount", "Someone asked for server configuration, but server not in database.");
    return;
  }

  await channel.sendTyping();

  log(LogSeverity.LOG, "sendServerConfiguration", `Sending configuration for server ID ${ channel.guild.id } (${ channel.guild.name })`);

  const guildName = channel.guild.name;
  const guildIconUrl = channel.guild.iconURL();

  const fetchedVerifiedRole = serverData.data.verifiedRoleId !== null ? (await channel.guild.roles.fetch(serverData.data.verifiedRoleId)) : null;
  const fetchedCommandsChannel = serverData.data.commandsChannelId !== null ? (await channel.guild.channels.fetch(serverData.data.commandsChannelId)) : null;
  const fetchedLeaderboardsChannel = serverData.data.leaderboardsChannelId !== null ? (await channel.guild.channels.fetch(serverData.data.leaderboardsChannelId)) : null;

  const countryConfig = serverData.data.country;
  const verifiedRoleConfig = fetchedVerifiedRole !== null ? fetchedVerifiedRole.name : null;
  const commandsChannelConfig = fetchedCommandsChannel !== null ? fetchedCommandsChannel.name : null;
  const leaderboardsChannelConfig = fetchedLeaderboardsChannel !== null ? fetchedLeaderboardsChannel.name : null;

  await channel.send({ embeds: [ createServerConfigurationEmbed(guildName, guildIconUrl, countryConfig, verifiedRoleConfig, commandsChannelConfig, leaderboardsChannelConfig) ] });
}

async function setServerCountryConfiguration(db: Pool, channel: TextChannel, message: Message) {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "setServerCountryConfiguration", "Someone asked for server configuration, but server not in database.");
    return;
  }

  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let disable = false;

  if(contents[3] === "-") {
    disable = true;
  }
  else if(contents[3].length !== 2) {
    await channel.send("**Error:** Country must be a 2-letter code. Send `config help` for details.");
    return;
  }

  if(!disable) {
    log(LogSeverity.LOG, "setServerCountryConfiguration", `Setting ${ channel.guild.name } server country configuration to ${ contents[3].toUpperCase() }...`);
  }
  else {
    log(LogSeverity.LOG, "setServerCountryConfiguration", `Disabling ${ channel.guild.name } server country configuration...`);
  }

  const result = await setServerCountry(db, channel.guild.id, !disable ? contents[3] : null);

  if(result.status !== DatabaseSuccess.OK) {
    switch(result.status) {
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error:** Database connection failed. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  await channel.send(!disable ? `Set server country restriction to ${ contents[3].toUpperCase() }.` : "Server country restriction disabled.");
}

async function setServerVerifiedRoleIdConfiguration(db: Pool, channel: TextChannel, message: Message) {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "setServerVerifiedRoleIdConfiguration", "Someone asked for server configuration, but server not in database.");
    return;
  }

  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let roleName = "";
  let roleId = "";
  let disable = false;

  if(contents[3] === "-") {
    disable = true;
  }
  else {
    let role: Role | undefined = undefined;

    if(message.mentions.roles.size > 0) {
      role = message.mentions.roles.at(0);
    }

    if(role === undefined) {
      if(contents[3] === undefined) {
        await channel.send("**Error:** Role must be specified. Send `config help` for details.");
        return;
      }

      const tempRole = await channel.guild.roles.fetch(contents[3]);
      if(tempRole === null) {
        await channel.send("**Error:** Role with specified ID not found. Send `config help` for details.");
        return;
      }

      role = tempRole;
    }

    roleName = role.name;
    roleId = role.id;
  }

  if(!disable) {
    log(LogSeverity.LOG, "setServerVerifiedRoleIdConfiguration", `Setting ${ channel.guild.name } server verified role ID to ${ roleId } (${ roleName })...`);
  }
  else {
    log(LogSeverity.LOG, "setServerCountryConfiguration", `Disabling ${ channel.guild.name } server verified role ID configuration...`);
  }

  const result = await setVerifiedRoleId(db, channel.guild.id, !disable ? roleId : null);

  if(result.status !== DatabaseSuccess.OK) {
    switch(result.status) {
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error:** Database connection failed. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  await channel.send(!disable ? `Set server country restriction to ${ contents[3].toUpperCase() }.` : "Server country restriction disabled.");
}

async function setServerCommandsChannelConfiguration(db: Pool, channel: TextChannel, message: Message) {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "setServerCommandsChannelConfiguration", "Someone asked for server configuration, but server not in database.");
    return;
  }

  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let channelName = "";
  let channelId = "";
  let disable = false;

  if(contents[3] === "-") {
    disable = true;
  }
  else {
    let targetChannel: TextChannel | undefined = undefined;

    if(message.mentions.channels.size > 0) {
      const tempChannel = message.mentions.channels.at(0);

      if(tempChannel !== undefined && tempChannel.isText()) {
        targetChannel = tempChannel as TextChannel;
      }
    }

    if(targetChannel === undefined) {
      if(contents[3] === undefined) {
        await channel.send("**Error:** Channel must be specified. Send `config help` for details.");
        return;
      }

      const tempChannel = await channel.guild.channels.fetch(contents[3]);
      if(tempChannel === null) {
        await channel.send("**Error:** Channel with specified ID not found. Send `config help` for details.");
        return;
      }

      targetChannel = tempChannel as TextChannel;
    }

    channelName = targetChannel.name;
    channelId = targetChannel.id;
  }

  if(!disable) {
    log(LogSeverity.LOG, "setServerCommandsChannelConfiguration", `Setting ${ channel.guild.name } server commands channel ID to ${ channelId } (${ channelName })...`);
  }
  else {
    log(LogSeverity.LOG, "setServerCountryConfiguration", `Disabling ${ channel.guild.name } server commands channel ID configuration...`);
  }

  const result = await setCommandsChannelId(db, channel.guild.id, !disable ? channelId : null);

  if(result.status !== DatabaseSuccess.OK) {
    switch(result.status) {
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error:** Database connection failed. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  await channel.send(!disable ? `Set server commands channel to ${ contents[3].toUpperCase() }.` : "Server commands channel restriction disabled.");
}

async function setServerLeaderboardsChannelConfiguration(db: Pool, channel: TextChannel, message: Message) {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "setServerLeaderboardsChannelConfiguration", "Someone asked for server configuration, but server not in database.");
    return;
  }

  const contents = message.content.split(/\s+/g); // split by one or more spaces
  let channelName = "";
  let channelId = "";
  let disable = false;

  if(contents[3] === "-") {
    disable = true;
  }
  else {
    let targetChannel: TextChannel | undefined = undefined;

    if(message.mentions.channels.size > 0) {
      const tempChannel = message.mentions.channels.at(0);

      if(tempChannel !== undefined && tempChannel.isText()) {
        targetChannel = tempChannel as TextChannel;
      }
    }

    if(targetChannel === undefined) {
      if(contents[3] === undefined) {
        await channel.send("**Error:** Channel must be specified. Send `config help` for details.");
        return;
      }

      const tempChannel = await channel.guild.channels.fetch(contents[3]);
      if(tempChannel === null) {
        await channel.send("**Error:** Channel with specified ID not found. Send `config help` for details.");
        return;
      }

      targetChannel = tempChannel as TextChannel;
    }

    channelName = targetChannel.name;
    channelId = targetChannel.id;
  }

  if(!disable) {
    log(LogSeverity.LOG, "setServerLeaderboardsChannelConfiguration", `Setting ${ channel.guild.name } server leaderboard commands channel ID to ${ channelId } (${ channelName })...`);
  }
  else {
    log(LogSeverity.LOG, "setServerCountryConfiguration", `Disabling ${ channel.guild.name } server leaderboard commands channel ID configuration...`);
  }

  const result = await setLeaderboardsChannelId(db, channel.guild.id, !disable ? channelId : null);

  if(result.status !== DatabaseSuccess.OK) {
    switch(result.status) {
      case DatabaseErrors.CONNECTION_ERROR:
        await channel.send("**Error:** Database connection failed. Please contact bot administrator.");
        break;
      case DatabaseErrors.CLIENT_ERROR:
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        break;
    }

    return;
  }

  await channel.send(!disable ? `Set server commands channel to ${ contents[3].toUpperCase() }.` : "Server commands channel restriction disabled.");
}

export { handleConfigCommands };
