// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Text.Json;
using LeaderpointsBot.Utils.Arguments;

namespace LeaderpointsBot.Utils;

public class Settings
{
	public static class SettingsTypes
	{
		public struct JsonClientSettings
		{
			public string BotToken { get; set; }
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
	}

	private const string DEFAULT_SETTINGS_PATH = "appsettings.json";

	private static readonly Settings instance = new Settings(DEFAULT_SETTINGS_PATH);

	public static Settings Instance => instance;

	internal SettingsTypes.JsonClientSettings client;
	internal SettingsTypes.JsonDatabaseSettings database;
	internal SettingsTypes.JsonOsuClientSettings osuApi;
	internal bool shouldPromptPassword;
	internal bool shouldInitializeInteractions;
	internal bool shouldInitializeDatabase;
	internal bool shouldOutputHelpMessage;

	public SettingsTypes.JsonClientSettings Client => client;
	public SettingsTypes.JsonDatabaseSettings Database => database;
	public SettingsTypes.JsonOsuClientSettings OsuApi => osuApi;
	public bool ShouldInitializeInteractions => shouldInitializeInteractions;
	public bool ShouldInitializeDatabase => shouldInitializeDatabase;

	private Settings(string settingsPath)
	{
		JsonSerializerOptions options = new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true
		};

		SettingsTypes.JsonSettings temp = JsonSerializer.Deserialize<SettingsTypes.JsonSettings>(File.ReadAllText(settingsPath), options);

		client = temp.Client;
		database = temp.Database;
		osuApi = temp.OsuApi;
	}

	internal void PostArgumentHandling()
	{
		if (shouldOutputHelpMessage)
		{
			ArgumentHandler.PrintHelpMessage();
			Environment.Exit(0);
		}

		if (shouldPromptPassword)
		{
			Console.Write($"Enter database password for {Instance.database.Username}: ");
			string temp = Input.ReadHiddenLine();
			database.Password = temp;
		}
	}

	[Argument("h", "help")]
	[Description("Prints this help message.")]
	internal void ShowHelpMessage() => shouldOutputHelpMessage = true;

	[Argument("t", "bot-token")]
	[Description("Sets Discord bot token.")]
	internal void UpdateClientBotToken([ArgumentParameter] string value) => client.BotToken = value;

	[Argument("r", "use-reply")]
	[Description("Sets whether client should reply after each message commands action.")]
	internal void UpdateClientUseReply() => client.UseReply = true;

	[Argument("u", "use-utc")]
	[Description("Sets whether client should use UTC time for logging.")]
	internal void UpdateClientUseUTC()
	{
		SettingsTypes.JsonClientLoggingSettings temp = client.Logging;
		temp.UseUTC = true;
		client.Logging = temp;
	}

	[Argument("s", "log-severity")]
	[Description("Sets client logging severity (1-5).")]
	internal void UpdateClientLogSeverity([ArgumentParameter] int value)
	{
		if (value is < 1 or > 5)
		{
			throw new ArgumentException("Invalid program argument.");
		}

		SettingsTypes.JsonClientLoggingSettings temp = client.Logging;
		temp.LogSeverity = value;
		client.Logging = temp;
	}

	[Argument("dh", "db-hostname")]
	[Description("Sets database hostname.")]
	internal void UpdateDatabaseHostname([ArgumentParameter] string value) => database.HostName = value;

	[Argument("dt", "db-port")]
	[Description("Sets database port.")]
	internal void UpdateDatabasePort([ArgumentParameter] int value) => database.Port = value;

	[Argument("du", "db-username")]
	[Description("Sets database username.")]
	internal void UpdateDatabaseUsername([ArgumentParameter] string value) => database.Username = value;

	[Argument("dp", "db-password")]
	[Description("Sets database password.")]
	internal void UpdateDatabasePassword() => shouldPromptPassword = true;

	[Argument("dp", "db-password")]
	[Description("Sets database password directly in plain text.")]
	internal void UpdateDatabasePassword([ArgumentParameter] string value) => database.Password = value;

	[Argument("dn", "db-name")]
	[Description("Sets database name.")]
	internal void UpdateDatabaseName([ArgumentParameter] string value) => database.DatabaseName = value;

	[Argument("dc", "db-cert")]
	[Description("Sets database certificate path.")]
	internal void UpdateDatabaseCAPath([ArgumentParameter] string value) => database.CAFilePath = value;

	[Argument("oc", "osu-clientid")]
	[Description("Sets osu! client ID.")]
	internal void UpdateOsuApiClientID([ArgumentParameter] int value) => osuApi.ClientID = value;

	[Argument("os", "osu-clientsecret")]
	[Description("Sets osu! client secret.")]
	internal void UpdateOsuApiClientSecret([ArgumentParameter] string value) => osuApi.ClientSecret = value;

	[Argument("or", "osu-use-respektive")]
	[Description("Sets whether should use respektive's osu!stats API.")]
	internal void UpdateOsuApiUseRespektive() => osuApi.UseRespektiveStats = true;

	[Argument("i", "init-interactions")]
	[Description("Initializes client interactions.")]
	internal void UpdateInitializeInteractions() => shouldInitializeInteractions = true;

	[Argument("d", "init-db")]
	[Description("Initializes database.")]
	internal void UpdateInitializeDatabase() => shouldInitializeDatabase = true;
}
