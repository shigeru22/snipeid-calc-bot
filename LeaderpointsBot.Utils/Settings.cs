// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Text.Json;
using LeaderpointsBot.Utils.Arguments;
using LeaderpointsBot.Utils.Exceptions.Environments;

namespace LeaderpointsBot.Utils;

public class Settings
{
	public static class SettingsTypes
	{
		// if field is not available on deserialization,
		// would default itself to its default value.
		// (int = 0, bool = false, heap types = null)

		public struct JsonClientSettings
		{
			public string BotToken { get; set; }
			public string[] BotTokens { get; set; }
			public bool UseReply { get; set; }
			public JsonClientLoggingSettings Logging { get; set; }
		}

		public struct JsonClientLoggingSettings
		{
			public bool UseUTC { get; set; }
			public int LogSeverity { get; set; }

			public bool IsVerboseOrDebug() => LogSeverity >= 4; // follows Discord.LogSeverity
		}

		public struct JsonDatabaseSettings
		{
			public string HostName { get; set; }
			public int Port { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public string DatabaseName { get; set; }
			public string CAFilePath { get; set; }
		}

		public struct JsonOsuClientSettings
		{
			public int ClientID { get; set; }
			public string ClientSecret { get; set; }
			public bool UseRespektiveStats { get; set; }
		}

		public struct JsonSettings
		{
			public JsonClientSettings Client { get; set; }
			public JsonDatabaseSettings Database { get; set; }
			public JsonOsuClientSettings OsuApi { get; set; }
		}

		public struct EnvironmentSettings
		{
			public string? BotToken { get; internal set; }
			public string[]? BotTokens { get; internal set; }
			public bool? UseReply { get; internal set; }
			public bool? LogUseUTC { get; internal set; }
			public int? LogSeverity { get; internal set; }
			public string? DatabaseHostname { get; internal set; }
			public int? DatabasePort { get; internal set; }
			public string? DatabaseUsername { get; internal set; }
			public string? DatabasePassword { get; internal set; }
			public string? DatabaseName { get; internal set; }
			public string? DatabaseCAFilePath { get; internal set; }
			public int? OsuApiClientID { get; internal set; }
			public string? OsuApiClientSecret { get; internal set; }
			public bool? UseRespektiveApi { get; internal set; }
			public bool? ShouldOutputHelp { get; internal set; }
			public bool? ShouldPromptPassword { get; internal set; }
			public bool? ShouldInitializeInteractions { get; internal set; }
			public bool? ShouldInitializeDatabase { get; internal set; }
			public bool? ShouldMigrateDatabase { get; internal set; }
		}
	}

	private const string DEFAULT_SETTINGS_PATH = "appsettings.json";

	private static readonly Settings instance = new Settings();

	public static Settings Instance => instance;

	internal SettingsTypes.JsonClientSettings client;
	internal SettingsTypes.JsonDatabaseSettings database;
	internal SettingsTypes.JsonOsuClientSettings osuApi;
	internal bool shouldPromptPassword;
	internal bool shouldInitializeInteractions;
	internal bool shouldInitializeDatabase;
	internal bool shouldMigrateDatabase;
	internal bool shouldOutputHelpMessage;

	public SettingsTypes.JsonClientSettings Client => client;
	public SettingsTypes.JsonDatabaseSettings Database => database;
	public SettingsTypes.JsonOsuClientSettings OsuApi => osuApi;
	public bool ShouldInitializeInteractions => shouldInitializeInteractions;
	public bool ShouldInitializeDatabase => shouldInitializeDatabase;
	public bool ShouldMigrateDatabase => shouldMigrateDatabase;

	private Settings()
	{
		SettingsTypes.EnvironmentSettings envConfig = new SettingsTypes.EnvironmentSettings();

		try
		{
			envConfig = Env.RetrieveEnvironmentData();
		}
		catch (EnvironmentVariableValueException e)
		{
			Console.WriteLine(e.Message);
			Environment.Exit(1);
		}

		SettingsTypes.JsonSettings fileConfig = JsonSerializer.Deserialize<SettingsTypes.JsonSettings>(
			File.ReadAllText(DEFAULT_SETTINGS_PATH),
			new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			}
		);
		SettingsTypes.EnvironmentSettings argConfig = Args.GetConfigurationArguments();

		// Console.WriteLine($"-t = {(argConfig.ShouldOutputHelp == null ? "null" : argConfig.ShouldOutputHelp)}");

		SettingsTypes.JsonSettings mergedConfig = new SettingsTypes.JsonSettings();
		mergedConfig = MergeConfiguration(
			MergeConfiguration(
				MergeConfiguration(mergedConfig, envConfig),
				fileConfig
			),
			argConfig
		);

