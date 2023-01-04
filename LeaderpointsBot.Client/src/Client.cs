// ReSharper disable InconsistentlySynchronizedField

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Messages;
using LeaderpointsBot.Client.Interactions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public class Client
{
	private readonly DiscordSocketClient client;

	private readonly string botToken;

	private readonly object exitMutex = new();
	private readonly CancellationTokenSource delayToken = new();

	public Client(string botToken)
	{
		Log.WriteVerbose("Client", "Client instantiated. Setting client config.");

		client = new DiscordSocketClient(new DiscordSocketConfig()
		{
			GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
		});
		this.botToken = botToken;

		Log.WriteVerbose("Client", "Instantiating event factories.");

		MessagesFactory messagesFactory = new(client);
		InteractionsFactory interactionsFactory = new(client);

		Log.WriteVerbose("Client", "Registering process events.");

		Console.CancelKeyPress += OnProcessExit;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		Log.WriteVerbose("Client", "Registering client events.");

		client.Log += Log.Write;
		client.MessageReceived += messagesFactory.OnNewMessage;
		client.SlashCommandExecuted += interactionsFactory.OnInvokeSlashInteraction;
		client.UserCommandExecuted += interactionsFactory.OnInvokeUserContextInteraction;

		Log.WriteVerbose("Client", "Client initialized.");
	}

	public async Task Run()
	{
		await Log.WriteVerbose("Run", "Start client using specified botToken.");

		await client.LoginAsync(Discord.TokenType.Bot, botToken);
		await client.StartAsync();

		await Log.WriteVerbose("Run", "Client started. Awaiting process indefinitely.");
		await Task.Delay(-1, delayToken.Token);
	}

	private void OnProcessExit(object? o, EventArgs e)
	{
		lock (exitMutex)
		{
			Log.WriteVerbose("OnProcessExit", "Method called. Logging out client.");

			client.StopAsync();
			client.LogoutAsync();

			Log.WriteVerbose("OnProcessExit", "Client logged out. Exiting process.");

			delayToken.Cancel();
		}
	}
}
