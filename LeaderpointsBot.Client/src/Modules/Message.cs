// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Modules;

public static class Message
{
	public class LinkModule : ModuleBase<SocketCommandContext>
	{
		// @bot link
		[Command("link")]
		[Summary("Links your Discord user to an osu! user.")]
		public async Task LinkUserCommand([Summary("osu! user ID.")] int osuId)
		{
			Log.WriteInfo($"Linking user {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id}) to osu! user ID {osuId}.");

			await Context.Channel.TriggerTypingAsync();

			Embed replyEmbed = await User.LinkUser(Context.User, osuId, Context.Guild);

			Log.WriteInfo("Link success. Sending embed response.");

			if (Settings.Instance.Client.UseReply)
			{
				_ = await Context.Message.ReplyAsync(embed: replyEmbed);
			}
			else
			{
				_ = await Context.Channel.SendMessageAsync(embed: replyEmbed);
			}
		}
	}

	public class PingModule : ModuleBase<SocketCommandContext>
	{
		// @bot ping
		[Command("ping")]
		[Summary("Pings the bot.")]
		public async Task SendPingCommand()
		{
			Log.WriteInfo($"Sending ping message.");

			string replyMsg = Help.GetPingMessage(Context.Client);

			if (Settings.Instance.Client.UseReply)
			{
				_ = await Context.Message.ReplyAsync(replyMsg);
			}
			else
			{
				_ = await Context.Channel.SendMessageAsync(replyMsg);
			}
		}
	}

	public class CountModule : ModuleBase<SocketCommandContext>
	{
		// @bot count
		[Command("count")]
		[Summary("Calculates points based on leaderboard count.")]
		public async Task CountPointsCommand()
		{
			Log.WriteInfo($"Calculating points for {Context.User.Username}#{Context.User.Discriminator}.");

			await Context.Channel.TriggerTypingAsync();

			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString(), Context.Guild);

			foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync(embed: response.GetEmbed());
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync(embed: response.GetEmbed());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Text)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync(response.GetString());
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync(response.GetString());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Error)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync($"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync($"**Error:** {response.GetString()}");
					}
				}
			}
		}

		// @bot count [osu! username]
		[Command("count")]
		[Summary("Calculates points based on leaderboard count.")]
		public async Task CountPointsCommand([Summary("osu! username.")] string osuUsername)
		{
			Log.WriteInfo($"Calculating points for osu! user {osuUsername}.");

			await Context.Channel.TriggerTypingAsync();

			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses = await Commands.Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername);

			foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync(embed: response.GetEmbed());
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync(embed: response.GetEmbed());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Text)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync(response.GetString());
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync(response.GetString());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Error)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync($"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync($"**Error:** {response.GetString()}");
					}
				}
			}
		}

		// @bot whatif [what-if arguments, comma-delimited]
		[Command("whatif")]
		[Summary("Calculates what-if points based on leaderboard count.")]
		public async Task WhatIfPointsCommand([Summary("Comma-delimited arguments representing what-if count.")] string pointsArgs)
		{
			Log.WriteInfo($"Calculating what-if points for {Context.User.Username}#{Context.User.Discriminator} ({pointsArgs}).");

			await Context.Channel.TriggerTypingAsync();

			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses = await Commands.Counter.WhatIfUserCount(Context.User.Id.ToString(), pointsArgs);

			foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync(embed: response.GetEmbed());
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync(embed: response.GetEmbed());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Text)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync(response.GetString());
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync(response.GetString());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Error)
				{
					if (Settings.Instance.Client.UseReply)
					{
						_ = await Context.Message.ReplyAsync($"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await Context.Channel.SendMessageAsync($"**Error:** {response.GetString()}");
					}
				}
			}
		}
	}

	public class LeaderboardModule : ModuleBase<SocketCommandContext>
	{
		// @bot leaderboard | @bot lb
		[Command("leaderboard")]
		[Alias("lb")]
		[Summary("Returns server points leaderboard.")]
		public async Task SendServerLeaderboardCommand()
		{
			Log.WriteInfo($"Retrieving server points leaderboard (guild ID {Context.Guild.Id}).");
		}
	}

	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		// @bot config help
		[Command("help")]
		[Summary("Returns commands usage help message.")]
		public async Task SendHelpCommand()
		{
			Log.WriteInfo($"Sending commands usage help message.");

			Embed replyEmbed = Help.GetBotHelpMessage(Context.Client);

			if (Settings.Instance.Client.UseReply)
			{
				_ = await Context.Message.ReplyAsync(embed: replyEmbed);
			}
			else
			{
				_ = await Context.Channel.SendMessageAsync(embed: replyEmbed);
			}
		}
	}

	[Group("config")]
	[Summary("Server configuration commands.")]
	public class ConfigurationModule : ModuleBase<SocketCommandContext>
	{
		// @bot config show
		[Command("show")]
		[Summary("Returns current server configuration. Only available for server administrators.")]
		public async Task ShowConfigurationCommand()
		{
			Log.WriteInfo($"Retrieving server configuration data (guild ID {Context.Guild.Id}).");
		}

		// @bot config help
		[Command("help")]
		[Summary("Returns server configuration commands help message. Only available for server administrators.")]
		public async Task SendHelpConfigurationCommand()
		{
			Log.WriteInfo($"Sending server configuration commands help message (guild ID {Context.Guild.Id}).");

			Embed replyEmbed = Help.GetConfigHelpMessage(Context.Client);

			if (Settings.Instance.Client.UseReply)
			{
				_ = await Context.Message.ReplyAsync(embed: replyEmbed);
			}
			else
			{
				_ = await Context.Channel.SendMessageAsync(embed: replyEmbed);
			}
		}

		[Group("set")]
		[Summary("Configuration setter commands.")]
		public class ConfigurationSetterModule : ModuleBase<SocketCommandContext>
		{
			// @bot config set country
			[Command("country")]
			[Summary("Sets country restriction for this server. Leave empty to disable.")]
			public async Task SetServerCountryCommand()
			{
				Log.WriteInfo($"Disabling server country restriction (guild ID {Context.Guild.Id}).");
			}

			// @bot config set country [2-letter country code]
			[Command("country")]
			[Summary("Sets country restriction for this server. Leave empty to disable.")]
			public async Task SetServerCountryCommand([Summary("2-letter country code. Leave empty to disable.")] string countryCode)
			{
				Log.WriteInfo($"Setting server country restriction to {countryCode} (guild ID {Context.Guild.Id}).");
			}

			// @bot config set verifiedrole
			[Command("verifiedrole")]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand()
			{
				Log.WriteInfo($"Disabling verified user role (guild ID {Context.Guild.Id}).");
			}

			// @bot config set verifiedrole [mentioned role]
			[Command("verifiedrole")]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand([Summary("Role for verified users. Leave empty to disable.")] SocketRole role)
			{
				Log.WriteInfo($"Setting verified user role to {role.Id} (guild ID {Context.Guild.Id}).");
			}

			// @bot config set verifiedrole [role ID]
			[Command("verifiedrole")]
			[Summary("Sets verified user role, see commands help for details. Leave empty to disable.")]
			public async Task SetServerVerifiedRoleCommand([Summary("Role for verified users. Leave empty to disable.")] string roleDiscordId)
			{
				Log.WriteInfo($"Setting verified user role to {roleDiscordId} (guild ID {Context.Guild.Id}).");
			}

			// @bot config set commandschannel
			[Command("commandschannel")]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand()
			{
				Log.WriteInfo($"Disabling command channel restriction (guild ID {Context.Guild.Id}).");
			}

			// @bot config set commandschannel [mentioned channel]
			[Command("commandschannel")]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand([Summary("Channel for commands restriction. Leave empty to disable.")] SocketGuildChannel channel)
			{
				Log.WriteInfo($"Setting commands channel to {channel.Id} (guild ID {Context.Guild.Id}).");
			}

			// @bot config set commandschannel [channel ID]
			[Command("commandschannel")]
			[Summary("Sets server commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerCommandsChannelCommand([Summary("Channel for commands restriction. Leave empty to disable.")] string channelDiscordId)
			{
				Log.WriteInfo($"Setting commands channel to {channelDiscordId} (guild ID {Context.Guild.Id}).");
			}

			// @bot config set leaderboardschannel
			[Command("leaderboardschannel")]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand()
			{
				Log.WriteInfo($"Disabling leaderboard commands channel restriction (guild ID {Context.Guild.Id}).");
			}

			// @bot config set leaderboardschannel [mentioned channel]
			[Command("leaderboardschannel")]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand([Summary("Channel for leaderboard command restriction. Leave empty to disable.")] SocketGuildChannel channel)
			{
				Log.WriteInfo($"Setting leaderboard commands channel to {channel.Id} (guild ID {Context.Guild.Id}).");
			}

			// @bot config set leaderboardschannel [channel ID]
			[Command("leaderboardschannel")]
			[Summary("Sets server leaderboard commands channel restriction. Leave channel option empty to disable.")]
			public async Task SetServerLeaderboardsChannelCommand([Summary("Channel for leaderboard command restriction. Leave empty to disable.")] string channelDiscordId)
			{
				Log.WriteInfo($"Setting leaderboard commands channel to {channelDiscordId} (guild ID {Context.Guild.Id}).");
			}
		}
	}
}
