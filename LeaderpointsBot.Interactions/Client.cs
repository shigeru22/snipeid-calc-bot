using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public class Client
{
	private DiscordSocketClient client;

	private string botToken;

	public Client(string botToken)
	{
		client = new();
		this.botToken = botToken;

		client.Log += Log.Write;
		client.Ready += OnReady;

		Log.WriteVerbose("Client", "Client initialized using botToken parameter.");
	}

	public async Task Run()
	{
		await Log.WriteVerbose("Run", "Start client using specified botToken.");

		await client.LoginAsync(Discord.TokenType.Bot, botToken);
		await client.StartAsync();

		await Task.Delay(-1);
	}

	private async Task OnReady()
	{
		await Log.WriteVerbose("OnReady", "Client entered ready state.");

		DateTime startTime = DateTime.Now;

		await Log.WriteInfo("Main", "Start initializing bot interaction commands.");

		try
		{
			await Log.WriteVerbose("OnReady", "Executing slash commands creation.");
			await SlashCommandsFactory.CreateSlashCommands(client);

			await Log.WriteVerbose("OnReady", "Executing context commands creation.");
			await ContextCommandsFactory.CreateUserContextCommands(client);
		}
		catch (Exception e)
		{
			// TODO: determine application command creation errors

			await Log.WriteCritical("OnReady", $"Unhandled error occurred while creating command. Exception details below.\n{ e.ToString() }");

			await Log.WriteVerbose("OnReady", "Exiting with code 1.");
			Environment.Exit(1);
		}

		DateTime endTime = DateTime.Now;

		await Log.WriteInfo("OnReady", $"Operation completed in { Math.Round((endTime - startTime).TotalSeconds, 3) } seconds.");

		await Log.WriteVerbose("OnReady", "Exiting with code 0.");
		Environment.Exit(0);
	}
}