// ReSharper disable InconsistentlySynchronizedField

using Discord;
using Discord.WebSocket;
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
		client = new DiscordSocketClient(new DiscordSocketConfig()
		{
			GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
		});
		this.botToken = botToken;

		Console.CancelKeyPress += OnProcessExit;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		client.Log += Log.Write;

		Log.WriteVerbose("Client", "Client initialized using botToken parameter.");
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