		client = mergedConfig.Client;
		database = mergedConfig.Database;
		osuApi = mergedConfig.OsuApi;

		if (!VerifyConfiguration())
		{
			Environment.Exit(1);
		}
	}

	public void HandleInstanceArguments()
	{
		if (shouldOutputHelpMessage)
		{
			ArgumentHandler.PrintHelpMessage();
			Environment.Exit(0);
		}

		if (shouldInitializeDatabase && shouldMigrateDatabase)
		{
			Console.WriteLine("Argument error: Both -d and -md must not be used together.");
			Console.WriteLine("Help: LeaderpointsBot.Client --help");
			Environment.Exit(1);
		}

		if (shouldPromptPassword)
		{
			Console.Write($"Enter database password for {Instance.database.Username}: ");
			string temp = Input.ReadHiddenLine();
			database.Password = temp;
		}
	}

	private SettingsTypes.JsonSettings MergeConfiguration(SettingsTypes.JsonSettings target, SettingsTypes.JsonSettings source)
	{
		var tempClient = target.Client;
		var tempDatabase = target.Database;
		var tempOsuApi = target.OsuApi;

		if (!string.IsNullOrWhiteSpace(source.Client.BotToken))
		{
			tempClient.BotToken = source.Client.BotToken;
		}

		if (source.Client.BotTokens != null && source.Client.BotTokens.Length > 0)
		{
			tempClient.BotTokens = source.Client.BotTokens;
		}

		if (source.Client.UseReply)
		{
			tempClient.UseReply = true;
		}

		if (source.Client.Logging.UseUTC || (source.Client.Logging.LogSeverity is >= 1 and <= 5))
		{
			var tempLogging = tempClient.Logging;

			if (source.Client.Logging.UseUTC)
			{
				tempLogging.UseUTC = true;
			}

			if (source.Client.Logging.LogSeverity is >= 1 and <= 5)
			{
				tempLogging.LogSeverity = source.Client.Logging.LogSeverity;
			}

			tempClient.Logging = tempLogging;
		}

		if (!string.IsNullOrWhiteSpace(source.Database.HostName))
		{
			tempDatabase.HostName = source.Database.HostName;
		}

		if (source.Database.Port is >= 1 and <= ushort.MaxValue) // 65535
		{
			tempDatabase.Port = source.Database.Port;
		}

		if (!string.IsNullOrWhiteSpace(source.Database.Username))
		{
			tempDatabase.Username = source.Database.Username;
		}

		if (!string.IsNullOrWhiteSpace(source.Database.Password))
		{
			tempDatabase.Password = source.Database.Password;
		}

		if (!string.IsNullOrWhiteSpace(source.Database.DatabaseName))
		{
			tempDatabase.DatabaseName = source.Database.DatabaseName;
		}

		if (!string.IsNullOrWhiteSpace(source.Database.CAFilePath))
		{
			tempDatabase.CAFilePath = source.Database.CAFilePath;
		}

		if (source.OsuApi.ClientID is >= 1)
		{
			tempOsuApi.ClientID = source.OsuApi.ClientID;
		}

		if (!string.IsNullOrWhiteSpace(source.OsuApi.ClientSecret))
		{
			tempOsuApi.ClientSecret = source.OsuApi.ClientSecret;
		}

		if (source.OsuApi.UseRespektiveStats)
		{
			tempOsuApi.UseRespektiveStats = true;
		}

		return new SettingsTypes.JsonSettings()
		{
			Client = tempClient,
			Database = tempDatabase,
			OsuApi = tempOsuApi
		};
	}

