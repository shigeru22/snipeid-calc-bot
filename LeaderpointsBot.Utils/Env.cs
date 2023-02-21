// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils;

public static class Env
{
	private static readonly string envPrefix = "LPB_";

	// key used for the dictionary is based on argument keys,
	// for easier referencing while querying help message
	internal static readonly Dictionary<string, string> environmentKeys = new Dictionary<string, string>()
	{
		{ "t", "BOT_TOKEN" },
		{ "r", "USE_REPLY" },
		{ "u", "LOG_USEUTC" },
		{ "s", "LOG_SEVERITY" },
		{ "dh", "DB_HOST" },
		{ "dt", "DB_PORT" },
		{ "du", "DB_USERNAME" },
		{ "dp", "DB_PASSWORD" },
		{ "dn", "DB_NAME" },
		{ "dc", "DB_CAPATH" },
		{ "oc", "OSUAPI_CLIENT_ID" },
		{ "os", "OSUAPI_CLIENT_SECRET" },
		{ "or", "OSUSTATS_USE_RESPEKTIVE" }
	};

	internal static string? GetEnvironmentKeyByShortFlag(string shortFlag) => environmentKeys.TryGetValue(shortFlag, out string? ret) ? ret : null;

	public static Settings.SettingsTypes.EnvironmentSettings RetrieveEnvironmentData()
	{
		string? envBotToken = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["t"]}");
		string? envUseReply = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["r"]}");
		string? envLogUseUtc = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["u"]}");
		string? envLogSeverity = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["s"]}");
		string? envDatabaseHostname = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["dh"]}");
		string? envDatabasePort = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["dt"]}");
		string? envDatabaseUsername = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["du"]}");
		string? envDatabasePassword = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["dp"]}");
		string? envDatabaseName = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["dn"]}");
		string? envDatabaseCAFilePath = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["dc"]}");
		string? envOsuApiClientID = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["oc"]}");
		string? envOsuApiClientSecret = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["os"]}");
		string? envUseRespektiveApi = Environment.GetEnvironmentVariable($"{envPrefix}{environmentKeys["or"]}");

		bool? shouldUseReply;
		bool? logUseUtc;
		int? logSeverity = null;
		int? databasePort = null;
		int? osuApiClientId = null;
		bool? useRespektiveApi;

		shouldUseReply = envUseReply != null ? envUseReply.Equals("1") || envUseReply.Equals("true") : null;
		logUseUtc = envLogUseUtc != null ? envLogUseUtc.Equals("1") || envLogUseUtc.Equals("true") : null;

		try
		{
			logSeverity = envLogSeverity != null ? int.Parse(envLogSeverity) : null;
		}
		catch (FormatException)
		{
			Log.WriteCritical($"Invalid {envPrefix}{environmentKeys["LogSeverity"]}: Value must be number.");
			Environment.Exit(1);
		}

		try
		{
			databasePort = envDatabasePort != null ? int.Parse(envDatabasePort) : null;
		}
		catch (FormatException)
		{
			Log.WriteCritical($"Invalid {envPrefix}{environmentKeys["DatabasePort"]}: Value must be number.");
			Environment.Exit(1);
		}

		try
		{
			osuApiClientId = envOsuApiClientID != null ? int.Parse(envOsuApiClientID) : null;
		}
		catch (FormatException)
		{
			Log.WriteCritical($"Invalid {envPrefix}{environmentKeys["OsuApiClientID"]}: Value must be number.");
			Environment.Exit(1);
		}

		useRespektiveApi = envUseRespektiveApi != null ? envUseRespektiveApi.Equals("1") || envUseRespektiveApi.Equals("true") : null;

		return new Settings.SettingsTypes.EnvironmentSettings()
		{
			BotToken = envBotToken,
			UseReply = shouldUseReply,
			LogUseUTC = logUseUtc,
			LogSeverity = logSeverity,
			DatabaseHostname = envDatabaseHostname,
			DatabasePort = databasePort,
			DatabaseUsername = envDatabaseUsername,
			DatabasePassword = envDatabasePassword,
			DatabaseName = envDatabaseName,
			DatabaseCAFilePath = envDatabaseCAFilePath,
			OsuApiClientID = osuApiClientId,
			OsuApiClientSecret = envOsuApiClientSecret,
			UseRespektiveApi = useRespektiveApi,
			ShouldOutputHelp = null,
			ShouldInitializeInteractions = null,
			ShouldInitializeDatabase = null
		};
	}
}
