// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Text.RegularExpressions;
using LeaderpointsBot.Utils.Exceptions.Parser;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Utils;

public static class Parser
{
	public static string ParseUsernameFromBathbotEmbedTitle(string embedTitle)
	{
		try
		{
			return embedTitle.Replace("In how many top X map leaderboards is ", string.Empty).Replace("?", string.Empty);
		}
		catch (NullReferenceException e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.UtilParserEmbedTitleError.Message));
			throw new InvalidEmbedTitleException(); // U0101
		}
	}

	public static int ParseOsuIDFromBathbotEmbedLink(string embedLink)
	{
		string[] temp = new Regex("http(s)?://osu.ppy.sh/u(sers)?/").Replace(embedLink, string.Empty).Split("/");

		if (temp.Length < 1 || string.IsNullOrWhiteSpace(temp[0]) || !int.TryParse(temp[0], out int ret))
		{
			throw new InvalidDataException("Invalid embedLink to be parsed."); // U0102
		}

		return ret;
	}

	public static int[,] ParseTopPointsFromBathbotEmbedDescription(string embedDescription)
	{
		string[] strTops = new Regex("\n?```\n?").Replace(embedDescription, string.Empty).Split("\n");
		List<int[]> arrTops = strTops.Select(strTop => strTop.Replace("Top ", string.Empty) // 15 :  1,242   #198
				.Replace(" ", string.Empty) // 15:1,242#198
				.Replace(",", string.Empty) // 15:1242#198
				.Split("#")[0] // 15:1242
				.Split(':'))
			.Select(temp => new[] { int.Parse(temp[0]), int.Parse(temp[1]) })
			.ToList();

		int arrTopsLength = arrTops.Count;
		int[,] ret = new int[arrTopsLength, 2];
		for (int i = 0; i < arrTopsLength; i++)
		{
			ret[i, 0] = arrTops[i][0];
			ret[i, 1] = arrTops[i][1];
		}

		return ret;
	}

	public static int[,] ParseWhatIfArguments(string args)
	{
		string[] strArgsList = args.Split(',');
		List<int[]> arrWhatIfs;
		try
		{
			arrWhatIfs = strArgsList.Select(strArg => strArg.Split('='))
				.Select(temp => new[] { int.Parse(temp[0]), int.Parse(temp[1]) })
				.ToList();
		}
		catch (FormatException)
		{
			throw new InvalidDataException("Invalid argument(s).");
		}

		int arrWhatIfsLength = arrWhatIfs.Count;
		int[,] ret = new int[arrWhatIfsLength, 2];
		for (int i = 0; i < arrWhatIfsLength; i++)
		{
			ret[i, 0] = arrWhatIfs[i][0];
			ret[i, 1] = arrWhatIfs[i][1];
		}

		return ret;
	}
}
