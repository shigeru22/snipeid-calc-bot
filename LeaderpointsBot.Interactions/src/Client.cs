// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public class Client
{
	private readonly DiscordSocketClient client;

	private readonly string botToken;

	public Client(string botToken)
	{
		client = new DiscordSocketClient();
		this.botToken = botToken;

		client.Log += Log.WriteAsync;
		client.Ready += OnReady;

		Log.WriteVerbose("Client", "Client initialized using botToken parameter.");
	}

	public async Task Run()
	{
		Log.WriteVerbose("Run", "Start client using specified botToken.");

		await client.LoginAsync(Discord.TokenType.Bot, botToken);
		await client.StartAsync();

		await Task.Delay(-1);
	}

	private async Task OnReady()
	{
		Log.WriteVerbose("OnReady", "Client entered ready state.");

		DateTime startTime = DateTime.Now;

		Log.WriteInfo("Main", "Start initializing bot interaction commands.");

		try
		{
			Log.WriteVerbose("OnReady", "Executing slash commands creation.");
			await SlashCommandsFactory.CreateSlashCommands(client);

			Log.WriteVerbose("OnReady", "Executing context commands creation.");
			await ContextCommandsFactory.CreateUserContextCommands(client);
		}
		catch (Exception e)
		{
			// TODO: determine application command creation errors

			Log.WriteCritical("OnReady", $"Unhandled error occurred while creating command. Exception details below.\n{e}");

			Log.WriteVerbose("OnReady", "Exiting with code 1.");
			Environment.Exit(1);
		}

		DateTime endTime = DateTime.Now;

		Log.WriteInfo("OnReady", $"Operation completed in {Math.Round((endTime - startTime).TotalSeconds, 3)} seconds.");

		Log.WriteVerbose("OnReady", "Exiting with code 0.");
		Environment.Exit(0);
	}
}
