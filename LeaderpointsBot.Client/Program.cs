// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Api;
using LeaderpointsBot.Client.Actions;
using LeaderpointsBot.Client.Caching;
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

		Log.WriteVerbose("Start populating server caches from database.");

		await Cache.PopulateGuildConfigurations();

		Log.WriteVerbose("Starting up client.");

		Client client = new Client(Settings.Instance.Client.BotToken);

		if (Settings.Instance.ShouldInitializeInteractions || Settings.Instance.ShouldInitializeDatabase)
		{
			await client.Initializer();
		}
		else
		{
			await client.Run();
		}
	}
}
