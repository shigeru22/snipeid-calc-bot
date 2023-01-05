using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public static class InteractionModules
{
	public static class LinkSlashModule
	{
		// /link [osuid]
		public static async Task LinkUserCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			await cmd.DeferAsync();
			int osuId = (int)cmd.Data.Options.First().Value;

			await Log.WriteInfo("LinkUserCommand", $"Linking user { cmd.User.Username }#{ cmd.User.Discriminator } ({ cmd.User.Id }) to osu! user ID { osuId }.");
		}
	}

	public static class PingSlashModule
	{
		// /ping
		public static async Task SendPingCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			await cmd.DeferAsync();

			await Log.WriteInfo("SendPingCommand", "Sending ping message.");

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
			catch (InvalidOperationException e)
			{
				await Log.WriteDebug("CountPointsCommand", $"cmd.Data.Options.First() contains no element. Calculating user's points instead.");
				await Log.WriteInfo("CountPointsCommand", $"Calculating points for { cmd.User.Username }#{ cmd.User.Discriminator }.");
			}
			catch (Exception e)
			{
				await Log.WriteError("CountPointsCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
				cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
				return;
			}
			
			await cmd.DeferAsync();
		}

		// /whatif [pointsargs]
		public static async Task WhatIfPointsCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			await cmd.DeferAsync();
			string pointsArgs = (string)cmd.Data.Options.First().Value;

			await Log.WriteInfo("WhatIfPointsCommand", $"Calculating what-if points for { cmd.User.Username }#{ cmd.User.Discriminator } ({ pointsArgs }).");
		}
	}

	public static class CountContextModule
	{
		// user context -> Calculate points
		public static async Task CountPointsCommand(DiscordSocketClient client, SocketUserCommand cmd)
		{
			await cmd.DeferAsync();
			SocketUser user = cmd.Data.Member;
			
			await Log.WriteInfo("CountPointsCommand", $"Calculating points for { user.Username }#{ user.Discriminator }.");
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

			await cmd.DeferAsync();

			await Log.WriteInfo("SendServerLeaderboardCommand", $"Retrieving server points leaderboard (guild ID { guildChannel.Guild.Id }).");
		}
	}

	public static class HelpModule
	{
		// /help
		public static async Task SendHelpCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			await cmd.DeferAsync();
			
			await Log.WriteInfo("SendHelpCommand", $"Sending commands usage help message.");
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

			await cmd.DeferAsync();

			await Log.WriteInfo("ShowConfigurationCommand", $"Retrieving server configuration data (guild ID { guildChannel.Guild.Id }).");
		}
		
		// /config help
		public static async Task SendHelpConfigurationCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			if(cmd.Channel is not SocketGuildChannel guildChannel)
			{
				return;
			}

			await cmd.DeferAsync();

			await Log.WriteInfo("SendHelpConfigurationCommand", $"Sending server configuration commands help message (guild ID { guildChannel.Guild.Id }).");
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
	
				await cmd.DeferAsync();
				string? countryCode = null;

				try
				{
					countryCode = (string)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerCountryCommand", $"Setting server country restriction to { countryCode } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException e)
				{
					await Log.WriteDebug("SetServerCountryCommand", $"cmd.Data.Options.First() contains no element. Disabling server country restriction.");
					await Log.WriteInfo("SetServerCountryCommand", $"Disabling server country restriction (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("CountPointsCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
					cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}
			}

			// /config set verifiedrole
			public static async Task SetServerVerifiedRoleCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}
	
				await cmd.DeferAsync();
				SocketRole? role = null;

				try
				{
					role = (SocketRole)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Setting verified user role to { role.Id } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException e)
				{
					await Log.WriteDebug("SetServerVerifiedRoleCommand", $"cmd.Data.Options.First() contains no element. Disabling verified user role.");
					await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Disabling verified user role (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("SetServerVerifiedRoleCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $"Exception details below.\n{ e }" : "") }");
					cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}
			}

			// /config set commandschannel
			public static async Task SetServerCommandsChannelCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}
	
				await cmd.DeferAsync();
				SocketChannel? channel = null;

				try
				{
					channel = (SocketChannel)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerCommandsChannelCommand", $"Setting commands channel to { channel.Id } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException e)
				{
					await Log.WriteDebug("SetServerCommandsChannelCommand", $"cmd.Data.Options.First() contains no element. Disabling command channel restriction.");
					await Log.WriteInfo("SetServerCommandsChannelCommand", $"Disabling command channel restriction (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("SetServerCommandsChannelCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
					cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}
			}

			// /config set leaderboardschannel
			public static async Task SetServerLeaderboardsChannelCommand(DiscordSocketClient client, SocketSlashCommand cmd)
			{
				if(cmd.Channel is not SocketGuildChannel guildChannel)
				{
					return;
				}
	
				await cmd.DeferAsync();
				SocketChannel? channel = null;

				try
				{
					channel = (SocketChannel)cmd.Data.Options.First().Options.First().Options.First().Value;
					await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Setting leaderboard commands channel to { channel.Id } (guild ID { guildChannel.Guild.Id }).");
				}
				catch (InvalidOperationException e)
				{
					await Log.WriteDebug("SetServerLeaderboardsChannelCommand", $"cmd.Data.Options.First() contains no element. Disabling leaderboards command channel restriction.");
					await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Disabling leaderboard commands channel restriction (guild ID { guildChannel.Guild.Id }).");
				}
				catch (Exception e)
				{
					await Log.WriteError("SetServerLeaderboardsChannelCommand", $"Unhandled exception occurred while retrieving options value.{ (Settings.Instance.Client.Logging.LogSeverity >= (int)LogSeverity.Verbose ? $" Exception details below.\n{ e }" : "") }");
					cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
					return;
				}
			}
		}
	}
}
