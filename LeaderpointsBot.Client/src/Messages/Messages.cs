using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Messages;

public static class MessagesFactory
{
	public static async Task OnNewMessage(SocketMessage msg)
	{
		await Log.WriteDebug("OnNewMessage", $"Message from { msg.Author.Username }#{ msg.Author.Discriminator }: { msg.Content }");
	}
}
