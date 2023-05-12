// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

// ReSharper disable InconsistentlySynchronizedField

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public class Client
{
	private readonly DiscordSocketClient client;

	private readonly string botToken;

	private readonly object exitMutex = new object();
	private readonly CancellationTokenSource delayToken = new CancellationTokenSource();
	private readonly CancellationTokenSource initDelayToken = new CancellationTokenSource();

	private readonly Handler handler;

	public Client(string botToken)
	{
		Log.WriteVerbose("Client instantiated. Setting client config.");

		client = new DiscordSocketClient(new DiscordSocketConfig()
		{
			UseInteractionSnowflakeDate = false,
			GatewayIntents = GatewayIntents.Guilds | // get channel information in contexts
				GatewayIntents.GuildMessages | // send messages for text-based commands
				GatewayIntents.DirectMessages | // get direct messages for text-based commands
				GatewayIntents.MessageContent // read message contents for text-based commands
		});
		this.botToken = botToken;

		Log.WriteVerbose("Instantiating event handlers.");

		handler = new Handler(client);

		if (!(Settings.Instance.ShouldInitializeInteractions || Settings.Instance.ShouldInitializeDatabase))
		{
			Log.WriteVerbose("Registering client events.");

			client.MessageReceived += handler.OnNewMessage;
			client.SlashCommandExecuted += handler.OnInvokeInteraction;
			client.UserCommandExecuted += handler.OnInvokeInteraction;
			client.JoinedGuild += handler.OnJoinGuild;
		}
		else
		{
			client.Ready += OnInitializerReady;
		}
		client.Log += Log.WriteAsync;

		Log.WriteVerbose("Registering process events.");

		Console.CancelKeyPress += OnProcessExit;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		Log.WriteVerbose("Client initialized.");
	}

	public async Task Run()
	{
		Log.WriteVerbose("Initializing command services.");

		await handler.InitializeCommandServiceAsync();
		await handler.InitializeInteractionServiceAsync();

		Log.WriteVerbose("Start client using specified botToken.");

		await client.LoginAsync(TokenType.Bot, botToken);
		await client.StartAsync();

		Log.WriteVerbose("Client started. Awaiting process indefinitely.");
		await Task.Delay(-1, delayToken.Token);
	}

	public async Task Initializer()
	{
		if (Settings.Instance.ShouldInitializeInteractions)
		{
			Log.WriteVerbose("Initializing interactions service.");
			await handler.InitializeInteractionServiceAsync();

			Log.WriteVerbose("Start client using specified botToken.");

			await client.LoginAsync(TokenType.Bot, botToken);
			await client.StartAsync();

			if (client.ConnectionState != ConnectionState.Connected)
			{
				SpinWait.SpinUntil(() => client.ConnectionState == ConnectionState.Connected);
			}

			await Initialize.CreateInteractionsAsync(handler);
		}

		if (Settings.Instance.ShouldInitializeDatabase)
		{
			await Initialize.CreateDatabaseAsync();
		}

		Environment.Exit(0);
	}

	private Task OnInitializerReady()
	{
		lock (exitMutex)
		{
			initDelayToken.Cancel();
		}

		return Task.CompletedTask;
	}

	private void OnProcessExit()
	{
		lock (exitMutex)
		{
			Log.WriteVerbose("Method called. Logging out client.");

			_ = Task.Run(async () =>
			{
				await client.StopAsync();
				await client.LogoutAsync();
			});

			Log.WriteVerbose("Client logged out.");

			delayToken.Cancel();
		}
	}

	private void OnProcessExit(object? o, EventArgs e) => OnProcessExit();
}
