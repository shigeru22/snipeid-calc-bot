using Discord.WebSocket;
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

	public async Task OnInvokeSlashInteraction(SocketSlashCommand cmd)
	{
		// await Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		SocketGuildChannel? guildChannel = cmd.Channel as SocketGuildChannel;

		switch(cmd.Data.Name)
		{
			case "link":
				// TODO: link user
				await Log.WriteDebug("OnInvokeSlashInteraction", "Link user command received.");
				await InteractionModules.LinkSlashModule.LinkUserCommand(client, cmd);
				break;
			case "ping":
				// TODO: send ping
				await Log.WriteDebug("OnInvokeSlashInteraction", $"Send ping command received{ (guildChannel != null ? $" (guild ID { guildChannel.Guild.Id })" : "") }.");
				await InteractionModules.PingSlashModule.SendPingCommand(client, cmd);
				break;
			case "count":
				// TODO: count points
				await Log.WriteDebug("OnInvokeSlashInteraction", "Count points command received.");
				await InteractionModules.CountSlashModule.CountPointsCommand(client, cmd);
				break;
			case "whatif":
				// TODO: count what-if points
				await Log.WriteDebug("OnInvokeSlashInteraction", "Count what-if points command received.");
				await InteractionModules.CountSlashModule.WhatIfPointsCommand(client, cmd);
				break;
			case "serverleaderboard":
				// TODO: send server leaderboard
				await Log.WriteDebug("OnInvokeSlashInteraction", "Get server leaderboard command received.");
				await InteractionModules.LeaderboardSlashModule.SendServerLeaderboardCommand(client, cmd);
				break;
			case "config":
				// TODO: configure server settings
				await Log.WriteDebug("OnInvokeSlashInteraction", "Server configuration command received. Handling subcommand.");
				await HandleConfigurationSlashCommand(cmd);
				break;
			case "help":
				// TODO: send help message
				await Log.WriteDebug("OnInvokeSlashInteraction", "Send help message command received.");
				await InteractionModules.HelpModule.SendHelpCommand(client, cmd);
				break;
		}
	}

	public async Task OnInvokeUserContextInteraction(SocketUserCommand cmd)
	{
		// await Log.WriteDebug("OnInvokeContextInteraction", $"Context interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		switch(cmd.Data.Name)
		{
			case "Calculate points":
				// TODO: count points
				await Log.WriteDebug("OnInvokeUserContextInteraction", "Count points command received.");
				try
				{
					await InteractionModules.CountContextModule.CountPointsCommand(client, cmd);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
				break;
		}
	}

	private async Task HandleConfigurationSlashCommand(SocketSlashCommand cmd)
	{
		string subcommandName = cmd.Data.Options.First().Name;

		switch(subcommandName)
		{
			case "show":
				await Log.WriteDebug("HandleConfigurationSlashCommand", "Show server configuration subcommand received.");
				await InteractionModules.ConfigurationSlashModule.ShowConfigurationCommand(client, cmd);
				break;
			case "help":
				await Log.WriteDebug("HandleConfigurationSlashCommand", "Server configuration setter command received.");
				await InteractionModules.ConfigurationSlashModule.SendHelpConfigurationCommand(client, cmd);
				break;	
			case "set":
				await Log.WriteDebug("HandleConfigurationSlashCommand", "Server configuration setter command received. Handling subcommand.");
				await HandleConfigurationSetterSlashCommand(cmd);
				break;
		}
	}

	private async Task HandleConfigurationSetterSlashCommand(SocketSlashCommand cmd)
	{
		string subcommandName;
		
		try
		{
			await Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Retrieving second subcommand name.");
			subcommandName = (string)cmd.Data.Options.First().Options.First().Name;
			await Log.WriteDebug("HandleConfigurationSetterSlashCommand", $"Second subcommand name retrieved: { subcommandName }.");
		}
		catch (Exception e)
		{
			await Log.WriteDebug("HandleConfigurationSetterSlashCommand", $"Unhandled exception occurred while retrieving second subcommand. Exception details below.\n{ e }");
			cmd.RespondAsync("An error occurred while processing your command.", ephemeral: true);
			return;
		}

		switch(subcommandName)
		{
			case "country":
				await Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server country restriction command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerCountryCommand(client, cmd);
				break;
			case "verifiedrole":
				await Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server verified role command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerVerifiedRoleCommand(client, cmd);
				break;
			case "commandschannel":
				await Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server commands channel restriction command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerCommandsChannelCommand(client, cmd);
				break;
			case "leaderboardschannel":
				await Log.WriteDebug("HandleConfigurationSetterSlashCommand", "Set server leaderboard commands channel restriction command received.");
				await InteractionModules.ConfigurationSlashModule.ConfigurationSetterSlashModule.SetServerLeaderboardsChannelCommand(client, cmd);
				break;
		}
	}
}
