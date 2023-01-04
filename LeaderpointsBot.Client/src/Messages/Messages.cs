using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Messages;

public class MessagesFactory
{
	private readonly DiscordSocketClient client;

	public MessagesFactory(DiscordSocketClient client)
	{
		Log.WriteVerbose("MessagesFactory", "MessagesFactory instance created.");

		this.client = client;

		Log.WriteVerbose("MessagesFactory", "Instance client set with client parameter.");
	}

	public async Task OnNewMessage(SocketMessage msg)
	{
		if(msg is not SocketUserMessage userMsg)
		{
			return;
		}

		if(msg.Channel.GetChannelType() != ChannelType.Text)
		{
			return;
		}

		// await Log.WriteDebug("OnNewMessage", $"Message from { msg.Author.Username }#{ msg.Author.Discriminator }: { msg.Content }");

		await HandleCommands(userMsg);
	}

	private async Task HandleCommands(SocketUserMessage msg)
	{
		string[] contents = new Regex("\\s+").Split(msg.Content);
		if(contents.Length < 2)
		{
			return;
		}

		SocketCommandContext context = new(client, msg);
		SocketGuildChannel? guildChannel = msg.Channel as SocketGuildChannel;

		// yeah, I went old style. problem?
		switch(contents[1])
		{
			case "link":
				// TODO: verify user
				await Log.WriteDebug("HandleCommands", "Link user command received.");
				break;
			case "ping":
				await Log.WriteDebug("HandleCommands", $"Send ping command received{ (guildChannel != null ? $" (guild ID { guildChannel.Guild.Id })" : "") }.");
				await SendPingCommand(context);
				break;
			case "count":
				// TODO: count points
				await Log.WriteDebug("HandleCommands", "Count points command received.");
				break;
			case "whatif":
				// TODO: count what-if points
				await Log.WriteDebug("HandleCommands", "Count what-if points command received.");
				break;
			case "lb": // fallthrough
			case "leaderboard":
				// TODO: send server leaderboard
				await Log.WriteDebug("HandleCommands", "Get server leaderboard command received.");
				break;
			case "config":
				// TODO: configure server settings
				await Log.WriteDebug("HandleCommands", "Configuration command received.");
				break;
			case "help":
				// TODO: send help message
				await Log.WriteDebug("HandleCommands", "Send help message command received.");
				break;
		}
	}

	private async Task SendPingCommand(SocketCommandContext msgContext)
	{
		await Log.WriteVerbose("SendPing", "Sending ping message.");

		string replyMsg = CommandsFactory.GetPingMessage(client);
		await msgContext.Message.ReplyAsync(replyMsg);
	}
}
