// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
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

		commandService.Log += Log.WriteAsync;
		interactionService.Log += Log.WriteAsync;

		commandService.CommandExecuted += OnCommandExecuted;
		interactionService.SlashCommandExecuted += OnSlashExecuted;
		interactionService.ContextCommandExecuted += OnContextExecuted;

		Log.WriteVerbose("Service instances created.");
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

		ChannelType? channelType = msg.Channel.GetChannelType();

		Log.WriteDebug($"Message from {msg.Author.Username}#{msg.Author.Discriminator}: {msg.Content}");

		if (msg is not SocketUserMessage userMsg)
		{
			Log.WriteDebug("Message is not from user.");
			return;
		}

		if (channelType is not ChannelType.Text and not ChannelType.PublicThread and not ChannelType.PrivateThread and not ChannelType.DM)
		{
			Log.WriteDebug("Message is not from text-based channel.");
			return;
		}

		// determine if Bathbot's leaderboard count embed is received
		if (msg.Author.Id.ToString().Equals(BATHBOT_DISCORD_ID))
		{
			Log.WriteDebug("Message is from Bathbot. Handling message.");

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

			if (string.IsNullOrWhiteSpace(botEmbed.Title) || !botEmbed.Title.StartsWith("In how many top X map leaderboards is"))
			{
				Log.WriteVerbose("Embed is not leaderboards count. Cancelling process.");
				return;
			}

			Log.WriteVerbose("Creating context and calculating points.");

			SocketCommandContext bathbotContext = new SocketCommandContext(client, userMsg);
			await Modules.Message.BathbotCountCommand(bathbotContext);

			return;
		}

		Log.WriteDebug("Message is from user and text-based channel. Handling command.");

		int argPos = 0; // TODO: create per-server prefix setting

		if (!userMsg.HasMentionPrefix(client.CurrentUser, ref argPos))
		{
			Log.WriteDebug("Bot is not mentioned.");
			return;
		}

		Log.WriteVerbose("Creating context and executing command.");

		SocketCommandContext context = new SocketCommandContext(client, userMsg);
		_ = await commandService.ExecuteAsync(context: context, argPos: argPos, services: null);
	}

	public async Task OnInvokeInteraction(SocketInteraction cmd)
	{
		// Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		Log.WriteVerbose("Creating context and executing command.");

		SocketInteractionContext context = new SocketInteractionContext(client, cmd);
		_ = await interactionService.ExecuteCommandAsync(context, null);
	}

	public async Task OnJoinGuild(SocketGuild guild)
	{
		Log.WriteDebug($"Joined server: {guild.Name} ({guild.Id})");
		Log.WriteInfo("Running joined server event actions.");

		try
		{
			await Actions.Guild.InsertGuildToDatabase(guild);
		}
		catch (Exception e)
		{
			Log.WriteError($"Unhandled error occurred while executing joined guild action. Exception details below.\n{e}");
			return;
		}

		Log.WriteInfo("Server initialization completed.");
	}

	private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, Discord.Commands.IResult result)
	{
		if (result.Error == null)
		{
			// command processing complete
			return;
		}

		if (result.Error == CommandError.UnknownCommand)
		{
			Log.WriteVerbose("Unknown command. Ignoring message.");
			return;
		}

		if (context is SocketCommandContext commandContext)
		{
			if (result.Error == CommandError.UnmetPrecondition)
			{
				await Actions.Reply.SendToCommandContextAsync(commandContext, new ReturnMessage()
				{
					IsError = true,
					Message = result.ErrorReason
				});

				return;
			}

			if (result is Discord.Commands.ExecuteResult execResult)
			{
				Exception e = execResult.Exception;

				if (e is SendMessageException ex)
				{
					await Actions.Reply.SendToCommandContextAsync(commandContext, new ReturnMessage()
					{
						IsError = true,
						Message = ex.Draft
					});
				}
				else
				{
					Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
					await Actions.Reply.SendToCommandContextAsync(commandContext, new ReturnMessage()
					{
						IsError = true,
						Message = "Unhandled client error occurred."
					});
				}
			}
		}
		else
		{
			Log.WriteCritical("This method is supposed to be used for command events.");
		}
	}

	private async Task OnSlashExecuted(SlashCommandInfo commandInfo, IInteractionContext context, Discord.Interactions.IResult result) => await InteractionErrorHandlingAsync(context, result);

	private async Task OnContextExecuted(ContextCommandInfo commandInfo, IInteractionContext context, Discord.Interactions.IResult result) => await InteractionErrorHandlingAsync(context, result);

	private async Task InteractionErrorHandlingAsync(IInteractionContext context, Discord.Interactions.IResult result)
	{
		if (result.Error == null)
		{
			// interaction processing complete
			return;
		}

		if (result is Discord.Interactions.ExecuteResult execResult)
		{
			if (context is SocketInteractionContext interactionContext)
			{
				Exception e = execResult.Exception;

				if (e is SendMessageException ex)
				{
					await Actions.Reply.SendToInteractionContextAsync(interactionContext, new ReturnMessage()
					{
						IsError = true,
						Message = ex.Draft
					});
				}
				else
				{
					Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
					await Actions.Reply.SendToInteractionContextAsync(interactionContext, new ReturnMessage()
					{
						IsError = true,
						Message = "Unhandled client error occurred."
					});
				}
			}
			else
			{
				Log.WriteCritical("This method is supposed to be used for interaction events.");
			}
		}
	}
}