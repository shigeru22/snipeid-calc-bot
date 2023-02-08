// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client;

public class Handler
{
	private const string BATHBOT_DISCORD_ID = "297073686916366336";

	private readonly DiscordSocketClient client;

	private readonly CommandService commandService;
	private readonly InteractionService interactionService;

	public Handler(DiscordSocketClient client)
	{
		Log.WriteVerbose("Handlers instance created.");

		this.client = client;

		Log.WriteVerbose("Instance parameters set. Will create service instances.");

		commandService = new CommandService(new CommandServiceConfig()
		{
			LogLevel = LogSeverity.Info,
			CaseSensitiveCommands = false
		});

		interactionService = new InteractionService(client.Rest, new InteractionServiceConfig()
		{
			LogLevel = LogSeverity.Info
		});

		Log.WriteVerbose("Services created.");
	}

	public Handler(DiscordSocketClient client, CommandService commandService, InteractionService interactionService)
	{
		Log.WriteVerbose("Handlers instance created.");

		this.client = client;
		this.commandService = commandService;
		this.interactionService = interactionService;

		Log.WriteVerbose("Instance parameters set.");
	}

	public async Task InitializeCommandServiceAsync()
	{
		Log.WriteVerbose("Registering entry assembly as command service module.");
		_ = await commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
	}

	public async Task InitializeInteractionServiceAsync()
	{
		Log.WriteVerbose("Registering entry assembly as interaction service module.");
		_ = await interactionService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
	}

	public async Task RegisterCommandsAsync()
	{
		Log.WriteVerbose("Registering interaction commands globally.");
		_ = await interactionService.RegisterCommandsGloballyAsync();
	}

	public async Task RegisterCommandsAsync(string guildDiscordId)
	{
		Log.WriteVerbose($"Registering interaction commands (guild ID {guildDiscordId}).");
		_ = await interactionService.RegisterCommandsToGuildAsync(ulong.Parse(guildDiscordId));
	}

	public async Task OnNewMessage(SocketMessage msg)
	{
		// ignore own messages silently
		if (msg.Author.Id == client.CurrentUser.Id)
		{
			return;
		}

		// Log.WriteDebug($"Message from {msg.Author.Username}#{msg.Author.Discriminator}: {msg.Content}");

		if (msg is not SocketUserMessage userMsg)
		{
			Log.WriteDebug("Message is not from user.");
			return;
		}

		if (msg.Channel.GetChannelType() != ChannelType.Text)
		{
			Log.WriteDebug("Message is not from text-based channel.");
			return;
		}

		// determine if Bathbot's leaderboard count embed is received
		if (msg.Author.Id.ToString().Equals(BATHBOT_DISCORD_ID))
		{
			Log.WriteDebug("Message is from Bathbot. Handling message.");
			await HandleBathbotMessageAsync(userMsg);
			return;
		}

		Log.WriteDebug("Message is from user and text-based channel. Handling command.");
		await HandleCommandsAsync(userMsg);
	}

	public async Task OnInvokeInteraction(SocketInteraction cmd)
	{
		static async Task SendResponse(SocketInteraction cmd, string message)
		{
			if (cmd.HasResponded)
			{
				_ = await cmd.ModifyOriginalResponseAsync(res => res.Content = message);
			}
			else
			{
				await cmd.RespondAsync(message);
			}
		}

		// Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		SocketInteractionContext context = new SocketInteractionContext(client, cmd);

		Discord.Interactions.IResult result = await interactionService.ExecuteCommandAsync(context, null);

		if (result.Error != InteractionCommandError.Exception)
		{
			// command processing complete
			return;
		}

		if (result is Discord.Interactions.ExecuteResult execResult)
		{
			Exception e = execResult.Exception;

			if (e is SendMessageException ex)
			{
				await SendResponse(cmd, $"{(ex.IsError ? "**Error:** " : string.Empty)}{ex.Draft})");
				return;
			}

			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			await SendResponse(cmd, "**Error:** Unhandled client error occurred.");
		}
	}

	private async Task HandleBathbotMessageAsync(SocketUserMessage msg)
	{
		Embed botEmbed;
		try
		{
			Log.WriteVerbose("Fetching first embed from Bathbot message.");
			botEmbed = msg.Embeds.First();
		}
		catch (Exception)
		{
			Log.WriteVerbose("No embeds found from Bathbot message. Cancelling process.");
			return;
		}

		// filter embed by title
		if (string.IsNullOrWhiteSpace(botEmbed.Title) || !botEmbed.Title.StartsWith("In how many top X map leaderboards is"))
		{
			Log.WriteVerbose("Embed is not leaderboards count. Cancelling process.");
			return;
		}

		if (msg.Channel is not SocketGuildChannel guildChannel)
		{
			// TODO: handle direct message response
			Log.WriteVerbose("Direct messages method not yet implemented.");
			return;
		}

		SocketCommandContext context = new SocketCommandContext(client, msg);
		ReturnMessages[] responses;

		try
		{
			Log.WriteVerbose("Calculating leaderboards count from first embed.");
			responses = await Counter.UserLeaderboardsCountBathbotAsync(msg.Embeds.First(), guildChannel.Guild);
		}
		catch (SendMessageException e)
		{
			Log.WriteVerbose("Send message signal received. Sending message and cancelling process.");
			_ = await context.Channel.SendMessageAsync(e.IsError ? $"**Error:** {e.Draft}" : e.Draft);
			return;
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			_ = await context.Channel.SendMessageAsync("**Error:** Unhandled client error occurred.");
			return;
		}

		Log.WriteVerbose("Sending responses.");
		foreach (ReturnMessages response in responses)
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
			Log.WriteDebug("Bot is not mentioned.");
			return;
		}

		Log.WriteVerbose("Creating context and executing command.");

		SocketCommandContext context = new SocketCommandContext(client, msg);

		Discord.Commands.IResult result = await commandService.ExecuteAsync(context: context, argPos: argPos, services: null);

		if (result.Error != CommandError.Exception)
		{
			// command processing complete
			return;
		}

		if (result is Discord.Commands.ExecuteResult execResult)
		{
			Exception e = execResult.Exception;

			if (e is SendMessageException ex)
			{
				await SendMessage(context, $"{(ex.IsError ? "**Error:** " : string.Empty)}{ex.Draft}");
				return;
			}

			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			await SendMessage(context, $"**Error:** Unhandled client error occurred.");
		}
	}
}