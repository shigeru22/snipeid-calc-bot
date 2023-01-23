// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace LeaderpointsBot.Utils;

public class Settings
{
	private const string DefaultSettingsPath = "appsettings.json";

	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:StaticReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "Private static readonly instance names should be lowercased (styling not yet configurable).")]
	private static readonly Settings instance = new Settings(DefaultSettingsPath);

	private SettingsTypes.JsonClientSettings client;
	private SettingsTypes.JsonDatabaseSettings database;
	private SettingsTypes.JsonOsuClientSettings osuApi;

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

	public static Settings Instance { get => instance; }

	public SettingsTypes.JsonClientSettings Client { get => client; }
	public SettingsTypes.JsonDatabaseSettings Database { get => database; }
	public SettingsTypes.JsonOsuClientSettings OsuApi { get => osuApi; }

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
}
