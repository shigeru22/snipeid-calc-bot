// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;

namespace LeaderpointsBot.Client.Embeds;

public static class Configuration
{
	private static readonly IDictionary<string, string> labels = new Dictionary<string, string>()
	{
		{ "Country", "Country" },
		{ "VerifiedRole", "Verified role" },
		{ "CommandsChannel", "Commands channel" },
		{ "LeaderboardsChannel", "Leaderboards channel" }
	};

	private static int tempMinimumWidth = -1;

	private static int LabelMinimumWidth
	{
		get
		{
			if (tempMinimumWidth < 0)
			{
				foreach (KeyValuePair<string, string> label in labels)
				{
					int tempLength = label.Value.Length;
					if (tempMinimumWidth < tempLength)
					{
						tempMinimumWidth = tempLength;
					}
				}
			}

			return tempMinimumWidth;
		}
	}

	public static Embed CreateServerConfigurationEmbed(Structures.Embeds.Configuration.ServerConfigurations data)
	{
		string title = "Current server configuration:";
		string description = "```\n" +
			$"{GenerateCountryConfig(data.CountryCode)}\n" +
			$"{GenerateVerifiedRoleConfig(data.VerifiedRoleName)}\n" +
			$"{GenerateCommandsChannelConfig(data.CommandsChannelName)}\n" +
			$"{GenerateLeaderboardsChannelConfig(data.LeaderboardsChannelName)}\n" +
			"```";
		EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder().WithText(data.GuildName);

		if (!string.IsNullOrWhiteSpace(data.GuildIconURL))
		{
			footerBuilder = footerBuilder.WithIconUrl(data.GuildIconURL);
		}

		return new EmbedBuilder().WithTitle(title)
			.WithDescription(description)
			.WithFooter(footerBuilder)
			.WithColor(BorderColor.Normal)
			.Build();
	}

	private static string GenerateCountryConfig(string? countryCode) => $"{labels["Country"].PadRight(LabelMinimumWidth, ' ')}: {CheckNullData(countryCode)}";
	private static string GenerateVerifiedRoleConfig(string? verifiedRoleName) => $"{labels["VerifiedRole"].PadRight(LabelMinimumWidth, ' ')}: {CheckNullData(verifiedRoleName)}";
	private static string GenerateCommandsChannelConfig(string? channelName) => $"{labels["CommandsChannel"].PadRight(LabelMinimumWidth, ' ')}: {CheckNullData(channelName)}";
	private static string GenerateLeaderboardsChannelConfig(string? channelName) => $"{labels["LeaderboardsChannel"].PadRight(LabelMinimumWidth, ' ')}: {CheckNullData(channelName)}";

	private static string CheckNullData(string? target) => string.IsNullOrWhiteSpace(target) ? "(disabled)" : target;
}
