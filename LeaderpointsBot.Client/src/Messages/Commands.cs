using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Messages;

public static class MessageCommands
{
	public class LinkModule : ModuleBase<SocketCommandContext>
	{
		// @bot link
		[Command("link")]
		[Summary("Links your Discord user to an osu! user.")]
		public async Task LinkUserCommand([Summary("osu! user ID.")] int osuId)
		{
			await Log.WriteInfo("LinkUserCommand", $"Linking user { Context.User.Username }#{ Context.User.Discriminator } ({ Context.User.Id }) to osu! user ID { osuId }.");
		}
	}

	public class PingModule : ModuleBase<SocketCommandContext>
	{
		// @bot ping
		[Command("ping")]
		[Summary("Pings the bot.")]
		public async Task PingCommand()
		{
			await Log.WriteInfo("SendPingCommand", $"Sending ping message (guild ID {Context.Guild.Id}).");

			string replyMsg = CommandsFactory.GetPingMessage(Context.Client);

			if(Settings.Instance.Client.UseReply)
			{
				await Context.Message.ReplyAsync(replyMsg);
			}
			else
			{
				await Context.Channel.SendMessageAsync(replyMsg);
			}
		}
	}

	public class CountModule : ModuleBase<SocketCommandContext>
	{
		[Command("count")]
		[Summary("Calculates points based on leaderboard count.")]
		public async Task CountPointsCommand()
		{
			await Log.WriteInfo("CountPointsCommand", $"Calculating points for { Context.User.Username }#{ Context.User.Discriminator }.");
		}

		[Command("count")]
		[Summary("Calculates points based on leaderboard count.")]
		public async Task CountPointsCommand([Summary("osu! username.")] string osuUsername)
		{
			await Log.WriteInfo("CountPointsCommand", $"Calculating points for osu! user { osuUsername }.");
		}

		[Command("whatif")]
		[Summary("Calculates what-if points based on leaderboard count.")]
		public async Task WhatIfPointsCommand([Summary("Comma-delimited arguments representing what-if count.")] string pointsArgs)
		{
			await Log.WriteInfo("WhatIfPointsCommand", $"Calculating what-if points for { Context.User.Username }#{ Context.User.Discriminator } ({ pointsArgs }).");
		}
	}

	public class LeaderboardModule : ModuleBase<SocketCommandContext>
	{
		[Command("leaderboard")]
		[Alias("lb")]
		[Summary("Returns server points leaderboard.")]
		public async Task SendServerLeaderboardCommand()
		{
			await Log.WriteInfo("SendServerLeaderboardCommand", $"Retrieving server points leaderboard (guild ID { Context.Guild.Id }).");
		}
	}

	[Group("config")]
	[Summary("Server configuration commands.")]
	public class ConfigurationModule : ModuleBase<SocketCommandContext>
	{
		[Command("show")]
		[Summary("Returns current server configuration. Only available for server administrators.")]
		public async Task ShowConfigurationCommand()
		{
			await Log.WriteInfo("ShowConfigurationCommand", $"Retrieving server configuration data (guild ID { Context.Guild.Id }).");
		}

		[Group("set")]
		[Summary("Configuration setter commands.")]
		public class ConfigurationSetterModule : ModuleBase<SocketCommandContext>
		{
			[Command("country")]
			[Summary("Sets country restriction for this server. Leave empty to disable.")]
			public async Task SetServerCountryCommand()
			{
				await Log.WriteInfo("SetServerCountryCommand", $"Disabling server country restriction (guild ID { Context.Guild.Id }).");
			}

			[Command("country")]
			[Summary("Sets country restriction for this server. Leave empty to disable.")]
			public async Task SetServerCountryCommand([Summary("2-letter country code. Leave empty to disable.")] string countryCode)
			{
				await Log.WriteInfo("SetServerCountryCommand", $"Setting server country restriction to { countryCode } (guild ID { Context.Guild.Id }).");
			}

			[Command("verifiedrole")]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand()
			{
				await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Disabling verified user role (guild ID { Context.Guild.Id }).");
			}

			[Command("verifiedrole")]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand([Summary("Role for verified users. Leave empty to disable.")] SocketRole role)
			{
				await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Setting verified user role to { role.Id } (guild ID { Context.Guild.Id }).");
			}

			[Command("verifiedrole")]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand([Summary("Role for verified users. Leave empty to disable.")] string roleDiscordId)
			{
				await Log.WriteInfo("SetServerVerifiedRoleCommand", $"Setting verified user role to { roleDiscordId } (guild ID { Context.Guild.Id }).");
			}

			[Command("commandschannel")]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand()
			{
				await Log.WriteInfo("SetServerCommandsChannelCommand", $"Disabling command channel restriction (guild ID { Context.Guild.Id }).");
			}

			[Command("commandschannel")]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand([Summary("Channel for commands restriction. Leave empty to disable.")] SocketGuildChannel channel)
			{
				await Log.WriteInfo("SetServerCommandsChannelCommand", $"Setting commands channel to { channel.Id } (guild ID { Context.Guild.Id }).");
			}

			[Command("commandschannel")]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand([Summary("Channel for commands restriction. Leave empty to disable.")] string channelDiscordId)
			{
				await Log.WriteInfo("SetServerCommandsChannelCommand", $"Setting commands channel to { channelDiscordId } (guild ID { Context.Guild.Id }).");
			}

			[Command("leaderboardschannel")]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand()
			{
				await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Disabling leaderboard commands channel restriction (guild ID { Context.Guild.Id }).");
			}

			[Command("leaderboardschannel")]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand([Summary("Channel for leaderboard command restriction. Leave empty to disable.")] SocketGuildChannel channel)
			{
				await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Setting leaderboard commands channel to { channel.Id } (guild ID { Context.Guild.Id }).");
			}

			[Command("leaderboardschannel")]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand([Summary("Channel for leaderboard command restriction. Leave empty to disable.")] string channelDiscordId)
			{
				await Log.WriteInfo("SetServerLeaderboardsChannelCommand", $"Setting leaderboard commands channel to { channelDiscordId } (guild ID { Context.Guild.Id }).");
			}
		}
	}
}