	private SettingsTypes.JsonSettings MergeConfiguration(SettingsTypes.JsonSettings target, SettingsTypes.EnvironmentSettings source)
	{
		var tempClient = target.Client;
		var tempDatabase = target.Database;
		var tempOsuApi = target.OsuApi;

		if (!string.IsNullOrWhiteSpace(source.BotToken))
		{
			tempClient.BotToken = source.BotToken;
		}

		if (source.BotTokens != null && source.BotTokens.Length > 0)
		{
			tempClient.BotTokens = source.BotTokens;
		}

		if (source.UseReply.HasValue && source.UseReply.Value == true)
		{
			tempClient.UseReply = true;
		}

		if (source.LogUseUTC == true || (source.LogSeverity.HasValue && source.LogSeverity.Value is >= 1 and <= 5))
		{
			var tempLogging = tempClient.Logging;

			if (source.LogUseUTC == true)
			{
				tempLogging.UseUTC = true;
			}

			if (source.LogSeverity.HasValue && source.LogSeverity.Value is >= 1 and <= 5)
			{
				tempLogging.LogSeverity = source.LogSeverity.Value;
			}

			tempClient.Logging = tempLogging;
		}

		if (!string.IsNullOrWhiteSpace(source.DatabaseHostname))
		{
			tempDatabase.HostName = source.DatabaseHostname;
		}

		if (source.DatabasePort.HasValue && source.DatabasePort.Value is >= 1 and <= ushort.MaxValue) // 65535
		{
			tempDatabase.Port = source.DatabasePort.Value;
		}

		if (!string.IsNullOrWhiteSpace(source.DatabaseUsername))
		{
			tempDatabase.Username = source.DatabaseUsername;
		}

		if (!string.IsNullOrWhiteSpace(source.DatabasePassword))
		{
			tempDatabase.Password = source.DatabasePassword;
		}

		if (!string.IsNullOrWhiteSpace(source.DatabaseName))
		{
			tempDatabase.DatabaseName = source.DatabaseName;
		}

		if (!string.IsNullOrWhiteSpace(source.DatabaseCAFilePath))
		{
			tempDatabase.CAFilePath = source.DatabaseCAFilePath;
		}

		if (source.OsuApiClientID.HasValue && source.OsuApiClientID.Value is >= 1)
		{
			tempOsuApi.ClientID = source.OsuApiClientID.Value;
		}

		if (!string.IsNullOrWhiteSpace(source.OsuApiClientSecret))
		{
			tempOsuApi.ClientSecret = source.OsuApiClientSecret;
		}

		if (source.UseRespektiveApi.HasValue && source.UseRespektiveApi.Value is true)
		{
			tempOsuApi.UseRespektiveStats = true;
		}

		if (source.ShouldOutputHelp.HasValue && source.ShouldOutputHelp is true)
		{
			shouldOutputHelpMessage = true;
		}

		if (source.ShouldPromptPassword.HasValue && source.ShouldPromptPassword.Value is true)
		{
			shouldPromptPassword = true;
		}

		if (source.ShouldInitializeInteractions.HasValue && source.ShouldInitializeInteractions is true)
		{
			shouldInitializeInteractions = true;
		}

		if (source.ShouldInitializeDatabase.HasValue && source.ShouldInitializeDatabase is true)
		{
			shouldInitializeDatabase = true;
		}

		if (source.ShouldMigrateDatabase.HasValue && source.ShouldMigrateDatabase is true)
		{
			shouldMigrateDatabase = true;
		}

		return new SettingsTypes.JsonSettings()
		{
			Client = tempClient,
			Database = tempDatabase,
			OsuApi = tempOsuApi
		};
	}

	private bool VerifyConfiguration()
	{
		bool isValid = true;

		if (string.IsNullOrWhiteSpace(client.BotToken))
		{
			Console.WriteLine(client.BotTokens.Length);
			if (client.BotTokens.Where(token => !string.IsNullOrWhiteSpace(token)).Count() < 2)
			{
				Console.WriteLine("Configuration error: Bot token must be specified.");
				isValid = false;
			}
		}

		if (client.Logging.LogSeverity == 0)
		{
			var temp = client.Logging;
			temp.LogSeverity = 3; // info
			client.Logging = temp;
		}
		else if (client.Logging.LogSeverity > 5)
		{
			Console.WriteLine("Configuration error: Bot token must be specified.");
			isValid = false;
		}

		if (string.IsNullOrWhiteSpace(database.HostName))
		{
			Console.WriteLine("Configuration error: Logging severity must be specified [1(critical)-5(debug)].");
			isValid = false;
		}

		if (database.Port is < 1 or > ushort.MaxValue)
		{
			Console.WriteLine("Configuration error: Database port must be specified [1-65535].");
			isValid = false;
		}

		if (string.IsNullOrWhiteSpace(database.Username))
		{
			Console.WriteLine("Configuration error: Database username must be specified.");
			isValid = false;
		}

		database.Password ??= string.Empty;

		if (string.IsNullOrWhiteSpace(database.DatabaseName))
		{
			Console.WriteLine("Configuration error: Database name must be specified.");
			isValid = false;
		}

		if (osuApi.ClientID is < 1)
		{
			Console.WriteLine("Configuration error: osu!api client ID must be specified.");
			isValid = false;
		}

		if (string.IsNullOrWhiteSpace(osuApi.ClientSecret))
		{
			Console.WriteLine("Configuration error: osu!api client secret must be specified.");
			isValid = false;
		}

		if (!isValid)
		{
			Console.WriteLine("Configuration error occurred. See help for configuration options.");
		}

		return isValid;
	}
}
