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

	[Argument("t")]
	internal void UpdateClientBotToken([ArgumentParameter] string value) => client.BotToken = value;

	[Argument("r")]
	internal void UpdateClientUseReply() => client.UseReply = true;

	[Argument("u")]
	internal void UpdateClientUseUTC()
	{
		SettingsTypes.JsonClientLoggingSettings temp = client.Logging;
		temp.UseUTC = true;
		client.Logging = temp;
	}

	[Argument("s")]
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

	[Argument("dh")]
	internal void UpdateDatabaseHostname([ArgumentParameter] string value) => database.HostName = value;

	[Argument("dt")]
	internal void UpdateDatabasePort([ArgumentParameter] int value) => database.Port = value;

	[Argument("du")]
	internal void UpdateDatabaseUsername([ArgumentParameter] string value) => database.Username = value;

	[Argument("dp")]
	internal void UpdateDatabasePassword() => shouldPromptPassword = true;

	[Argument("dp")]
	internal void UpdateDatabasePassword([ArgumentParameter] string value) => database.Password = value;

	[Argument("dn")]
	internal void UpdateDatabaseName([ArgumentParameter] string value) => database.DatabaseName = value;

	[Argument("dc")]
	internal void UpdateDatabaseCAPath([ArgumentParameter] string value) => database.CAFilePath = value;

	[Argument("oc")]
	internal void UpdateOsuApiClientID([ArgumentParameter] int value) => osuApi.ClientID = value;

	[Argument("os")]
	internal void UpdateOsuApiClientSecret([ArgumentParameter] string value) => osuApi.ClientSecret = value;

	[Argument("or")]
	internal void UpdateOsuApiUseRespektive() => osuApi.UseRespektiveStats = true;

	[Argument("i")]
	internal void UpdateInitializeInteractions() => shouldInitializeInteractions = true;

	[Argument("d")]
	internal void UpdateInitializeDatabase() => shouldInitializeDatabase = true;
}
