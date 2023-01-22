using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public static class InteractionModules
{
	public static class LinkSlashModule
	{
		// /link [osuid]
		public static async Task LinkUserCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			long osuId = (long)cmd.Data.Options.First().Value;

			await Log.WriteInfo("LinkUserCommand", $"Linking user { cmd.User.Username }#{ cmd.User.Discriminator } ({ cmd.User.Id }) to osu! user ID { osuId }.");
			await cmd.DeferAsync();
		}
	}

	public static class PingSlashModule
	{
		// /ping
		public static async Task SendPingCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			await Log.WriteInfo("SendPingCommand", "Sending ping message.");
			await cmd.DeferAsync();

			string replyMsg = CommandsFactory.GetPingMessage(client);
			await cmd.ModifyOriginalResponseAsync(msg => msg.Content = replyMsg);
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
				await Log.WriteInfo("CountPointsCommand", $"Calculating points for osu! user { osuUsername }.");
			}
			catch (InvalidOperationException)
			{
				await Log.WriteDebug("CountPointsCommand", $"cmd.Data.Options.First() contains no element. Calculating user's points instead.");
				await Log.WriteInfo("CountPointsCommand", $"Calculating points for { cmd.User.Username }#{ cmd.User.Discriminator }.");
			}
			catch (Exception e)
			{
				await Log.WriteError("CountPointsCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
				await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
				return;
			}

			await cmd.DeferAsync();

			SocketGuildChannel? guildChannel = cmd.Channel as SocketGuildChannel;
			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses;

			if(guildChannel != null)
			{
				if(!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Commands.CountModule.CountLeaderboardPointsByOsuUsernameAsync(osuUsername, guildChannel.Guild);
				}
				else
				{
					responses = await Commands.CountModule.CountLeaderboardPointsByDiscordUserAsync(cmd.User.Id.ToString(), client.CurrentUser.Id.ToString(), guildChannel.Guild);
				}
			}
			else
			{
				await Log.WriteInfo("CountPointsCommand", "Command invoked from direct message. This will ignore update actions.");

				if(!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Commands.CountModule.CountLeaderboardPointsByOsuUsernameAsync(osuUsername);
				}
				else
				{
					responses = await Commands.CountModule.CountLeaderboardPointsByDiscordUserAsync(cmd.User.Id.ToString(), client.CurrentUser.Id.ToString());
				}
			}

			RestInteractionMessage? replyMsg = null;
			foreach(Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if(response.MessageType == Common.ResponseMessageType.EMBED)
				{
					if(replyMsg == null)
					{
						replyMsg = await cmd.ModifyOriginalResponseAsync(msg => {
							msg.Content = "";
							msg.Embed = response.GetEmbed();
						});
					}
					else
					{
						await replyMsg.Channel.SendMessageAsync(embed: response.GetEmbed());
					}
				}
				else if(response.MessageType == Common.ResponseMessageType.TEXT)
				{
					if(replyMsg == null)
					{
						replyMsg = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = response.GetString());
					}
					else
					{
						await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
				else if(response.MessageType == Common.ResponseMessageType.ERROR)
				{
					if(replyMsg == null)
					{
						replyMsg = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = $"**Error:** { response.GetString() }");
					}
					else
					{
						await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
			}
		}

		// /whatif [pointsargs]
		public static async Task WhatIfPointsCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			string pointsArgs = (string)cmd.Data.Options.First().Value;

			await Log.WriteInfo("WhatIfPointsCommand", $"Calculating what-if points for { cmd.User.Username }#{ cmd.User.Discriminator } ({ pointsArgs }).");
			await cmd.DeferAsync();
		}
	}

	public static class CountContextModule
	{
		// user context -> Calculate points
		public static async Task CountPointsCommand(DiscordSocketClient client, SocketUserCommand cmd)
		{
			SocketUser user = cmd.Data.Member;

			await Log.WriteInfo("CountPointsCommand", $"Calculating points for { user.Username }#{ user.Discriminator }.");
			await cmd.DeferAsync();
		}
	}
	
	public static class LeaderboardSlashModule
	{
		// /serverleaderboard
		public static async Task SendServerLeaderboardCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if(cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			await Log.WriteInfo("SendServerLeaderboardCommand", $"Retrieving server points leaderboard (guild ID { guildChannel.Guild.Id }).");
			await cmd.DeferAsync();
		}
	}

	public static class HelpModule
	{
		// /help
		public static async Task SendHelpCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			await Log.WriteInfo("SendHelpCommand", $"Sending commands usage help message.");
			await cmd.DeferAsync();

			Embed replyEmbed = CommandsFactory.GetBotHelpMessage(client, true);

			await cmd.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
		}
	}

	public static class ConfigurationSlashModule
	{
		// /config show
		public static async Task ShowConfigurationCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if(cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			await Log.WriteInfo("ShowConfigurationCommand", $"Retrieving server configuration data (guild ID { guildChannel.Guild.Id }).");
			await cmd.DeferAsync();
		}

		// /config help
		public static async Task SendHelpConfigurationCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if(cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			await Log.WriteInfo("SendHelpConfigurationCommand", $"Sending server configuration commands help message (guild ID { guildChannel.Guild.Id }).");
			await cmd.DeferAsync();

			Embed replyEmbed = CommandsFactory.GetConfigHelpMessage(client, true);

			await cmd.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
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
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				string? countryCode = null;

				try
				{
					countryCode = (string)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerCountryCommand", $"Setting server country restriction to { countryCode } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException)
				{
					await Log.WriteDebug("SetServerCountryCommand", $"cmd.Data.Options.First() contains no element. Disabling server country restriction.");
					await Log.WriteInfo("SetServerCountryCommand", $"Disabling server country restriction (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("CountPointsCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}

			// /config set verifiedrole
			public static async Task SetServerVerifiedRoleCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				SocketRole? role = null;

				try
				{
					role = (SocketRole)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Setting verified user role to { role.Id } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException)
				{
					await Log.WriteDebug("SetServerVerifiedRoleCommand", $"cmd.Data.Options.First() contains no element. Disabling verified user role.");
					await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Disabling verified user role (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("SetServerVerifiedRoleCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $"Exception details below.\n{ e }" : "") }");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}

			// /config set commandschannel
			public static async Task SetServerCommandsChannelCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				SocketChannel? channel = null;

				try
				{
					channel = (SocketChannel)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerCommandsChannelCommand", $"Setting commands channel to { channel.Id } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException)
				{
					await Log.WriteDebug("SetServerCommandsChannelCommand", $"cmd.Data.Options.First() contains no element. Disabling command channel restriction.");
					await Log.WriteInfo("SetServerCommandsChannelCommand", $"Disabling command channel restriction (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("SetServerCommandsChannelCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}

			// /config set leaderboardschannel
			public static async Task SetServerLeaderboardsChannelCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}

				SocketChannel? channel = null;

				try
				{
					channel = (SocketChannel)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Setting leaderboard commands channel to { channel.Id } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException)
				{
					await Log.WriteDebug("SetServerLeaderboardsChannelCommand", $"cmd.Data.Options.First() contains no element. Disabling leaderboards command channel restriction.");
					await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Disabling leaderboard commands channel restriction (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("SetServerLeaderboardsChannelCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
					await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}

				await cmd.DeferAsync();
			}
		}
	}
}
