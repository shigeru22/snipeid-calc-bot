// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

// ReSharper disable InconsistentlySynchronizedField

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderpointsBot.Client.Interactions;
using LeaderpointsBot.Client.Messages;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public class Client
{
	private readonly DiscordSocketClient client;
	private readonly CommandService commandService;

	private readonly string botToken;

	private readonly object exitMutex = new object();
	private readonly CancellationTokenSource delayToken = new CancellationTokenSource();

	private readonly MessageHandler messagesFactory;
	private readonly InteractionHandler interactionsFactory;

	public Client(string botToken)
	{
		Log.WriteVerbose("Client instantiated. Setting client config.");

		client = new DiscordSocketClient(new DiscordSocketConfig()
		{
			UseInteractionSnowflakeDate = false,
			GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
		});
		this.botToken = botToken;

		Log.WriteVerbose("Instantiating command service instance.");

		commandService = new CommandService(new CommandServiceConfig()
		{
			LogLevel = LogSeverity.Info,
			CaseSensitiveCommands = false
		});

		Log.WriteVerbose("Instantiating event factories.");

		messagesFactory = new MessageHandler(client, commandService);
		interactionsFactory = new InteractionHandler(client);

		Log.WriteVerbose("Registering process events.");

		Console.CancelKeyPress += OnProcessExit;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		Log.WriteVerbose("Registering client events.");

		client.Log += Log.WriteAsync;
		client.MessageReceived += messagesFactory.OnNewMessage;
		client.SlashCommandExecuted += interactionsFactory.OnInvokeSlashInteraction;
		client.UserCommandExecuted += interactionsFactory.OnInvokeUserContextInteraction;

		Log.WriteVerbose("Client initialized.");
	}

	public async Task Run()
	{
		await messagesFactory.InitializeServiceAsync();

		Log.WriteVerbose("Start client using specified botToken.");

		await client.LoginAsync(TokenType.Bot, botToken);
		await client.StartAsync();

		Log.WriteVerbose("Client started. Awaiting process indefinitely.");
		await Task.Delay(-1, delayToken.Token);
	}

	private void OnProcessExit(object? o, EventArgs e)
	{
		lock (exitMutex)
		{
			Log.WriteVerbose("Method called. Logging out client.");

			_ = Task.Run(async () =>
			{
				await client.StopAsync();
				await client.LogoutAsync();
			});

			Log.WriteVerbose("Client logged out. Exiting process.");

			delayToken.Cancel();
		}
	}
}
