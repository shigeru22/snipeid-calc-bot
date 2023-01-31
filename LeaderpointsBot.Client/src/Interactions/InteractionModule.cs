// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public static class InteractionModule
{
	public static class LinkSlashModule
	{
		// /link [osuid]
		public static async Task LinkUserCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			long osuId = (long)cmd.Data.Options.First().Value;

			Log.WriteInfo($"Linking user {cmd.User.Username}#{cmd.User.Discriminator} ({cmd.User.Id}) to osu! user ID {osuId}.");
			await cmd.DeferAsync();

			Embed replyEmbed;

			if (cmd.Channel is SocketGuildChannel guildChannel)
			{
				Log.WriteVerbose("Interaction sent from server.");
				replyEmbed = await User.LinkUser(cmd.User, (int)osuId, guildChannel.Guild);
			}
			else
			{
				Log.WriteVerbose("Interaction sent from direct message.");
				replyEmbed = await User.LinkUser(cmd.User, (int)osuId);
			}

			Log.WriteInfo("Link success. Sending embed response.");

			_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
		}
	}

	public static class PingSlashModule
	{
		// /ping
		public static async Task SendPingCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			Log.WriteInfo("Sending ping message.");
			await cmd.DeferAsync();

			string replyMsg = Help.GetPingMessage(client);
			_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = replyMsg);
		}
	}

	public static class CountSlashModule
	{
		// /count [osuusername?]
		public static async Task CountPointsCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			string? osuUsername = null;

			try
			{
				osuUsername = (string)cmd.Data.Options.First().Value;
				Log.WriteInfo($"Calculating points for osu! user {osuUsername}.");
			}
			catch (InvalidOperationException)
			{
				Log.WriteDebug($"cmd.Data.Options.First() contains no element. Calculating user's points instead.");
				Log.WriteInfo($"Calculating points for {cmd.User.Username}#{cmd.User.Discriminator}.");
			}
			catch (Exception e)
			{
				Log.WriteError($"Unhandled exception occurred while retrieving options value.{(Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{e}" : string.Empty)}");
				await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
				return;
			}

			await cmd.DeferAsync();

			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses;

			if (cmd.Channel is SocketGuildChannel guildChannel)
			{
				if (!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Commands.Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername, guildChannel.Guild);
				}
				else
				{
					responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(cmd.User.Id.ToString(), client.CurrentUser.Id.ToString(), guildChannel.Guild);
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
					responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(cmd.User.Id.ToString(), client.CurrentUser.Id.ToString());
				}
			}

			RestInteractionMessage? replyMsg = null;
			foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (replyMsg == null)
					{
						replyMsg = await cmd.ModifyOriginalResponseAsync(msg =>
						{
							msg.Content = string.Empty;
							msg.Embed = response.GetEmbed();
						});
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(embed: response.GetEmbed());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Text)
				{
					if (replyMsg == null)
					{
						replyMsg = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = response.GetString());
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Error)
				{
					if (replyMsg == null)
					{
						replyMsg = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = $"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
			}
		}

		// /whatif [pointsargs]
		public static async Task WhatIfPointsCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			string pointsArgs = (string)cmd.Data.Options.First().Value;

			Log.WriteInfo($"Calculating what-if points for {cmd.User.Username}#{cmd.User.Discriminator} ({pointsArgs}).");
			await cmd.DeferAsync();
		}
	}

	public static class CountContextModule
	{
		// user context -> Calculate points
		public static async Task CountPointsCommand(DiscordSocketClient client, SocketUserCommand cmd)
		{
			SocketUser user = cmd.Data.Member;

			Log.WriteInfo($"Calculating points for {user.Username}#{user.Discriminator}.");
			await cmd.DeferAsync();
		}
	}

	public static class LeaderboardSlashModule
	{
		// /serverleaderboard
		public static async Task SendServerLeaderboardCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if (cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			Log.WriteInfo($"Retrieving server points leaderboard (guild ID {guildChannel.Guild.Id}).");
			await cmd.DeferAsync();
		}
	}

	public static class HelpModule
	{
		// /help
		public static async Task SendHelpCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			Log.WriteInfo($"Sending commands usage help message.");
			await cmd.DeferAsync();

			Embed replyEmbed = Help.GetBotHelpMessage(client, true);

			_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
		}
	}

	public static class ConfigurationSlashModule
	{
		// /config show
		public static async Task ShowConfigurationCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if (cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			Log.WriteInfo($"Retrieving server configuration data (guild ID {guildChannel.Guild.Id}).");
			await cmd.DeferAsync();
		}

		// /config help
		public static async Task SendHelpConfigurationCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if (cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			Log.WriteInfo($"Sending server configuration commands help message (guild ID {guildChannel.Guild.Id}).");
			await cmd.DeferAsync();

			Embed replyEmbed = Help.GetConfigHelpMessage(client, true);

			_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
		}

		public static class ConfigurationSetterSlashModule
		{
			/*
			 * since we're handling subcommand group's subcommand,
			 * third option will be used. see mapping below.
			 *
			 * command
			 *  > name = "config"
			 *  > options (subcommand group)
			 *     > name = "set"
			 *     > options (subcommand)
			 *        > name = "country", "verifiedrole", etc.
			 *        > options (parameter value)
			 *           > value
			 *
			 * hence, cmd.Data.Options.First().Options.First().Options.First().Value
			 * should contain option value.
			 */

			// /config set country
			public static async Task SetServerCountryCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if (cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				string? countryCode = null;

				try
				{
					countryCode = (string)cmd.Data.Options.First().Options.First().Options.First().Value;
					Log.WriteInfo($"Setting server country restriction to {countryCode} (guild ID {guildChannel.Guild.Id}).");
				}
				catch (InvalidOperationException)
				{
					Log.WriteDebug($"cmd.Data.Options.First() contains no element. Disabling server country restriction.");
					Log.WriteInfo($"Disabling server country restriction (guild ID {guildChannel.Guild.Id}).");
				}
				catch (Exception e)
				{
					Log.WriteError($"Unhandled exception occurred while retrieving options value.{(Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{e}" : string.Empty)}");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}

			// /config set verifiedrole
			public static async Task SetServerVerifiedRoleCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if (cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				SocketRole? role = null;

				try
				{
					role = (SocketRole)cmd.Data.Options.First().Options.First().Options.First().Value;
					Log.WriteInfo($"Setting verified user role to {role.Id} (guild ID {guildChannel.Guild.Id}).");
				}
				catch (InvalidOperationException)
				{
					Log.WriteDebug($"cmd.Data.Options.First() contains no element. Disabling verified user role.");
					Log.WriteInfo($"Disabling verified user role (guild ID {guildChannel.Guild.Id}).");
				}
				catch (Exception e)
				{
					Log.WriteError($"Unhandled exception occurred while retrieving options value.{(Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $"Exception details below.\n{e}" : string.Empty)}");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}

			// /config set commandschannel
			public static async Task SetServerCommandsChannelCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if (cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				SocketChannel? channel = null;

				try
				{
					channel = (SocketChannel)cmd.Data.Options.First().Options.First().Options.First().Value;
					Log.WriteInfo($"Setting commands channel to {channel.Id} (guild ID {guildChannel.Guild.Id}).");
				}
				catch (InvalidOperationException)
				{
					Log.WriteDebug($"cmd.Data.Options.First() contains no element. Disabling command channel restriction.");
					Log.WriteInfo($"Disabling command channel restriction (guild ID {guildChannel.Guild.Id}).");
				}
				catch (Exception e)
				{
					Log.WriteError($"Unhandled exception occurred while retrieving options value.{(Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{e}" : string.Empty)}");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}

			// /config set leaderboardschannel
			public static async Task SetServerLeaderboardsChannelCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if (cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				SocketChannel? channel = null;

				try
				{
					channel = (SocketChannel)cmd.Data.Options.First().Options.First().Options.First().Value;
					Log.WriteInfo($"Setting leaderboard commands channel to {channel.Id} (guild ID {guildChannel.Guild.Id}).");
				}
				catch (InvalidOperationException)
				{
					Log.WriteDebug($"cmd.Data.Options.First() contains no element. Disabling leaderboards command channel restriction.");
					Log.WriteInfo($"Disabling leaderboard commands channel restriction (guild ID {guildChannel.Guild.Id}).");
				}
				catch (Exception e)
				{
					Log.WriteError($"Unhandled exception occurred while retrieving options value.{(Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{e}" : string.Empty)}");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}
		}
	}
}
