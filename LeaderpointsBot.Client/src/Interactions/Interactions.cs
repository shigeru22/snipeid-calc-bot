// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public class InteractionsFactory
{
	private readonly DiscordSocketClient client;

	public InteractionsFactory(DiscordSocketClient client)
	{
		Log.WriteVerbose("InteractionsFactory", "InteractionsFactory instance created.");

		this.client = client;

		Log.WriteVerbose("InteractionsFactory", "Instance client set with client parameter.");
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
						Log.WriteDebug("OnInvokeSlashInteraction", "Link user command received.");
						await InteractionModules.LinkSlashModule.LinkUserCommand(client, cmd);
						break;
					case "ping":
						// TODO: send ping
						Log.WriteDebug("OnInvokeSlashInteraction", $"Send ping command received{(guildChannel != null ? $" (guild ID {guildChannel.Guild.Id})" : string.Empty)}.");
						await InteractionModules.PingSlashModule.SendPingCommand(client, cmd);
						break;
					case "count":
						// TODO: count points
						Log.WriteDebug("OnInvokeSlashInteraction", "Count points command received.");
						await InteractionModules.CountSlashModule.CountPointsCommand(client, cmd);
						break;
					case "whatif":
						// TODO: count what-if points
						Log.WriteDebug("OnInvokeSlashInteraction", "Count what-if points command received.");
						await InteractionModules.CountSlashModule.WhatIfPointsCommand(client, cmd);
						break;
					case "serverleaderboard":
						// TODO: send server leaderboard
						Log.WriteDebug("OnInvokeSlashInteraction", "Get server leaderboard command received.");
						await InteractionModules.LeaderboardSlashModule.SendServerLeaderboardCommand(client, cmd);
						break;
					case "config":
						// TODO: configure server settings
						Log.WriteDebug("OnInvokeSlashInteraction", "Server configuration command received. Handling subcommand.");
						await HandleConfigurationSlashCommand(cmd);
						break;
					case "help":
						// TODO: send help message
						Log.WriteDebug("OnInvokeSlashInteraction", "Send help message command received.");
						await InteractionModules.HelpModule.SendHelpCommand(client, cmd);
						break;
				}
			}
			catch (SendMessageException e)
			{
				Log.WriteVerbose("OnInvokeSlashInteraction", "Send message signal received. Sending message and cancelling process.");

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
				Log.WriteError("OnInvokeSlashInteraction", $"Unhandled client error occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");

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
						Log.WriteDebug("OnInvokeUserContextInteraction", "Count points command received.");
						await InteractionModules.CountContextModule.CountPointsCommand(client, cmd);
						break;
				}
			}
			catch (SendMessageException e)
			{
				Log.WriteVerbose("OnInvokeSlashInteraction", "Send message signal received. Sending message and cancelling process.");

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
				Log.WriteError("OnInvokeSlashInteraction", $"Unhandled client error occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");

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
				Log.WriteDebug("HandleConfigurationSlashCommand", "Show server configuration subcommand received.");
				await InteractionModules.ConfigurationSlashModule.ShowConfigurationCommand(client, cmd);
				break;
			case "help":
				Log.WriteDebug("HandleConfigurationSlashCommand", "Server configuration setter command received.");
				await InteractionModules.ConfigurationSlashModule.SendHelpConfigurationCommand(client, cmd);
				break;
			case "set":
				Log.WriteDebug("HandleConfigurationSlashCommand", "Server configuration setter command received. Handling subcommand.");
				await HandleConfigurationSetterSlashCommand(cmd);
				break;
		}
	}

	private async Task HandleConfigurationSetterSlashCommand(SocketSlashCommand cmd)
	{
		string subcommandName;

		try
		{
			Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Retrieving second subcommand name.");
			subcommandName = (string)cmd.Data.Options.First().Options.First().Name;
			Log.WriteDebug("HandleConfigurationSetterSlashCommand", $"Second subcommand name retrieved: {subcommandName}.");
		}
		catch (Exception e)
		{
			Log.WriteDebug("HandleConfigurationSetterSlashCommand", $"Unhandled exception occurred while retrieving second subcommand. Exception details below.\n{e}");
			await cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
			return;
		}

		switch (subcommandName)
		{
			case "country":
				Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server country restriction command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerCountryCommand(client, cmd);
				break;
			case "verifiedrole":
				Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server verified role command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerVerifiedRoleCommand(client, cmd);
				break;
			case "commandschannel":
				Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server commands channel restriction command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerCommandsChannelCommand(client, cmd);
				break;
			case "leaderboardschannel":
				Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server leaderboard commands channel restriction command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerLeaderboardsChannelCommand(client, cmd);
				break;
		}
	}
}
