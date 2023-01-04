using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
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
		if(msg.Channel.GetChannelType() != ChannelType.Text)
		{
			return;
		}

		// await Log.WriteDebug("OnNewMessage", $"Message from { msg.Author.Username }#{ msg.Author.Discriminator }: { msg.Content }");

		string[] contents = new Regex("\\s+").Split(msg.Content);
		bool isClientMentioned = msg.MentionedUsers.Count(user => user.Id == client.CurrentUser.Id) == 1 && contents[0].Contains(client.CurrentUser.Id.ToString());

		bool processed = await HandleCommands(msg);

		if(!processed && isClientMentioned)
		{
			// TODO: send random conversation message
			await Log.WriteDebug("OnNewMessage", "Bot mentioned but nothing processed. Send random message.");
		}
	}

	private async Task<bool> HandleCommands(SocketMessage msg)
	{
		string[] contents = new Regex("\\s+").Split(msg.Content);
		if(contents.Length < 2)
		{
			return false;
		}

		bool processed = true;

		switch(contents[1])
		{
			case "link":
				// TODO: verify user
				await Log.WriteDebug("HandleCommands", "Link user command received.");
				break;
			case "ping":
				// TODO: send ping
				await Log.WriteDebug("HandleCommands", "Send ping command received.");
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
			default:
				processed = false;
				break;
		}

		return processed;
	}
}
