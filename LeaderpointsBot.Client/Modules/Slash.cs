// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LeaderpointsBot.Client.Actions;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Modules;

public static class Slash
{
	public class LinkSlashModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /link [osuid]
		[EnabledInDm(true)]
		[SlashCommand("link", "Links your Discord user to an osu! user.", runMode: RunMode.Async)]
		public async Task LinkUserCommand([Summary("osuid", "osu! user ID to be linked.")] int osuId)
		{
			Log.WriteInfo($"Linking user {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id}) to osu! user ID {osuId}.");
			await Context.Interaction.DeferAsync();

			ReturnMessage response;

			if (!Context.Interaction.IsDMInteraction)
			{
				Log.WriteVerbose("Interaction sent from server.");
				response = await User.LinkUser(Context.User, osuId, Context.Guild);
			}
			else
			{
				Log.WriteVerbose("Interaction sent from direct message.");
				response = await User.LinkUser(Context.User, osuId);
			}

			Log.WriteInfo("Link success. Sending embed response.");
			await Reply.SendToInteractionContextAsync(Context, response);
		}
	}

	public class PingSlashModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /ping
		[EnabledInDm(true)]
		[SlashCommand("ping", "Pings the bot.", runMode: RunMode.Async)]
		public async Task SendPingCommand()
		{
			Log.WriteInfo("Sending ping message.");
			await Context.Interaction.DeferAsync();

			ReturnMessage response = Help.GetPingMessage(Context.Client);
			await Reply.SendToInteractionContextAsync(Context, response);
		}
	}

	public class CountSlashModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /count [osuusername?]
		[EnabledInDm(true)]
		[SlashCommand("count", "Calculates points based on leaderboard count.", runMode: RunMode.Async)]
		public async Task CountPointsCommand([Summary("osuuser", "osu! username to be calculated.")] string? osuUsername = null)
		{
			if (string.IsNullOrEmpty(osuUsername))
			{
				Log.WriteInfo($"Calculating points for {Context.User.Username}#{Context.User.Discriminator}.");
			}
			else
			{
				Log.WriteInfo($"Calculating points for osu! user {osuUsername}.");
			}

			await Context.Interaction.DeferAsync();

			ReturnMessage[] responses;

			if (!Context.Interaction.IsDMInteraction)
			{
				if (!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Commands.Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername, Context.Guild);
				}
				else
				{
					responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString(), Context.Guild);
				}
			}
			else
			{
				Log.WriteInfo("Command invoked from direct message. This will ignore update actions.");

				if (!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Commands.Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername);
				}
				else
				{
					responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString());
				}
			}

			Log.WriteVerbose("Points calculated successfully. Sending responses.");
			await Reply.SendToInteractionContextAsync(Context, responses);
		}

		// /whatif [pointsargs]
		[EnabledInDm(true)]
		[SlashCommand("whatif", "Calculates what-if points.", runMode: RunMode.Async)]
		public async Task WhatIfPointsCommand([Summary("parameters", "Arguments for what-if count. See help for details.")] string pointsArgs)
		{
			Log.WriteInfo($"Calculating what-if points for {Context.User.Username}#{Context.User.Discriminator} ({pointsArgs}).");
			await Context.Interaction.DeferAsync();

			ReturnMessage[] responses = await Commands.Counter.WhatIfUserCount(Context.User.Id.ToString(), pointsArgs, Context.Guild.Id.ToString());

			Log.WriteVerbose("What-if calculated successfully. Sending responses.");
			await Reply.SendToInteractionContextAsync(Context, responses);
		}
	}

	public class LeaderboardSlashModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /serverleaderboard
		[EnabledInDm(false)]
		[SlashCommand("serverleaderboard", "Returns server points leaderboard.", runMode: RunMode.Async)]
		public async Task SendServerLeaderboardCommand()
		{
			if (Context.Interaction.IsDMInteraction)
			{
				await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
				return;
			}

			Log.WriteInfo($"Retrieving server points leaderboard (guild ID {Context.Guild.Id}).");
			await Context.Interaction.DeferAsync();

			ReturnMessage response = await Leaderboard.GetServerLeaderboard(Context.Guild.Id.ToString());

			Log.WriteVerbose("Leaderboard retrieved successfully. Sending embed response.");
			await Reply.SendToInteractionContextAsync(Context, response);
		}
	}

	public class HelpModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /help
		[EnabledInDm(true)]
		[SlashCommand("help", "Returns all commands usage help.", runMode: RunMode.Async)]
		public async Task SendHelpCommand()
		{
			Log.WriteInfo($"Sending commands usage help message.");
			await Context.Interaction.DeferAsync();

			ReturnMessage response = Help.GetBotHelpMessage(Context.Client, true);
			await Reply.SendToInteractionContextAsync(Context, response);
		}
	}

	[EnabledInDm(false)]
	[RequireUserPermission(GuildPermission.Administrator)]
	[Group("config", "Server configuration commands.")]
	public class ConfigurationSlashModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /config show
		[EnabledInDm(false)]
		[SlashCommand("show", "Returns current server configuration. Only available for server administrators.", runMode: RunMode.Async)]
		public async Task ShowConfigurationCommand()
		{
			if (Context.Interaction.IsDMInteraction)
			{
				await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
				return;
			}

			Log.WriteInfo($"Retrieving server configuration data (guild ID {Context.Guild.Id}).");
			await Context.Interaction.DeferAsync();

			ReturnMessage response = await Configuration.GetGuildConfigurationAsync(Context.Guild);

			Log.WriteVerbose("Server data fetched. Returning configuration embed message.");
			await Reply.SendToInteractionContextAsync(Context, response);
		}

		// /config help
		[EnabledInDm(false)]
		[SlashCommand("help", "Returns server configuration commands usage help. Only available for server administrators.", runMode: RunMode.Async)]
		public async Task SendHelpConfigurationCommand()
		{
			if (Context.Interaction.IsDMInteraction)
			{
				await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
				return;
			}

			Log.WriteInfo($"Sending server configuration commands help message (guild ID {Context.Guild.Id}).");
			await Context.Interaction.DeferAsync();

			ReturnMessage response = Help.GetConfigHelpMessage(Context.Client, true);
			await Reply.SendToInteractionContextAsync(Context, response);
		}

		[EnabledInDm(false)]
		[Group("set", "Server configuration setter commands.")]
		public class ConfigurationSetterSlashModule : InteractionModuleBase<SocketInteractionContext>
		{
			// /config set country
			[EnabledInDm(false)]
			[SlashCommand("country", "Sets country restriction for this server. Leave option empty to disable.", runMode: RunMode.Async)]
			public async Task SetServerCountryCommand([Summary("code", "2-letter country code. Leave empty to disable.")] string? countryCode = null)
			{
				if (Context.Interaction.IsDMInteraction)
				{
					await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
					return;
				}

				if (!string.IsNullOrEmpty(countryCode))
				{
					Log.WriteInfo($"Setting server country restriction to {countryCode} (guild ID {Context.Guild.Id}).");
				}
				else
				{
					Log.WriteInfo($"Disabling server country restriction (guild ID {Context.Guild.Id}).");
				}

				await Context.Interaction.DeferAsync();

				ReturnMessage response = await Configuration.SetGuildCountryConfigurationAsync(Context.Guild, countryCode);

				Log.WriteVerbose("Server country configuration set. Sending result message.");
				await Reply.SendToInteractionContextAsync(Context, response);
			}

			// /config set verifiedrole
			[EnabledInDm(false)]
			[SlashCommand("verifiedrole", "Sets verified user role, see commands help for details. Leave option empty to disable.", runMode: RunMode.Async)]
			public async Task SetServerVerifiedRoleCommand([Summary("role", "Role for verified users. Leave empty to disable.")] SocketRole? role = null)
			{
				if (Context.Interaction.IsDMInteraction)
				{
					await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
					return;
				}

				if (role != null)
				{
					Log.WriteInfo($"Setting verified user role to {role.Id} (guild ID {Context.Guild.Id}).");
				}
				else
				{
					Log.WriteInfo($"Disabling verified user role (guild ID {Context.Guild.Id}).");
				}

				await Context.Interaction.DeferAsync();

				ReturnMessage response = await Configuration.SetGuildVerifiedRoleConfigurationAsync(Context.Guild, role);

				Log.WriteVerbose("Server verified role configuration set. Sending result message.");
				await Reply.SendToInteractionContextAsync(Context, response);
			}

			// /config set commandschannel
			[EnabledInDm(false)]
			[SlashCommand("commandschannel", "Sets server command channel restriction. Leave option empty to disable.", runMode: RunMode.Async)]
			public async Task SetServerCommandsChannelCommand([Summary("channel", "Channel for commands restriction. Leave empty to disable.")] SocketTextChannel? channel = null)
			{
				if (Context.Interaction.IsDMInteraction)
				{
					await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
					return;
				}

				if (channel != null)
				{
					Log.WriteInfo($"Setting commands channel to {channel.Id} (guild ID {Context.Guild.Id}).");
				}
				else
				{
					Log.WriteInfo($"Disabling commands channel restriction (guild ID {Context.Guild.Id}).");
				}

				await Context.Interaction.DeferAsync();

				ReturnMessage response = await Configuration.SetGuildCommandsChannelConfigurationAsync(Context.Guild, channel);

				Log.WriteVerbose("Server commands channel restriction configuration set. Sending result message.");
				await Reply.SendToInteractionContextAsync(Context, response);
			}

			// /config set leaderboardschannel
			[EnabledInDm(false)]
			[SlashCommand("leaderboardschannel", "Sets server leaderboard command channel restriction. Leave option empty to disable.", runMode: RunMode.Async)]
			public async Task SetServerLeaderboardsChannelCommand([Summary("channel", "Channel for leaderboard command restriction. Leave empty to disable.")] SocketTextChannel? channel = null)
			{
				if (Context.Interaction.IsDMInteraction)
				{
					await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
					return;
				}

				if (channel != null)
				{
					Log.WriteInfo($"Setting leaderboard commands channel to {channel.Id} (guild ID {Context.Guild.Id}).");
				}
				else
				{
					Log.WriteInfo($"Disabling leaderboard commands channel restriction (guild ID {Context.Guild.Id}).");
				}

				await Context.Interaction.DeferAsync();

				ReturnMessage response = await Configuration.SetGuildLeaderboardsChannelConfigurationAsync(Context.Guild, channel);

				Log.WriteVerbose("Server leaderboards channel restriction configuration set. Sending result message.");
				await Reply.SendToInteractionContextAsync(Context, response, modifyResponse: true);
			}
		}

		[EnabledInDm(false)]
		[Group("guildrole", "Guild role commands.")]
		public class ConfigurationGuildRoleModule : InteractionModuleBase<SocketInteractionContext>
		{
			// /config guildrole show
			[EnabledInDm(false)]
			[SlashCommand("show", "Returns server role configuration.", runMode: RunMode.Async)]
			public async Task ShowGuildRolePointsCommand()
			{
				Log.WriteInfo($"Retrieving server role configuration data (guild ID {Context.Guild.Id}).");
				await Context.Interaction.DeferAsync();

				ReturnMessage response = await Configuration.GetGuildRolePointsAsync(Context.Guild);

				Log.WriteVerbose("Server role configuration data fetched. Sending configuration embed message.");
				await Reply.SendToInteractionContextAsync(Context, response);
			}

			// /config guildrole add
			[EnabledInDm(false)]
			[SlashCommand("add", "Adds server role configuration for achieving certain points.", runMode: RunMode.Async)]
			public async Task AddGuildRolePointsCommand([Summary("role", "Target role.")] SocketRole role, [Summary("minpoints", "Minimum points for target role (> 0).")] int minPoints)
			{
				if (Context.Interaction.IsDMInteraction)
				{
					await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
					return;
				}

				Log.WriteInfo($"Inserting role ({role.Id}, {minPoints} pts.) (guild ID {Context.Guild.Id})");
				await Context.Interaction.DeferAsync();

				ReturnMessage response = await Configuration.AddGuildRolePointsConfigurationAsync(Context.Guild, role, minPoints);

				Log.WriteVerbose("Server role inserted. Sending result message.");
				await Reply.SendToInteractionContextAsync(Context, response);
			}

			// /config guildrole remove
			[EnabledInDm(false)]
			[SlashCommand("remove", "Removes server role configuration for achieving certain points.", runMode: RunMode.Async)]
			public async Task RemoveGuildRolePointsCommand([Summary("role", "Target role.")] SocketRole role)
			{
				if (Context.Interaction.IsDMInteraction)
				{
					await Context.Interaction.RespondAsync("This command is usable on servers.", ephemeral: true);
					return;
				}

				Log.WriteInfo($"Removing role ({role.Id}) (guild ID {Context.Guild.Id})");
				await Context.Channel.TriggerTypingAsync();

				ReturnMessage response = await Configuration.RemoveGuildRolePointsConfigurationAsync(Context.Guild, role);

				Log.WriteVerbose("Server role removed. Sending result message.");
				await Reply.SendToInteractionContextAsync(Context, response);
			}
		}
	}
}
