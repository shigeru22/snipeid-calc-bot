import { ContextMenuCommandBuilder, ApplicationCommandType, RESTPostAPIContextMenuApplicationCommandsJSONBody } from "discord.js";

/**
 * Context command interactions class.
 */
class ContextCommandsFactory {
  /**
   * Context commands array.
   */
  private static commands = [
    new ContextMenuCommandBuilder().setName("Calculate points")
      .setType(ApplicationCommandType.User)
      .setDMPermission(false)
  ];

  /**
   * Gets all context commands array.
   *
   * @returns { ContextMenuCommandBuilder[] } Context commands builder array.
   */
  public static getAllCommands(): ContextMenuCommandBuilder[] {
    return ContextCommandsFactory.commands;
  }

  public static buildRestData(): RESTPostAPIContextMenuApplicationCommandsJSONBody[] {
    return ContextCommandsFactory.commands.map(cmd => cmd.toJSON());
  }
}

export default ContextCommandsFactory;
