using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public class InteractionsFactory
{
	private DiscordSocketClient client;

	public InteractionsFactory(DiscordSocketClient client)
	{
		Log.WriteVerbose("InteractionsFactory", "InteractionsFactory instance created.");

		this.client = client;

		Log.WriteVerbose("InteractionsFactory", "Instance client set with client parameter.");
	}

	public async Task OnInvokeSlashInteraction(SocketSlashCommand cmd)
	{
		await Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");
	}

	public async Task OnInvokeUserContextInteraction(SocketUserCommand cmd)
	{
		await Log.WriteDebug("OnInvokeContextInteraction", $"Context interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");
	}
}
