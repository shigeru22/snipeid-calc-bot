import { Client, CommandInteraction, REST, Routes } from "discord.js";
import { Environment } from "../../utils";
import SlashCommandsFactory from "./slash";
import ContextCommandsFactory from "./context";

async function initializeInteractionCommands() {
  const slashRestData = SlashCommandsFactory.buildRestData();
  const contextRestData = ContextCommandsFactory.buildRestData();

  const rest = new REST({ version: "10" }).setToken(Environment.getBotToken());

  await rest.put(
    Routes.applicationCommands(Environment.getBotClientId()),
    {
      body: [
        ...slashRestData,
        ...contextRestData
      ]
    }
  );
}

async function handleInteractionCommands(client: Client, interaction: CommandInteraction) {
  switch(interaction.commandName) {
    /* ping */
    case "ping":
      await SlashCommandsFactory.handlePingInteraction(interaction);
      break;
  }
}

export { initializeInteractionCommands, handleInteractionCommands };
