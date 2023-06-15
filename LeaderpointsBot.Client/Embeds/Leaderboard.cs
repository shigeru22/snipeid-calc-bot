// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Text;
using Discord;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Embeds;

public static class Leaderboard
{
	public static Embed CreateLeaderboardEmbed(Users.UsersLeaderboardData[] data, DateTime lastUpdate, int usersLimit = 50, bool useLegacyColor = false)
	{
		string title = $"Top {usersLimit} players based on points count:";
		StringBuilder description = new StringBuilder();
		string footer = $"Last updated: {Date.DateTimeToString(lastUpdate, true)}";

		int dataLength = data.Length;
		for (int i = 0; i < dataLength; i++)
		{
			_ = description.Append($"{i + 1}. {data[i].Username}: {data[i].Points}{(i < dataLength - 1 ? "\n" : string.Empty)}");
			if (i == usersLimit - 1)
			{
				break;
			}
		}

		return new EmbedBuilder().WithTitle(title)
			.WithDescription(description.ToString())
			.WithFooter(footer)
			.WithColor(useLegacyColor ? LegacyBorderColor.Normal : BorderColor.Normal)
			.Build();
	}
}
