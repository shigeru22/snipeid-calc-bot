import { Client, CommandInteraction, REST, Routes } from "discord.js";
import { Environment } from "../../utils";
import SlashCommandsFactory from "./slash";

async function initializeInteractionCommands() {
  const commandsRestData = [ ...SlashCommandsFactory.buildRestData() ];
  const rest = new REST({ version: "10" }).setToken(Environment.getBotToken());

  await rest.put(
    Routes.applicationCommands(Environment.getBotClientId()),
    { body: commandsRestData }
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
