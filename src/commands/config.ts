import { Client, TextChannel, Message, PermissionFlagsBits, Role } from "discord.js";
import { Pool } from "pg";
import { DBServers } from "../db";
import { Log } from "../utils/log";
import { createConfigCommandsEmbed, createServerConfigurationEmbed } from "../messages/config";
import { ServerNotFoundError } from "../errors/db";

/**
 * Config commands class.
 */
class Config {
  /**
   * Handles configuration commands.
   *
   * @param { Client } client Bot client object.
   * @param { TextChannel } channel Channel of received command.
   * @param { Pool } db Database connection pool.
   * @param { Message } message Message that triggered the object.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async handleConfigCommands(client: Client, channel: TextChannel, db: Pool, message: Message): Promise<void> {
    if(message.member === null) {
      await channel.send("**Error:** This should not happen, but you're not a member.");
      return;
    }

    if(!message.member.permissions.has(PermissionFlagsBits.Administrator)) {
      await channel.send("**Error:** This command is only available for administrators.");
      return;
    }

    const contents = message.content.split(/\s+/g); // split by one or more spaces

    switch(contents[2]) {
      case undefined: // fallthrough
      case "help":
        await this.sendConfigCommands(channel);
        break;
      case "show":
        await this.sendServerConfiguration(db, channel);
        break;
      case "setcountry":
        await this.setServerCountryConfiguration(db, channel, message);
        break;
      case "setverifiedrole":
        await this.setServerVerifiedRoleIdConfiguration(db, channel, message);
        break;
      case "setcommandchannel":
        await this.setServerCommandsChannelConfiguration(db, channel, message);
        break;
      case "setleaderboardschannel":
        await this.setServerLeaderboardsChannelConfiguration(db, channel, message);
        break;
      default:
        await channel.send("**Error:** Unknown command. Send `config help` for help.");
    }
  }

  /**
   * Sends help commands embed to specified `channel`.
   *
   * @param { TextChannel } channel Channel to send embed to.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async sendConfigCommands(channel: TextChannel): Promise<void> {
    await channel.send({ embeds: [ createConfigCommandsEmbed() ] });
  }

  /**
   * Sends current `channel`'s server configuration.
   *
   * @param { Pool } db Database pool object.
   * @param { TextChannel } channel Channel to send embed to.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async sendServerConfiguration(db: Pool, channel: TextChannel): Promise<void> {
    let serverData;

    try {
      serverData = await DBServers.getServerByDiscordId(db, channel.guild.id);
    }
    catch (e) {
      if(e instanceof ServerNotFoundError) {
        await channel.send("**Error:** Server not in database. Please contact bot administrator.");
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return;
    }

    await channel.sendTyping();

    Log.info("sendServerConfiguration", `Sending configuration for server ID ${ channel.guild.id } (${ channel.guild.name })`);

    const guildName = channel.guild.name;
    const guildIconUrl = channel.guild.iconURL();

    const fetchedVerifiedRole = serverData.verifiedRoleId !== null ? (await channel.guild.roles.fetch(serverData.verifiedRoleId)) : null;
    const fetchedCommandsChannel = serverData.commandsChannelId !== null ? (await channel.guild.channels.fetch(serverData.commandsChannelId)) : null;
    const fetchedLeaderboardsChannel = serverData.leaderboardsChannelId !== null ? (await channel.guild.channels.fetch(serverData.leaderboardsChannelId)) : null;

    const countryConfig = serverData.country;
    const verifiedRoleConfig = fetchedVerifiedRole !== null ? fetchedVerifiedRole.name : null;
    const commandsChannelConfig = fetchedCommandsChannel !== null ? fetchedCommandsChannel.name : null;
    const leaderboardsChannelConfig = fetchedLeaderboardsChannel !== null ? fetchedLeaderboardsChannel.name : null;

    await channel.send({ embeds: [ createServerConfigurationEmbed(guildName, guildIconUrl, countryConfig, verifiedRoleConfig, commandsChannelConfig, leaderboardsChannelConfig) ] });
  }

  /**
   * Sets current `channel`'s server country configuration.
   *
   * @param { Pool } db Database connection pool.
   * @param { TextChannel } channel Channel to set country configuration.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async setServerCountryConfiguration(db: Pool, channel: TextChannel, message: Message): Promise<void> {
    // check if server exists
    {
      try {
        await DBServers.getServerByDiscordId(db, channel.guild.id);
      }
      catch (e) {
        if(e instanceof ServerNotFoundError) {
          await channel.send("**Error:** Server not in database. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }
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
      Log.info("setServerCountryConfiguration", `Setting ${ channel.guild.name } server country configuration to ${ contents[3].toUpperCase() }...`);
    }
    else {
      Log.info("setServerCountryConfiguration", `Disabling ${ channel.guild.name } server country configuration...`);
    }

    try {
      await DBServers.setServerCountry(db, channel.guild.id, !disable ? contents[3] : null);
    }
    catch (e) {
      await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      return;
    }

    await channel.send(!disable ? `Set server country restriction to **${ contents[3].toUpperCase() }**.` : "Server country restriction disabled.");
  }

  /**
   * Sets current `channel`'s server verified role ID configuration.
   *
   * @param { Pool } db Database connection pool.
   * @param { TextChannel } channel Channel to set country configuration.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async setServerVerifiedRoleIdConfiguration(db: Pool, channel: TextChannel, message: Message): Promise<void> {
    // check if server exists
    {
      try {
        await DBServers.getServerByDiscordId(db, channel.guild.id);
      }
      catch (e) {
        if(e instanceof ServerNotFoundError) {
          await channel.send("**Error:** Server not in database. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }
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
      Log.info("setServerVerifiedRoleIdConfiguration", `Setting ${ channel.guild.name } server verified role ID to ${ roleId } (${ roleName })...`);
    }
    else {
      Log.info("setServerCountryConfiguration", `Disabling ${ channel.guild.name } server verified role ID configuration...`);
    }

    try {
      await DBServers.setVerifiedRoleId(db, channel.guild.id, !disable ? roleId : null);
    }
    catch (e) {
      await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      return;
    }

    await channel.send(!disable ? `Set server verified role to **${ roleName }**.` : "Server verified role disabled.");
  }

  /**
   * Sets current `channel`'s server commands channel ID configuration.
   *
   * @param { Pool } db Database connection pool.
   * @param { TextChannel } channel Channel to set country configuration.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async setServerCommandsChannelConfiguration(db: Pool, channel: TextChannel, message: Message): Promise<void> {
    // check if server exists
    {
      try {
        await DBServers.getServerByDiscordId(db, channel.guild.id);
      }
      catch (e) {
        if(e instanceof ServerNotFoundError) {
          await channel.send("**Error:** Server not in database. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }
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

        if(tempChannel !== undefined && tempChannel.isTextBased()) {
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
      Log.info("setServerCommandsChannelConfiguration", `Setting ${ channel.guild.name } server commands channel ID to ${ channelId } (${ channelName })...`);
    }
    else {
      Log.info("setServerCountryConfiguration", `Disabling ${ channel.guild.name } server commands channel ID configuration...`);
    }

    try {
      await DBServers.setCommandsChannelId(db, channel.guild.id, !disable ? channelId : null);
    }
    catch (e) {
      await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      return;
    }

    await channel.send(!disable ? `Set server commands channel to **${ channelName }**.` : "Server commands channel restriction disabled.");
  }

  /**
   * Sets current `channel`'s server leaderboards channel ID configuration.
   *
   * @param { Pool } db Database connection pool.
   * @param { TextChannel } channel Channel to set country configuration.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async setServerLeaderboardsChannelConfiguration(db: Pool, channel: TextChannel, message: Message): Promise<void> {
    // check if server exists
    {
      try {
        await DBServers.getServerByDiscordId(db, channel.guild.id);
      }
      catch (e) {
        if(e instanceof ServerNotFoundError) {
          await channel.send("**Error:** Server not in database. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }
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

        if(tempChannel !== undefined && tempChannel.isTextBased()) {
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
      Log.info("setServerLeaderboardsChannelConfiguration", `Setting ${ channel.guild.name } server leaderboard commands channel ID to ${ channelId } (${ channelName })...`);
    }
    else {
      Log.info("setServerCountryConfiguration", `Disabling ${ channel.guild.name } server leaderboard commands channel ID configuration...`);
    }

    try {
      await DBServers.setLeaderboardsChannelId(db, channel.guild.id, !disable ? channelId : null);
    }
    catch (e) {
      await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      return;
    }

    await channel.send(!disable ? `Set server leaderboards commands channel to **${ channelName }**.` : "Server leaderboards channel restriction disabled.");
  }
}

export default Config;
