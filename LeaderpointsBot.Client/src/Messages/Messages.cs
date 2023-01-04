using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Messages;

public class MessagesFactory
{
	private DiscordSocketClient client;

	public MessagesFactory(DiscordSocketClient client)
	{
		Log.WriteVerbose("MessagesFactory", "MessagesFactory instance created.");

		this.client = client;

		Log.WriteVerbose("MessagesFactory", "Instance client set with client parameter.");
	}

	public async Task OnNewMessage(SocketMessage msg)
	{
		await Log.WriteDebug("OnNewMessage", $"Message from { msg.Author.Username }#{ msg.Author.Discriminator }: { msg.Content }");
	}
}
