using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Messages;

public class MessagesFactory
{
	private readonly DiscordSocketClient client;
	private readonly CommandService commandService;

	public MessagesFactory(DiscordSocketClient client, CommandService commandService)
	{
		Log.WriteVerbose("MessagesFactory", "MessagesFactory instance created.");

		this.client = client;
		this.commandService = commandService;

		Log.WriteVerbose("MessagesFactory", "Instance parameters set.");
	}

	public async Task InitializeServiceAsync()
	{
		await Log.WriteVerbose("InitializeServiceAsync", "Registering entry assembly as command service module.");
		await commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
	}

	public async Task OnNewMessage(SocketMessage msg)
	{
		// ignore own messages silently
		if(msg.Author.Id == client.CurrentUser.Id)
		{
			return;
		}

		await Log.WriteDebug("OnNewMessage", $"Message from { msg.Author.Username }#{ msg.Author.Discriminator }: { msg.Content }");

		if(msg is not SocketUserMessage userMsg)
		{
			await Log.WriteDebug("OnNewMessage", "Message is not from user.");
			return;
		}

		if(msg.Channel.GetChannelType() != ChannelType.Text)
		{
			await Log.WriteDebug("OnNewMessage", "Message is not from text-based channel.");
			return;
		}

		await Log.WriteDebug("OnNewMessage", "Message is from user and text-based channel. Handling command.");
		await HandleCommandsAsync(userMsg);
	}

	private async Task HandleCommandsAsync(SocketUserMessage msg)
	{
		int argPos = 0; // TODO: create per-server prefix setting

		if(!msg.HasMentionPrefix(client.CurrentUser, ref argPos))
		{
			await Log.WriteDebug("HandleCommands", "Bot is not mentioned.");
			return;
		}

		await Log.WriteVerbose("HandleCommands", "Creating context and executing command.");

		SocketCommandContext context = new(client, msg);
		await commandService.ExecuteAsync(context: context, argPos: argPos, services: null);
	}
}
