using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
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
				break;
			case "ping":
				// TODO: send ping
				await Log.WriteDebug("OnInvokeSlashInteraction", $"Send ping command received{ (guildChannel != null ? $" (guild ID { guildChannel.Guild.Id })" : "") }.");
				await SendPingCommand(cmd);
				break;
			case "count":
				// TODO: count points
				await Log.WriteDebug("OnInvokeSlashInteraction", "Count points command received.");
				break;
			case "whatif":
				// TODO: count what-if points
				await Log.WriteDebug("OnInvokeSlashInteraction", "Count what-if points command received.");
				break;
			case "serverleaderboard":
				// TODO: send server leaderboard
				await Log.WriteDebug("OnInvokeSlashInteraction", "Get server leaderboard command received.");
				break;
			case "config":
				// TODO: configure server settings
				await Log.WriteDebug("OnInvokeSlashInteraction", "Configuration command received.");
				break;
			case "help":
				// TODO: send help message
				await Log.WriteDebug("OnInvokeSlashInteraction", "Send help message command received.");
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
				break;
		}
	}

	private async Task SendPingCommand(SocketInteraction cmd)
	{
		await cmd.DeferAsync();

		await Log.WriteVerbose("SendPingSlashCommand", "Sending ping message.");

		string replyMsg = CommandsFactory.GetPingMessage(client);
		await cmd.ModifyOriginalResponseAsync(msg => msg.Content = replyMsg);
	}
}
