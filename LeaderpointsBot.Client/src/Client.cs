using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public class Client
{
	private DiscordSocketClient client;

	private string botToken;

	public Client(string botToken)
	{
		client = new();
		this.botToken = botToken;

		client.Log += Log.Write;

		Log.WriteVerbose("Client", "Client initialized using botToken parameter.");
	}

	public async Task Run()
	{
		await Log.WriteVerbose("Run", "Start client using specified botToken.");

		await client.LoginAsync(Discord.TokenType.Bot, botToken);
		await client.StartAsync();

		await Log.WriteVerbose("Run", "Client started. Awaiting process indefinitely.");
		await Task.Delay(-1);
	}
}
