// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public static class SlashCommandsFactory
{
	public static SlashCommandBuilder[] SlashCommands { get; } =
	{
		new SlashCommandBuilder().WithName("ping")
			.WithDescription("Pings the bot."),
		new SlashCommandBuilder().WithName("count")
			.WithDescription("Calculates points based on leaderboard count.")
			.AddOption("osuuser", ApplicationCommandOptionType.String, "osu! username to be calculated.", false),
		new SlashCommandBuilder().WithName("serverleaderboard")
			.WithDescription("Returns server points leaderboard.")
			.WithDMPermission(false),
		new SlashCommandBuilder().WithName("link")
			.WithDescription("Links your Discord user to an osu! user."),
		new SlashCommandBuilder().WithName("help")
			.WithDescription("Returns all commands usage help."),
		new SlashCommandBuilder().WithName("config")
			.WithDescription("Configuration commands.")
			.AddOption(new SlashCommandOptionBuilder().WithName("help")
				.WithDescription("Returns all server configuration commands help. Only available for server administrators.")
				.WithType(ApplicationCommandOptionType.SubCommand))
			.AddOption(new SlashCommandOptionBuilder().WithName("show")
				.WithDescription("Returns current server configuration. Only available for server administrators.")
				.WithType(ApplicationCommandOptionType.SubCommand))
			.AddOption(new SlashCommandOptionBuilder().WithName("set")
				.WithDescription("Configuration setter commands.")
				.WithType(ApplicationCommandOptionType.SubCommandGroup)
				.AddOption(new SlashCommandOptionBuilder().WithName("country")
					.WithDescription("Sets country restriction for this server. Leave empty to disable.")
					.WithType(ApplicationCommandOptionType.SubCommand)
					.AddOption("code", ApplicationCommandOptionType.String, "2-letter country code. Leave empty to disable.", false))
				.AddOption(new SlashCommandOptionBuilder().WithName("verifiedrole")
					.WithDescription("Sets verified user role, see commands help for details. Leave role option empty to disable.")
					.WithType(ApplicationCommandOptionType.SubCommand)
					.AddOption("role", ApplicationCommandOptionType.Role, "Role for verified users. Leave empty to disable.", false))
				.AddOption(new SlashCommandOptionBuilder().WithName("commandschannel")
					.WithDescription("Sets server command channel restriction. Leave channel option empty to disable.")
					.WithType(ApplicationCommandOptionType.SubCommand)
					.AddOption("channel", ApplicationCommandOptionType.Channel, "Channel for commands restriction. Leave empty to disable.", false))
				.AddOption(new SlashCommandOptionBuilder().WithName("leaderboardschannel")
					.WithDescription("Sets server leaderboard command channel restriction. Leave channel option empty to disable.")
					.WithType(ApplicationCommandOptionType.SubCommand)
					.AddOption("channel", ApplicationCommandOptionType.Channel, "Channel for leaderboard command restriction. Leave empty to disable.", false)))
			.WithDMPermission(false)
	};

	public static async Task CreateSlashCommands(DiscordSocketClient client)
	{
		await Log.WriteVerbose("CreateSlashCommands", "Iterating slash commands array.");

		int slashCommandsCount = SlashCommands.Length;
		for (int i = 0; i < slashCommandsCount; i++)
		{
			await Log.WriteInfo("CreateSlashCommands", $"Creating slash commands ({i + 1}/{slashCommandsCount})...");

			await Log.WriteVerbose("CreateSlashCommands", $"Creating command (index {i}) on client.");
			await client.CreateGlobalApplicationCommandAsync(SlashCommands[i].Build());

			if (i < slashCommandsCount - 1)
			{
				// logging level is not verbose or debug
				if (Settings.Instance.Client.Logging.LogSeverity < 4)
				{
					await Log.DeletePreviousLine();
				}
			}
			else
			{
				await Log.WriteInfo("CreateSlashCommands", "Slash commands created.");
			}
		}
	}
}
