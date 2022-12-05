import { SlashCommandBuilder, RESTPostAPIChatInputApplicationCommandsJSONBody, CommandInteraction } from "discord.js";

/**
 * Slash command interactions class.
 */
class SlashCommandsFactory {
  /**
   * Slash commands array.
   */
  private static commands = [
    new SlashCommandBuilder().setName("ping")
      .setDescription("Pings the bot.")
  ];

  /**
   * Gets all commands array.
   *
   * @returns { SlashCommandBuilder[] } Slash commands builder array.
   */
  public static getAllCommands(): SlashCommandBuilder[] {
    return SlashCommandsFactory.commands;
  }

  /**
   * Builds all commands as REST body data.
   *
   * @returns { RESTPostAPIChatInputApplicationCommandsJSONBody[] } Slash commands builder as JSON data for REST usage.
   */
  public static buildRestData(): RESTPostAPIChatInputApplicationCommandsJSONBody[] {
    return SlashCommandsFactory.commands.map(command => command.toJSON());
  }

  public static async handlePingInteraction(interaction: CommandInteraction) {
    await interaction.reply("Pong!");
  }
}

export default SlashCommandsFactory;
