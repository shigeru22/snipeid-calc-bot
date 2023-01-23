// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using LeaderpointsBot.Api;
using LeaderpointsBot.Database;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public static class Program
{
	[SuppressMessage("csharp", "IDE0060", Justification = "Reviewed: Will unify interaction creation project as client argument.")]
	public static async Task Main(string[] args)
	{
		await Log.WriteInfo("Main", "Program started."); // test

		await Log.WriteVerbose("Main", "Setting up ApiFactory.");

		ApiFactory.Instance.OsuApiInstance.Token.ClientID = Settings.Instance.OsuApi.ClientID;
		ApiFactory.Instance.OsuApiInstance.Token.ClientSecret = Settings.Instance.OsuApi.ClientSecret;

		await Log.WriteVerbose("Main", "Setting up DatabaseFactory.");

		DatabaseFactory.Instance.SetConfig(new DatabaseConfig()
		{
			HostName = Settings.Instance.Database.HostName,
			Port = Settings.Instance.Database.Port,
			Username = Settings.Instance.Database.Username,
			Password = Settings.Instance.Database.Password,
			DatabaseName = Settings.Instance.Database.DatabaseName,
			CAFilePath = Settings.Instance.Database.CAFilePath,
		});

		await Log.WriteVerbose("Main", "Starting up client.");

		Client client = new Client(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
