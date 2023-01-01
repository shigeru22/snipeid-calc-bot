using Discord.WebSocket;
using LeaderpointsBot.Client.Utils;

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
	}

	public async Task Run()
	{
		await client.LoginAsync(Discord.TokenType.Bot, botToken);
		await client.StartAsync();

		await Task.Delay(-1);
	}
}
