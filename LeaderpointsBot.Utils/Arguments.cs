// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils;

public partial class Settings
{
	public void AddArguments()
	{
		AddArguments(Environment.GetCommandLineArgs()[1..]);
	}

	public void AddArguments(string[] args)
	{
		int argsLength = args.Length;

		for (int i = 0; i < argsLength; i++)
		{
			if (args[i].StartsWith("--"))
			{
				HandleLongArgument(ref i, args[i][2..], GetArgumentElement(args, i + 1));
			}
			else if (args[i].StartsWith("-"))
			{
				HandleShortArgument(ref i, args[i][1..], GetArgumentElement(args, i + 1));
			}
			else
			{
				throw new ArgumentException($"Invalid program argument: {args[i]}");
			}
		}

		if (shouldPromptPassword)
		{
			Console.Write($"Enter password for {database.Username}: ");
			string temp = Input.ReadHiddenLine();
			database.Password = temp;
		}
	}

	private void HandleShortArgument(ref int current, string key, string? value)
	{
		switch (key)
		{
			case "t":
				UpdateClientBotToken(StringNullCheck(value));
				current++;
				return;
			case "r":
				UpdateClientUseReply();
				return;
			case "u":
				UpdateClientUseUTC();
				return;
			case "s":
				UpdateClientLogSeverity(int.Parse(StringNullCheck(value)));
				current++;
				return;
			case "dh":
				UpdateDatabaseHostname(StringNullCheck(value));
				current++;
				return;
			case "dt":
				UpdateDatabasePort(int.Parse(StringNullCheck(value)));
				current++;
				return;
			case "du":
				UpdateDatabaseUsername(StringNullCheck(value));
				current++;
				return;
			case "dp":
				if (string.IsNullOrEmpty(value))
				{
					UpdateDatabasePassword();
				}
				else
				{
					UpdateDatabasePassword(value);
					current++;
				}
				return;
			case "dn":
				UpdateDatabaseName(StringNullCheck(value));
				current++;
				return;
			case "dc":
				UpdateDatabaseCAPath(StringNullCheck(value));
				current++;
				return;
			case "oc":
				UpdateOsuApiClientID(int.Parse(StringNullCheck(value)));
				current++;
				return;
			case "os":
				UpdateOsuApiClientSecret(StringNullCheck(value));
				current++;
				return;
			case "or":
				UpdateOsuApiUseRespektive();
				return;
			case "i":
				UpdateInitializeInteractions();
				return;
			case "d":
				UpdateInitializeDatabase();
				return;
			default:
				throw new ArgumentException($"Invalid program argument: -{key}");
		}
	}

	private void HandleLongArgument(ref int current, string key, string? value = null)
	{
		switch (key)
		{
			case "bot-token":
				UpdateClientBotToken(StringNullCheck(value));
				current++;
				return;
			case "use-reply":
				UpdateClientUseReply();
				return;
			case "utc":
				UpdateClientUseUTC();
				return;
			case "severity":
				UpdateClientLogSeverity(int.Parse(StringNullCheck(value)));
				current++;
				return;
			case "db-hostname":
				UpdateDatabaseHostname(StringNullCheck(value));
				current++;
				return;
			case "db-port":
				UpdateDatabasePort(int.Parse(StringNullCheck(value)));
				current++;
				return;
			case "db-username":
				UpdateDatabaseUsername(StringNullCheck(value));
				current++;
				return;
			case "db-password":
				if (string.IsNullOrEmpty(value))
				{
					UpdateDatabasePassword();
				}
				else
				{
					UpdateDatabasePassword(value);
					current++;
				}
				return;
			case "db-name":
				UpdateDatabaseName(StringNullCheck(value));
				current++;
				return;
			case "db-capath":
				UpdateDatabaseCAPath(StringNullCheck(value));
				current++;
				return;
			case "osu-clientid":
				UpdateOsuApiClientID(int.Parse(StringNullCheck(value)));
				current++;
				return;
			case "osu-clientsecret":
				UpdateOsuApiClientSecret(StringNullCheck(value));
				current++;
				return;
			case "osustats-respektive":
				UpdateOsuApiUseRespektive();
				return;
			case "init-interactions":
				UpdateInitializeInteractions();
				return;
			case "init-db":
				UpdateInitializeDatabase();
				return;
			default:
				throw new ArgumentException($"Invalid program argument: --{key}");
		}
	}

	private void UpdateClientBotToken(string value) => client.BotToken = value;

	private void UpdateClientUseReply() => client.UseReply = true;

	private void UpdateClientUseUTC()
	{
		SettingsTypes.JsonClientLoggingSettings temp = client.Logging;
		temp.UseUTC = true;
		client.Logging = temp;
	}

	private void UpdateClientLogSeverity(int value)
	{
		if (value is < 1 or > 5)
		{
			throw new ArgumentException("Invalid program argument.");
		}

		SettingsTypes.JsonClientLoggingSettings temp = client.Logging;
		temp.LogSeverity = value;
		client.Logging = temp;
	}

	private void UpdateDatabaseHostname(string value) => database.HostName = value;

	private void UpdateDatabasePort(int value) => database.Port = value;

	private void UpdateDatabaseUsername(string value) => database.Username = value;

	private void UpdateDatabasePassword() => shouldPromptPassword = true;

	private void UpdateDatabasePassword(string value) => database.Password = value;

	private void UpdateDatabaseName(string value) => database.DatabaseName = value;

	private void UpdateDatabaseCAPath(string value) => database.CAFilePath = value;

	private void UpdateOsuApiClientID(int value) => osuApi.ClientID = value;

	private void UpdateOsuApiClientSecret(string value) => osuApi.ClientSecret = value;

	private void UpdateOsuApiUseRespektive() => osuApi.UseRespektiveStats = true;

	private void UpdateInitializeInteractions() => shouldInitializeInteractions = true;

	private void UpdateInitializeDatabase() => shouldInitializeDatabase = true;

	private string StringNullCheck(string? value) => !string.IsNullOrEmpty(value) ? value : throw new ArgumentException("Invalid program argument.");

	private string? GetArgumentElement(string[] arr, int index)
	{
		try
		{
			if (string.IsNullOrEmpty(arr[index]))
			{
				return null;
			}

			return arr[index];
		}
		catch (IndexOutOfRangeException)
		{
			return null;
		}
	}
}
