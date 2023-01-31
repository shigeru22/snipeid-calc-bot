// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Interactions;

public class InteractionHandler
{
	private readonly DiscordSocketClient client;

	public InteractionHandler(DiscordSocketClient client)
	{
		Log.WriteVerbose("InteractionsFactory instance created.");

		this.client = client;

		Log.WriteVerbose("Instance client set with client parameter.");
	}

	public Task OnInvokeSlashInteraction(SocketSlashCommand cmd)
	{
		// Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		_ = Task.Run(async () =>
		{
			try
			{
				SocketGuildChannel? guildChannel = cmd.Channel as SocketGuildChannel;

				switch (cmd.Data.Name)
				{
					case "link":
						// TODO: link user
						Log.WriteDebug("Link user command received.");
						await InteractionModule.LinkSlashModule.LinkUserCommand(client, cmd);
						break;
					case "ping":
						// TODO: send ping
						Log.WriteDebug($"Send ping command received{(guildChannel != null ? $" (guild ID {guildChannel.Guild.Id})" : string.Empty)}.");
						await InteractionModule.PingSlashModule.SendPingCommand(client, cmd);
						break;
					case "count":
						// TODO: count points
						Log.WriteDebug("Count points command received.");
						await InteractionModule.CountSlashModule.CountPointsCommand(client, cmd);
						break;
					case "whatif":
						// TODO: count what-if points
						Log.WriteDebug("Count what-if points command received.");
						await InteractionModule.CountSlashModule.WhatIfPointsCommand(client, cmd);
						break;
					case "serverleaderboard":
						// TODO: send server leaderboard
						Log.WriteDebug("Get server leaderboard command received.");
						await InteractionModule.LeaderboardSlashModule.SendServerLeaderboardCommand(client, cmd);
						break;
					case "config":
						// TODO: configure server settings
						Log.WriteDebug("Server configuration command received. Handling subcommand.");
						await HandleConfigurationSlashCommand(cmd);
						break;
					case "help":
						// TODO: send help message
						Log.WriteDebug("Send help message command received.");
						await InteractionModule.HelpModule.SendHelpCommand(client, cmd);
						break;
				}
			}
			catch (SendMessageException e)
			{
				Log.WriteVerbose("Send message signal received. Sending message and cancelling process.");

				if (cmd.HasResponded)
				{
					_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
				}
				else
				{
					await cmd.RespondAsync(e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
				}
			}
			catch (Exception e)
			{
				Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));

				if (cmd.HasResponded)
				{
					_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = "**Error:** Unhandled client error occurred.");
				}
				else
				{
					await cmd.RespondAsync("**Error:** Unhandled client error occurred.");
				}
			}
		});

		return Task.CompletedTask;
	}

	public Task OnInvokeUserContextInteraction(SocketUserCommand cmd)
	{
		// Log.WriteDebug("OnInvokeContextInteraction", $"Context interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		_ = Task.Run(async () =>
		{
			try
			{
				switch (cmd.Data.Name)
				{
					case "Calculate points":
						// TODO: count points
						Log.WriteDebug("Count points command received.");
						await InteractionModule.CountContextModule.CountPointsCommand(client, cmd);
						break;
				}
			}
			catch (SendMessageException e)
			{
				Log.WriteVerbose("Send message signal received. Sending message and cancelling process.");

				if (cmd.HasResponded)
				{
					_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
				}
				else
				{
					await cmd.RespondAsync(e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
				}
			}
			catch (Exception e)
			{
				Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));

				if (cmd.HasResponded)
				{
					_ = await cmd.ModifyOriginalResponseAsync(msg => msg.Content = "**Error:** Unhandled client error occurred.");
				}
				else
				{
					await cmd.RespondAsync("**Error:** Unhandled client error occurred.");
				}
			}
		});

		return Task.CompletedTask;
	}

	private async Task HandleConfigurationSlashCommand(SocketSlashCommand cmd)
	{
		string subcommandName = cmd.Data.Options.First().Name;

		switch (subcommandName)
		{
			case "show":
				Log.WriteDebug("Show server configuration subcommand received.");
				await InteractionModule.ConfigurationSlashModule.ShowConfigurationCommand(client, cmd);
				break;
			case "help":
				Log.WriteDebug("Server configuration setter command received.");
				await InteractionModule.ConfigurationSlashModule.SendHelpConfigurationCommand(client, cmd);
				break;
			case "set":
				Log.WriteDebug("Server configuration setter command received. Handling subcommand.");
				await HandleConfigurationSetterSlashCommand(cmd);
				break;
		}
	}

	private async Task HandleConfigurationSetterSlashCommand(SocketSlashCommand cmd)
	{
		string subcommandName;

		try
		{
			Log.WriteDebug("Retrieving second subcommand name.");
			subcommandName = (string)cmd.Data.Options.First().Options.First().Name;
			Log.WriteDebug($"Second subcommand name retrieved: {subcommandName}.");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
			return;
		}

		switch (subcommandName)
		{
			case "country":
				Log.WriteDebug("Set server country restriction command received.");
				await InteractionModule.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerCountryCommand(client, cmd);
				break;
			case "verifiedrole":
				Log.WriteDebug("Set server verified role command received.");
				await InteractionModule.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerVerifiedRoleCommand(client, cmd);
				break;
			case "commandschannel":
				Log.WriteDebug("Set server commands channel restriction command received.");
				await InteractionModule.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerCommandsChannelCommand(client, cmd);
				break;
			case "leaderboardschannel":
				Log.WriteDebug("Set server leaderboard commands channel restriction command received.");
				await InteractionModule.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerLeaderboardsChannelCommand(client, cmd);
				break;
		}
	}
}
