// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Messages;

public class MessagesFactory
{
	private const string BathbotDiscordID = "297073686916366336";

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
		if (msg.Author.Id == client.CurrentUser.Id)
		{
			return;
		}

		await Log.WriteDebug("OnNewMessage", $"Message from {msg.Author.Username}#{msg.Author.Discriminator}: {msg.Content}");

		if (msg is not SocketUserMessage userMsg)
		{
			await Log.WriteDebug("OnNewMessage", "Message is not from user.");
			return;
		}

		if (msg.Channel.GetChannelType() != ChannelType.Text)
		{
			await Log.WriteDebug("OnNewMessage", "Message is not from text-based channel.");
			return;
		}

		// determine if Bathbot's leaderboard count embed is received
		if (msg.Author.Id.ToString().Equals(BathbotDiscordID))
		{
			await Log.WriteDebug("OnNewMessage", "Message is from Bathbot. Handling message.");
			await HandleBathbotMessageAsync(userMsg);
			return;
		}

		await Log.WriteDebug("OnNewMessage", "Message is from user and text-based channel. Handling command.");
		await HandleCommandsAsync(userMsg);
	}

	private async Task HandleBathbotMessageAsync(SocketUserMessage msg)
	{
		Embed botEmbed;
		try
		{
			await Log.WriteVerbose("HandleBathbotMessageAsync", "Fetching first embed from Bathbot message.");
			botEmbed = msg.Embeds.First();
		}
		catch (Exception)
		{
			await Log.WriteVerbose("HandleBathbotMessageAsync", "No embeds found from Bathbot message. Cancelling process.");
			return;
		}

		// filter embed by title
		if (string.IsNullOrWhiteSpace(botEmbed.Title) || !botEmbed.Title.StartsWith("In how many top X map leaderboards is"))
		{
			await Log.WriteVerbose("HandleBathbotMessageAsync", "Embed is not leaderboards count. Cancelling process.");
			return;
		}

		if (msg.Channel is not SocketGuildChannel guildChannel)
		{
			// TODO: handle direct message response
			await Log.WriteVerbose("HandleBathbotMessageAsync", "Direct messages method not yet implemented.");
			return;
		}

		SocketCommandContext context = new SocketCommandContext(client, msg);
		Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses;

		try
		{
			await Log.WriteVerbose("HandleBathbotMessageAsync", "Calculating leaderboards count from first embed.");
			responses = await CountModule.UserLeaderboardsCountBathbotAsync(msg.Embeds.First(), guildChannel.Guild);
		}
		catch (SendMessageException e)
		{
			await Log.WriteVerbose("HandleBathbotMessageAsync", "Send message signal received. Sending message and cancelling process.");
			await context.Channel.SendMessageAsync(e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
			return;
		}
		catch (Exception e)
		{
			await Log.WriteError("HandleBathbotMessageAsync", $"Unhandled client error occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			await context.Channel.SendMessageAsync("**Error:** Unhandled client error occurred.");
			return;
		}

		await Log.WriteVerbose("HandleBathbotMessageAsync", "Sending responses.");
		foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
		{
			switch (response.MessageType)
			{
				case Common.ResponseMessageType.EMBED:
					await context.Channel.SendMessageAsync(embed: response.GetEmbed());
					break;
				case Common.ResponseMessageType.TEXT:
					await context.Channel.SendMessageAsync(response.GetString());
					break;
				case Common.ResponseMessageType.ERROR:
					await context.Channel.SendMessageAsync($"**Error:** {response.GetString()}");
					break;
				default:
					continue;
			}
		}
	}

	private async Task HandleCommandsAsync(SocketUserMessage msg)
	{
		int argPos = 0; // TODO: create per-server prefix setting

		if (!msg.HasMentionPrefix(client.CurrentUser, ref argPos))
		{
			await Log.WriteDebug("HandleCommands", "Bot is not mentioned.");
			return;
		}

		await Log.WriteVerbose("HandleCommands", "Creating context and executing command.");

		SocketCommandContext context = new SocketCommandContext(client, msg);
		await commandService.ExecuteAsync(context: context, argPos: argPos, services: null);
	}
}
