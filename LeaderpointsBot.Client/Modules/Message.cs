// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Client.Actions;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Modules;

public static class Message
{
	public class LinkModule : ModuleBase<SocketCommandContext>
	{
		// @bot link
		[Command("link", RunMode = RunMode.Async)]
		[Summary("Links your Discord user to an osu! user.")]
		public async Task LinkUserCommand([Summary("osu! user ID.")] int osuId)
		{
			Log.WriteInfo($"Linking user {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id}) to osu! user ID {osuId}.");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage response = await User.LinkUser(Context.User, osuId, Context.Guild);

			Log.WriteInfo("Link success. Sending embed response.");
			await Reply.SendToCommandContextAsync(Context, response);
		}
	}

	public class PingModule : ModuleBase<SocketCommandContext>
	{
		// @bot ping
		[Command("ping", RunMode = RunMode.Async)]
		[Summary("Pings the bot.")]
		public async Task SendPingCommand()
		{
			Log.WriteInfo($"Sending ping message.");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage response = Help.GetPingMessage(Context.Client);
			await Reply.SendToCommandContextAsync(Context, response);
		}
	}

	public class CountModule : ModuleBase<SocketCommandContext>
	{
		// @bot count
		[Command("count", RunMode = RunMode.Async)]
		[Summary("Calculates points based on leaderboard count.")]
		public async Task CountPointsCommand()
		{
			Log.WriteInfo($"Calculating points for {Context.User.Username}#{Context.User.Discriminator}.");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage[] responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString(), Context.Guild);

			Log.WriteVerbose("Points calculated successfully. Sending responses.");
			await Reply.SendToCommandContextAsync(Context, responses);
		}

		// @bot count [osu! username]
		[Command("count", RunMode = RunMode.Async)]
		[Summary("Calculates points based on leaderboard count.")]
		public async Task CountPointsCommand([Summary("osu! username.")] string osuUsername)
		{
			Log.WriteInfo($"Calculating points for osu! user {osuUsername}.");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage[] responses = await Commands.Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername);

