import { SlashCommandBuilder, RESTPostAPIChatInputApplicationCommandsJSONBody, SlashCommandSubcommandsOnlyBuilder, CommandInteraction } from "discord.js";

/**
 * Slash command interactions class.
 */
class SlashCommandsFactory {
  /**
   * Slash commands array.
   */
  private static commands = [
    new SlashCommandBuilder().setName("ping")
      .setDescription("Pings the bot."),
    new SlashCommandBuilder().setName("count")
      .setDescription("Calculates points based on leaderboard count.")
      .addStringOption(opt => opt.setName("osuuser")
        .setDescription("osu! username to be calculated.")
        .setRequired(false)),
    new SlashCommandBuilder().setName("serverleaderboard")
      .setDescription("Returns server points leaderboard.")
      .setDMPermission(false),
    new SlashCommandBuilder().setName("link")
      .setDescription("Links your Discord user to an osu! user."),
    new SlashCommandBuilder().setName("help")
      .setDescription("Returns all commands usage help."),
    new SlashCommandBuilder().setName("config")
      .setDescription("Configuration commands.")
      .addSubcommand(cmd => cmd.setName("help")
        .setDescription("Returns all server configuration commands help. Only available for server administrators."))
      .addSubcommand(cmd => cmd.setName("show")
        .setDescription("Returns current server configuration. Only available for server administrators."))
      .addSubcommandGroup(cmdGroup => cmdGroup.setName("set")
        .setDescription("Configuration setter commands.")
        .addSubcommand(cmd => cmd.setName("country")
          .setDescription("Sets country restriction for this server. Leave country option empty to disable.")
          .addStringOption(opt => opt.setName("code")
            .setDescription("2-letter country code. Leave empty to disable.")
            .setRequired(false)))
        .addSubcommand(cmd => cmd.setName("verifiedrole")
          .setDescription("Sets verified user role, see commands help for details. Leave role option empty to disable.")
          .addRoleOption(opt => opt.setName("role")
            .setDescription("Role for verified users. Leave empty to disable.")
            .setRequired(false)))
        .addSubcommand(cmd => cmd.setName("commandschannel")
          .setDescription("Sets server command channel restriction. Leave channel option empty to disable.")
          .addChannelOption(opt => opt.setName("server")
            .setDescription("Server for commands restriction. Leave empty to disable.")
            .setRequired(false)))
        .addSubcommand(cmd => cmd.setName("leaderboardschannel")
          .setDescription("Sets server leaderboard command channel restriction. Leave channel option empty to disable.")
          .addChannelOption(opt => opt.setName("server")
            .setDescription("Server for leaderboard command restriction. Leave empty to disable.")
            .setRequired(false)))
      )
  ];

  /**
   * Gets all slash commands array.
   *
   * @returns { (Omit<SlashCommandBuilder, "addSubcommand" | "addSubcommandGroup"> | SlashCommandSubcommandsOnlyBuilder)[] } Slash commands builder array.
   */
  public static getAllCommands(): (Omit<SlashCommandBuilder, "addSubcommand" | "addSubcommandGroup"> | SlashCommandSubcommandsOnlyBuilder)[] {
    return SlashCommandsFactory.commands;
  }

  /**
   * Builds all commands as REST body data.
   *
   * @returns { RESTPostAPIChatInputApplicationCommandsJSONBody[] } Slash commands builder as JSON data for REST usage.
   */
  public static buildRestData(): RESTPostAPIChatInputApplicationCommandsJSONBody[] {
    return SlashCommandsFactory.commands.map(cmd => cmd.toJSON());
  }

  /**
   * Handles ping interaction.
   *
   * @param { CommandInteraction } interaction Received interaction object.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  public static async handlePingInteraction(interaction: CommandInteraction): Promise<void> {
    await interaction.reply("Pong!");
  }
}

export default SlashCommandsFactory;
