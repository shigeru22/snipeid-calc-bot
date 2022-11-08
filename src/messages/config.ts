import { MessageEmbed } from "discord.js";

/**
 * Creates all available config commands embed.
 *
 * @returns { MessageEmbed } MessageEmbed object with syntax commands help data.
 */
function createConfigCommandsEmbed(): MessageEmbed {
  const temp = new MessageEmbed();

  temp.setTitle("Configuration commands list:");
  temp.setDescription("All configuration commands are only available to server administrators.\nFor all `set` commands, set argument as `-` to disable.\n\n`show`\nRetrieves current server configuration.\n\n`setcountry [string]`\nEnables country-restricted mode.\n`[string]` must be a 2-letter country code. Example: `ID`\n\n`setverifiedrole [role]`\nSets verified role for linked user.\nThis role is also granted if the linked user, either new or old, joins the server.\n\n`setcommandschannel [channel]`\nRestricts count commands to be called from specified channel only (including Bathbot response).\n\n`setleaderboardschannel [channel]`\nRestricts leaderboard command to be called from specified channel only.\n\n**Note:**\n`[role]` and `[channel]` may be specified by mentioning or entering its respective Snowflake ID.\n\n`help`\nShows this help.");
  temp.setFooter({ text: "osu-leaderpoints-bot" }); // TODO: add version information here

  return temp;
}

function createServerConfigurationEmbed(serverName: string, serverIconUrl: string | null, country: string | null, verifiedRole: string | null, commandsChannel: string | null, leaderboardsChannel: string | null): MessageEmbed {
  const temp = new MessageEmbed();

  temp.setTitle("Server configuration:");
  temp.setDescription(`\`\`\`\nCountry             : ${ country !== null ? country : "(disabled)" }\nVerified role       : ${ verifiedRole !== null ? verifiedRole : "(disabled)" }\nCommands channel    : ${ commandsChannel !== null ? commandsChannel : "(disabled)" }\nLeaderboards channel: ${ leaderboardsChannel !== null ? leaderboardsChannel : "(disabled)" }\n\`\`\``);
  temp.setFooter({
    text: serverName,
    iconURL: serverIconUrl !== null ? serverIconUrl : undefined
  });

  return temp;
}

export { createConfigCommandsEmbed, createServerConfigurationEmbed };
