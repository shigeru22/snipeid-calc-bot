using System.Text.Json;

namespace LeaderpointsBot.Utils;

public class Settings
{
	private static readonly string DEFAULT_SETTINGS_PATH = "appsettings.json";

	public static class SettingsTypes
	{
		public struct JsonClientSettings
		{
			public string BotToken { get; set; }
			public JsonClientLoggingSettings Logging { get; set; }
		}

		public struct JsonClientLoggingSettings
		{
			public bool UseUTC { get; set; }
			public int LogSeverity { get; set; }
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
			public int ClientId { get; set; }
			public string ClientSecret { get; set; }
		}

		public struct JsonSettings
		{
			public JsonClientSettings Client { get; set; }
			public JsonDatabaseSettings Database { get; set; }
			public JsonOsuClientSettings OsuApi { get; set; }
		}
	}

	public SettingsTypes.JsonClientSettings Client { get; private set; }
	public SettingsTypes.JsonDatabaseSettings Database { get; private set; }
	public SettingsTypes.JsonOsuClientSettings OsuApi { get; private set; }

	private static readonly Settings instance = new(DEFAULT_SETTINGS_PATH);

	public static Settings Instance { get => instance; }

	private Settings(string settingsPath)
	{
		JsonSerializerOptions options = new()
		{
			PropertyNameCaseInsensitive = true
		};

		SettingsTypes.JsonSettings temp = JsonSerializer.Deserialize<SettingsTypes.JsonSettings>(File.ReadAllText(settingsPath), options);

		Client = temp.Client;
		Database = temp.Database;
		OsuApi = temp.OsuApi;
	}
}