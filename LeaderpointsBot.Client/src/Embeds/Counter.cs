using Discord;
using LeaderpointsBot.Client.Exceptions.Embeds;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Embeds;

public static class Counter
{
	public static Embed CreateCountEmbed(string osuUsername, int[,] topsCount, bool useRespektive = false)
	{
		int points = CalculateTopPoints(topsCount, useRespektive);
		string description;

		// since CalculateTopPoints() throw exception on invalid data,
		// these arrays could be assured without rechecking
		int[] ranks = !useRespektive ? new[] { 1, 8, 15, 25, 50 } : new[] { 1, 8, 25, 50 };
		int[] tops = ArrayOperations.SearchRankAtTopsArray(topsCount, ranks);

		if(!useRespektive)
		{
			description = "```\n" +
				$"{ tops[0] * 5, -6 } = { $"{ tops[0] } x 5", 19 }\n" +
				$"{ (tops[1] - tops[0]) * 3, -6 } = { $"({ tops[1] } - { tops[0] }) x 3", 19 }\n" +
				$"{ (tops[2] - tops[1]) * 2, -6 } = { $"({ tops[2] } - { tops[1] }) x 2", 19 }\n" +
				$"{ tops[3] - tops[2], -6 } = { $"({ tops[3] } - { tops[2] }) x 1", 19 }\n" +
				$"{ tops[4] - tops[3], -6 } = { $"({ tops[4] } - { tops[3] }) x 1", 19 }\n" +
				"```\n" +
				$"= **{ points }** points.";
		}
		else
		{
			// TODO: create respektive warning settings option
			description = "```\n" + 
				$"{ tops[0] * 5, -6 } = { $"{ tops[0] } x 5", 19 }\n" +
				$"{ (tops[1] - tops[0]) * 3, -6 } = { $"({ tops[1] } - { tops[0] }) x 3", 19 }\n" +
				$"{ tops[2] - tops[1], -6 } = { $"({ tops[2] } - { tops[1] }) x 1", 19 }\n" +
				$"{ tops[3] - tops[2], -6 } = { $"({ tops[3] } - { tops[2] }) x 1", 19 }\n" +
				"```\n" +
				$"= **{ points }** points.";
		}

		return new EmbedBuilder().WithTitle($"Points for {osuUsername}:")
			.WithDescription(description)
			.WithColor(238, 229, 229)
			.Build();
	}

	public static int CalculateTopPoints(int[,] topsCount, bool useRespektive = false)
	{
		int[] ranks = !useRespektive ? new[] { 1, 8, 15, 25, 50 } : new[] { 1, 8, 25, 50 };
		int[] tops = ArrayOperations.SearchRankAtTopsArray(topsCount, ranks);

		switch(useRespektive)
		{
			case false when tops.Length != 5: // fallthrough
			case true when tops.Length != 4:
				throw new InvalidEmbedDescriptionException();
			case true: // respektive
				return tops[0] * 5 + (tops[1] - tops[0]) * 3 + (tops[2] - tops[1]) + (tops[3] - tops[2]);
			default: // osu!stats
				return tops[0] * 5 + (tops[1] - tops[0]) * 3 + (tops[2] - tops[1]) * 2 + (tops[3] - tops[2]) + (tops[4] - tops[3]);
		}
	}
}
