// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;

namespace LeaderpointsBot.Client.Embeds;

public static class User
{
	public static Embed CreateLinkedEmbed(string userDiscordId, string osuUsername, int osuId)
	{
		string osuUserLink = $"https://osu.ppy.sh/users/{osuId}";
		string osuUserImageLink = $"https://a.ppy.sh/{osuId}";

		return new EmbedBuilder().WithTitle("osu! User Linkage")
			.WithDescription($"Linked <@{userDiscordId}> to osu! user **[{osuUsername}]({osuUserLink})**.\n" +
				"Welcome to osu!leaderpoints!")
			.WithThumbnailUrl(osuUserImageLink)
			.WithColor(238, 229, 229)
			.Build();
	}
}
