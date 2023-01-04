using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Interactions;

public class InteractionsFactory
{
	public static async Task OnInvokeSlashInteraction(SocketSlashCommand cmd)
	{
		await Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");
	}

	public static async Task OnInvokeUserContextInteraction(SocketUserCommand cmd)
	{
		await Log.WriteDebug("OnInvokeContextInteraction", $"Context interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");
	}
}
