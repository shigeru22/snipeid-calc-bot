// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Api;
using LeaderpointsBot.Client.Actions;
using LeaderpointsBot.Database;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public static class Program
{
	public static async Task Main()
	{
		Settings.Instance.HandleInstanceArguments();

		Log.WriteInfo("Program started.");

		Log.WriteVerbose("Setting up ApiFactory.");

		ApiFactory.Instance.OsuApiInstance.Token.ClientID = Settings.Instance.OsuApi.ClientID;
		ApiFactory.Instance.OsuApiInstance.Token.ClientSecret = Settings.Instance.OsuApi.ClientSecret;

		Log.WriteVerbose("Setting up DatabaseFactory.");

		DatabaseFactory.Instance.SetConfig(new DatabaseConfig()
		{
			HostName = Settings.Instance.Database.HostName,
			Port = Settings.Instance.Database.Port,
			Username = Settings.Instance.Database.Username,
			Password = Settings.Instance.Database.Password,
			DatabaseName = Settings.Instance.Database.DatabaseName,
			CAFilePath = Settings.Instance.Database.CAFilePath,
		});

		Log.WriteInfo("Populating server caches from database.");

		await Cache.PopulateGuildConfigurations();

		// single bot token value always takes precedence
		if (!string.IsNullOrWhiteSpace(Settings.Instance.Client.BotToken))
		{
			Client client = new Client(Settings.Instance.Client.BotToken);

			if (Settings.Instance.ShouldInitializeInteractions || Settings.Instance.ShouldInitializeDatabase)
			{
				Log.WriteVerbose("Initializing client.");
				await client.Initializer();
			}
			else
			{
				Log.WriteVerbose("Starting up client.");
				await client.Run();
			}
		}
		else
		{
			/*
			 * Multiple clients support.
			 * Refrain from using these since this is used for legacy bot support (osu!snipe Indonesia's SnipeID bot)
			 * and I'd like to keep that way for that server.
			 * Keep it undocumented.
			 */

			Client[] clients = Settings.Instance.Client.BotTokens.Select(token => new Client(token)).ToArray();
			int clientsLength = clients.Length;

			if (Settings.Instance.ShouldInitializeInteractions || Settings.Instance.ShouldInitializeDatabase)
			{
				for (int i = 0; i < clientsLength; i++)
				{
					Log.WriteInfo($"Initializing client {i}.");
					await clients[i].Initializer();
				}
			}
			else
			{
				Task[] taskStartClients = clients.Select(client => client.Run()).ToArray();

				Log.WriteInfo($"Starting up {clientsLength} clients.");
				await Task.WhenAll(taskStartClients);
			}
		}
	}
}
