// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public static class InteractionModule
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

			Embed replyEmbed;

			if (!Context.Interaction.IsDMInteraction)
			{
				Log.WriteVerbose("Interaction sent from server.");
				replyEmbed = await User.LinkUser(Context.User, osuId, Context.Guild);
			}
			else
			{
				Log.WriteVerbose("Interaction sent from direct message.");
				replyEmbed = await User.LinkUser(Context.User, osuId);
			}

			Log.WriteInfo("Link success. Sending embed response.");

			_ = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
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

			string replyMsg = Help.GetPingMessage(Context.Client);
			_ = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = replyMsg);
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

			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses;

			if (!Context.Interaction.IsDMInteraction)
			{
				if (!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername, Context.Guild);
				}
				else
				{
					responses = await Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString(), Context.Guild);
				}
			}
			else
			{
				Log.WriteInfo("Command invoked from direct message. This will ignore update actions.");

				if (!string.IsNullOrWhiteSpace(osuUsername))
				{
					responses = await Counter.CountLeaderboardPointsByOsuUsernameAsync(osuUsername);
				}
				else
				{
					responses = await Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString());
				}
			}

			RestInteractionMessage? replyMsg = null;
			foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (replyMsg == null)
					{
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg =>
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
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = response.GetString());
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
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = $"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
			}
		}

		// /whatif [pointsargs]
		[EnabledInDm(true)]
		[SlashCommand("whatif", "Calculates what-if points.", runMode: RunMode.Async)]
		public async Task WhatIfPointsCommand([Summary("parameters", "Arguments for what-if count. See help for details.")] string pointsArgs)
		{
			Log.WriteInfo($"Calculating what-if points for {Context.User.Username}#{Context.User.Discriminator} ({pointsArgs}).");
			await Context.Interaction.DeferAsync();
		}
	}

	public class CountContextModule : InteractionModuleBase<SocketInteractionContext>
	{
		// user context -> Calculate points
		[EnabledInDm(true)]
		[UserCommand("Calculate points")]
		public async Task CountPointsCommand(IUser user)
		{
			Log.WriteInfo($"Calculating points for {user.Username}#{user.Discriminator}.");

			await Context.Interaction.DeferAsync();

			Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses = await Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString());

			RestInteractionMessage? replyMsg = null;
			foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (replyMsg == null)
					{
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg =>
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
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = response.GetString());
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
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = $"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
			}
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
		}
	}

	public class HelpModule : InteractionModuleBase<SocketInteractionContext>
	{
		// /help
		[EnabledInDm(true)]
		[SlashCommand("help", "Returns all commands usage help.", runMode: RunMode.Async)]
		public static async Task SendHelpCommand(DiscordSocketClient client, SocketSlashCommand cmd)
		{
			Log.WriteInfo($"Sending commands usage help message.");
			await cmd.DeferAsync();

			Embed replyEmbed = Help.GetBotHelpMessage(client, true);

			_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
		}
	}

	[EnabledInDm(false)]
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

			Embed replyEmbed = Help.GetConfigHelpMessage(Context.Client, true);

			_ = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
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
					Log.WriteInfo($"Disabling command channel restriction (guild ID {Context.Guild.Id}).");
				}

				await Context.Interaction.DeferAsync();
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
			}
		}
	}
}
