// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Text.Json;

namespace LeaderpointsBot.Utils;

public partial class Settings
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

	private SettingsTypes.JsonClientSettings client;
	private SettingsTypes.JsonDatabaseSettings database;
	private SettingsTypes.JsonOsuClientSettings osuApi;
	private bool shouldPromptPassword;
	private bool shouldInitializeInteractions;
	private bool shouldInitializeDatabase;

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
}