			Log.WriteVerbose("Points calculated successfully. Sending responses.");
			await Reply.SendToCommandContextAsync(Context, responses);
		}

		// @bot whatif [what-if arguments, comma-delimited]
		[Command("whatif", RunMode = RunMode.Async)]
		[Summary("Calculates what-if points based on leaderboard count.")]
		public async Task WhatIfPointsCommand([Summary("Comma-delimited arguments representing what-if count.")] string pointsArgs)
		{
			Log.WriteInfo($"Calculating what-if points for {Context.User.Username}#{Context.User.Discriminator} ({pointsArgs}).");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage[] responses = await Commands.Counter.WhatIfUserCount(Context.User.Id.ToString(), pointsArgs);

			Log.WriteVerbose("What-if calculated successfully. Sending responses.");
			await Reply.SendToCommandContextAsync(Context, responses);
		}
	}

	public class LeaderboardModule : ModuleBase<SocketCommandContext>
	{
		// @bot leaderboard | @bot lb
		[Command("leaderboard", RunMode = RunMode.Async)]
		[Alias("lb")]
		[Summary("Returns server points leaderboard.")]
		public async Task SendServerLeaderboardCommand()
		{
			Log.WriteInfo($"Retrieving server points leaderboard (guild ID {Context.Guild.Id}).");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage response = await Leaderboard.GetServerLeaderboard(Context.Guild.Id.ToString());

			Log.WriteVerbose("Leaderboard retrieved successfully. Sending embed response.");
			await Reply.SendToCommandContextAsync(Context, response);
		}
	}

	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		// @bot config help
		[Command("help", RunMode = RunMode.Async)]
		[Summary("Returns commands usage help message.")]
		public async Task SendHelpCommand()
		{
			Log.WriteInfo($"Sending commands usage help message.");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage response = Help.GetBotHelpMessage(Context.Client);
			await Reply.SendToCommandContextAsync(Context, response);
		}
	}

	[Group("config")]
	[Summary("Server configuration commands.")]
	public class ConfigurationModule : ModuleBase<SocketCommandContext>
	{
		// @bot config show
		[Command("show", RunMode = RunMode.Async)]
		[Summary("Returns current server configuration. Only available for server administrators.")]
		public async Task ShowConfigurationCommand()
		{
			Log.WriteInfo($"Retrieving server configuration data (guild ID {Context.Guild.Id}).");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage response = await Configuration.GetGuildConfigurationAsync(Context.Guild);

			Log.WriteVerbose("Server data fetched. Sending configuration embed message.");
			await Reply.SendToCommandContextAsync(Context, response);
		}

		// @bot config help
		[Command("help", RunMode = RunMode.Async)]
		[Summary("Returns server configuration commands help message. Only available for server administrators.")]
		public async Task SendHelpConfigurationCommand()
		{
			Log.WriteInfo($"Sending server configuration commands help message (guild ID {Context.Guild.Id}).");
			await Context.Channel.TriggerTypingAsync();

			ReturnMessage response = Help.GetConfigHelpMessage(Context.Client);
			await Reply.SendToCommandContextAsync(Context, response);
		}

		[Group("set")]
		[Summary("Configuration setter commands.")]
		public class ConfigurationSetterModule : ModuleBase<SocketCommandContext>
		{
			// @bot config set country
			[Command("country", RunMode = RunMode.Async)]
			[Summary("Sets country restriction for this server. Leave empty to disable.")]
			public async Task SetServerCountryCommand()
			{
				Log.WriteInfo($"Disabling server country restriction (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildCountryConfigurationAsync(Context.Guild);

				Log.WriteVerbose("Server country configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set country [2-letter country code]
			[Command("country", RunMode = RunMode.Async)]
			[Summary("Sets country restriction for this server. Leave empty to disable.")]
			public async Task SetServerCountryCommand([Summary("2-letter country code. Leave empty to disable.")] string countryCode)
			{
				Log.WriteInfo($"Setting server country restriction to {countryCode} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildCountryConfigurationAsync(Context.Guild, countryCode);

				Log.WriteVerbose("Server country configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set verifiedrole
			[Command("verifiedrole", RunMode = RunMode.Async)]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand()
			{
				Log.WriteInfo($"Disabling verified user role (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildVerifiedRoleConfigurationAsync(Context.Guild);

				Log.WriteVerbose("Server verified role configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set verifiedrole [mentioned role]
			[Command("verifiedrole", RunMode = RunMode.Async)]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand([Summary("Role for verified users. Leave empty to disable.")] SocketRole role)
			{
				Log.WriteInfo($"Setting verified user role to {role.Id} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildVerifiedRoleConfigurationAsync(Context.Guild, role); // determine whether role is in server?

				Log.WriteVerbose("Server verified role configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set verifiedrole [role ID]
			[Command("verifiedrole", RunMode = RunMode.Async)]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand([Summary("Role for verified users. Leave empty to disable.")] string roleDiscordId)
			{
				Log.WriteInfo($"Setting verified user role to {roleDiscordId} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildVerifiedRoleConfigurationAsync(Context.Guild, roleDiscordId);

				Log.WriteVerbose("Server verified role configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set commandschannel
			[Command("commandschannel", RunMode = RunMode.Async)]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand()
			{
				Log.WriteInfo($"Disabling command channel restriction (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildCommandsChannelConfigurationAsync(Context.Guild);

				Log.WriteVerbose("Server commands channel restriction configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set commandschannel [mentioned channel]
			[Command("commandschannel", RunMode = RunMode.Async)]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand([Summary("Channel for commands restriction. Leave empty to disable.")] SocketGuildChannel channel)
			{
				Log.WriteInfo($"Setting commands channel to {channel.Id} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildCommandsChannelConfigurationAsync(Context.Guild, channel);

				Log.WriteVerbose("Server commands channel restriction configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set commandschannel [channel ID]
			[Command("commandschannel", RunMode = RunMode.Async)]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand([Summary("Channel for commands restriction. Leave empty to disable.")] string channelDiscordId)
			{
				Log.WriteInfo($"Setting commands channel to {channelDiscordId} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildCommandsChannelConfigurationAsync(Context.Guild, channelDiscordId);

				Log.WriteVerbose("Server commands channel restriction configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set leaderboardschannel
			[Command("leaderboardschannel", RunMode = RunMode.Async)]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand()
			{
				Log.WriteInfo($"Disabling leaderboard commands channel restriction (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildLeaderboardsChannelConfigurationAsync(Context.Guild);

				Log.WriteVerbose("Server leaderboards channel restriction configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set leaderboardschannel [mentioned channel]
			[Command("leaderboardschannel", RunMode = RunMode.Async)]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand([Summary("Channel for leaderboard command restriction. Leave empty to disable.")] SocketGuildChannel channel)
			{
				Log.WriteInfo($"Setting leaderboard commands channel to {channel.Id} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildLeaderboardsChannelConfigurationAsync(Context.Guild, channel);

				Log.WriteVerbose("Server leaderboards channel restriction configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}

			// @bot config set leaderboardschannel [channel ID]
			[Command("leaderboardschannel", RunMode = RunMode.Async)]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand([Summary("Channel for leaderboard command restriction. Leave empty to disable.")] string channelDiscordId)
			{
				Log.WriteInfo($"Setting leaderboard commands channel to {channelDiscordId} (guild ID {Context.Guild.Id}).");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.SetGuildLeaderboardsChannelConfigurationAsync(Context.Guild, channelDiscordId);

				Log.WriteVerbose("Server leaderboards channel restriction configuration set. Sending result message.");
				await Reply.SendToCommandContextAsync(Context, response);
			}
		}
	}

	public static async Task BathbotCountCommand(SocketCommandContext context)
	{
		Embed countEmbed = context.Message.Embeds.First();

		string embedUsername = Parser.ParseUsernameFromBathbotEmbedTitle(countEmbed.Title);
		Log.WriteInfo($"Calculating points for osu! user {embedUsername}.");

		ReturnMessage[] responses = await Commands.Counter.CountBathbotLeaderboardPointsAsync(countEmbed, context.Guild);

		Log.WriteVerbose("Points calculated successfully. Sending responses.");
		await Reply.SendToCommandContextAsync(context, responses);
	}
}
