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

public class MessageHandler
{
	private const string BATHBOT_DISCORD_ID = "297073686916366336";

	private readonly DiscordSocketClient client;
	private readonly CommandService commandService;

	public MessageHandler(DiscordSocketClient client, CommandService commandService)
	{
		Log.WriteVerbose("MessagesFactory", "MessagesFactory instance created.");

		this.client = client;
		this.commandService = commandService;

		Log.WriteVerbose("MessagesFactory", "Instance parameters set.");
	}

	public async Task InitializeServiceAsync()
	{
		Log.WriteVerbose("InitializeServiceAsync", "Registering entry assembly as command service module.");
		_ = await commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
	}

	public async Task OnNewMessage(SocketMessage msg)
	{
		// ignore own messages silently
		if (msg.Author.Id == client.CurrentUser.Id)
		{
			return;
		}

		Log.WriteDebug("OnNewMessage", $"Message from {msg.Author.Username}#{msg.Author.Discriminator}: {msg.Content}");

		if (msg is not SocketUserMessage userMsg)
		{
			Log.WriteDebug("OnNewMessage", "Message is not from user.");
			return;
		}

		if (msg.Channel.GetChannelType() != ChannelType.Text)
		{
			Log.WriteDebug("OnNewMessage", "Message is not from text-based channel.");
			return;
		}

		// determine if Bathbot's leaderboard count embed is received
		if (msg.Author.Id.ToString().Equals(BATHBOT_DISCORD_ID))
		{
			Log.WriteDebug("OnNewMessage", "Message is from Bathbot. Handling message.");
			await HandleBathbotMessageAsync(userMsg);
			return;
		}

		Log.WriteDebug("OnNewMessage", "Message is from user and text-based channel. Handling command.");
		await HandleCommandsAsync(userMsg);
	}

	private async Task HandleBathbotMessageAsync(SocketUserMessage msg)
	{
		Embed botEmbed;
		try
		{
			Log.WriteVerbose("HandleBathbotMessageAsync", "Fetching first embed from Bathbot message.");
			botEmbed = msg.Embeds.First();
		}
		catch (Exception)
		{
			Log.WriteVerbose("HandleBathbotMessageAsync", "No embeds found from Bathbot message. Cancelling process.");
			return;
		}

		// filter embed by title
		if (string.IsNullOrWhiteSpace(botEmbed.Title) || !botEmbed.Title.StartsWith("In how many top X map leaderboards is"))
		{
			Log.WriteVerbose("HandleBathbotMessageAsync", "Embed is not leaderboards count. Cancelling process.");
			return;
		}

		if (msg.Channel is not SocketGuildChannel guildChannel)
		{
			// TODO: handle direct message response
			Log.WriteVerbose("HandleBathbotMessageAsync", "Direct messages method not yet implemented.");
			return;
		}

		SocketCommandContext context = new SocketCommandContext(client, msg);
		Structures.Commands.CountModule.UserLeaderboardsCountMessages[] responses;

		try
		{
			Log.WriteVerbose("HandleBathbotMessageAsync", "Calculating leaderboards count from first embed.");
			responses = await Counter.UserLeaderboardsCountBathbotAsync(msg.Embeds.First(), guildChannel.Guild);
		}
		catch (SendMessageException e)
		{
			Log.WriteVerbose("HandleBathbotMessageAsync", "Send message signal received. Sending message and cancelling process.");
			_ = await context.Channel.SendMessageAsync(e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
			return;
		}
		catch (Exception e)
		{
			Log.WriteError("HandleBathbotMessageAsync", $"Unhandled client error occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			_ = await context.Channel.SendMessageAsync("**Error:** Unhandled client error occurred.");
			return;
		}

		Log.WriteVerbose("HandleBathbotMessageAsync", "Sending responses.");
		foreach (Structures.Commands.CountModule.UserLeaderboardsCountMessages response in responses)
		{
			switch (response.MessageType)
			{
				case Common.ResponseMessageType.Embed:
					_ = await context.Channel.SendMessageAsync(embed: response.GetEmbed());
					break;
				case Common.ResponseMessageType.Text:
					_ = await context.Channel.SendMessageAsync(response.GetString());
					break;
				case Common.ResponseMessageType.Error:
					_ = await context.Channel.SendMessageAsync($"**Error:** {response.GetString()}");
					break;
				default:
					continue;
			}
		}
	}

	private async Task HandleCommandsAsync(SocketUserMessage msg)
	{
		static async Task SendMessage(SocketCommandContext context, string message)
		{
			if (Settings.Instance.Client.UseReply)
			{
				_ = await context.Message.ReplyAsync(message);
			}
			else
			{
				_ = await context.Channel.SendMessageAsync(message);
			}
		}

		int argPos = 0; // TODO: create per-server prefix setting

		if (!msg.HasMentionPrefix(client.CurrentUser, ref argPos))
		{
			Log.WriteDebug("HandleCommands", "Bot is not mentioned.");
			return;
		}

		Log.WriteVerbose("HandleCommands", "Creating context and executing command.");

		SocketCommandContext context = new SocketCommandContext(client, msg);

		IResult result = await commandService.ExecuteAsync(context: context, argPos: argPos, services: null);

		if (result.Error != CommandError.Exception)
		{
			// command processing complete
			return;
		}

		if (result is ExecuteResult execResult)
		{
			Exception e = execResult.Exception;

			if (e is SendMessageException ex)
			{
				await SendMessage(context, $"{(ex.IsError ? "**Error:** " : string.Empty)}{ex.Draft}");
				return;
			}

			Log.WriteError(nameof(HandleCommandsAsync), $"Unhandled client error occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			await SendMessage(context, $"**Error:** Unhandled client error occurred.");
		}
	}
}
