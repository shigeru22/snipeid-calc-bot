// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using LeaderpointsBot.Utils.Arguments;

namespace LeaderpointsBot.Utils;

public static class Args
{
	private static Settings.SettingsTypes.EnvironmentSettings tempConfig;

	public static Settings.SettingsTypes.EnvironmentSettings GetConfigurationArguments(object? target = null)
	{
		tempConfig = new Settings.SettingsTypes.EnvironmentSettings();
		ArgumentHandler.HandleArguments(Environment.GetCommandLineArgs()[1..], nameof(Args), BindingFlags.NonPublic | BindingFlags.Static);
		return tempConfig;
	}

	[Argument("t", "bot-token")]
	[Description("Sets Discord bot token.")]
	internal static void UpdateClientBotToken([ArgumentParameter] string value)
	{
		tempConfig.BotToken = value;
	}

	[Argument("r", "use-reply")]
	[Description("Sets whether client should reply after each message commands action.")]
	internal static void UpdateClientUseReply()
	{
		tempConfig.UseReply = true;
	}

	[Argument("u", "use-utc")]
	[Description("Sets whether client should use UTC time for logging.")]
	internal static void UpdateClientUseUTC()
	{
		tempConfig.LogUseUTC = true;
	}

	[Argument("s", "log-severity")]
	[Description("Sets client logging severity (1-5).")]
	internal static void UpdateClientLogSeverity([ArgumentParameter] int value)
	{
		if (value is < 1 or > 5)
		{
			throw new ArgumentException("Invalid program argument.");
		}

		tempConfig.LogSeverity = value;
	}

	[Argument("dh", "db-hostname")]
	[Description("Sets database hostname.")]
	internal static void UpdateDatabaseHostname([ArgumentParameter] string value)
	{
		tempConfig.DatabaseHostname = value;
	}

	[Argument("dt", "db-port")]
	[Description("Sets database port.")]
	internal static void UpdateDatabasePort([ArgumentParameter] int value)
	{
		tempConfig.DatabasePort = value;
	}

	[Argument("du", "db-username")]
	[Description("Sets database username.")]
	internal static void UpdateDatabaseUsername([ArgumentParameter] string value)
	{
		tempConfig.DatabaseUsername = value;
	}

	[Argument("dp", "db-password")]
	[Description("Prompts for database password.")]
	internal static void UpdateDatabasePassword()
	{
		tempConfig.ShouldPromptPassword = true;
	}

	[Argument("dp", "db-password")]
	[Description("Sets database password directly in plain text.")]
	internal static void UpdateDatabasePassword([ArgumentParameter] string value)
	{
		tempConfig.DatabasePassword = value;
	}

	[Argument("dn", "db-name")]
	[Description("Sets database name.")]
	internal static void UpdateDatabaseName([ArgumentParameter] string value)
	{
		tempConfig.DatabaseName = value;
	}

	[Argument("dc", "db-cert")]
	[Description("Sets database certificate path.")]
	internal static void UpdateDatabaseCAPath([ArgumentParameter] string value)
	{
		tempConfig.DatabaseCAFilePath = value;
	}

	[Argument("oc", "osu-clientid")]
	[Description("Sets osu! client ID.")]
	internal static void UpdateOsuApiClientID([ArgumentParameter] int value)
	{
		tempConfig.OsuApiClientID = value;
	}

	[Argument("os", "osu-clientsecret")]
	[Description("Sets osu! client secret.")]
	internal static void UpdateOsuApiClientSecret([ArgumentParameter] string value)
	{
		tempConfig.OsuApiClientSecret = value;
	}

	[Argument("or", "osu-use-respektive")]
	[Description("Sets whether should use respektive's osu!stats API.")]
	internal static void UpdateOsuApiUseRespektive()
	{
		tempConfig.UseRespektiveApi = true;
	}

	[Argument("h", "help")]
	[Description("Prints this help message.")]
	internal static void ShowHelpMessage()
	{
		tempConfig.ShouldOutputHelp = true;
	}

	[Argument("i", "init-interactions")]
	[Description("Initializes client interactions.")]
	internal static void UpdateInitializeInteractions()
	{
		tempConfig.ShouldInitializeInteractions = true;
	}

	[Argument("d", "init-db")]
	[Description("Initializes database.")]
	internal static void UpdateInitializeDatabase()
	{
		tempConfig.ShouldInitializeDatabase = true;
	}
}
