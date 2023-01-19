using System.Text.RegularExpressions;
using LeaderpointsBot.Utils.Exceptions.Parser;

namespace LeaderpointsBot.Utils;

public static class Parser
{
	public static string ParseUsernameFromBathbotEmbedTitle(string embedTitle)
	{
		try
		{
			return embedTitle.Replace("In how many top X map leaderboards is ", "").Replace("?", "");
		}
		catch (NullReferenceException)
		{
			throw new InvalidEmbedTitleException();
		}
	}

	public static int ParseOsuIDFromBathbotEmbedLink(string embedLink)
	{
		string[] temp = new Regex("http(s)?://osu.ppy.sh/u(sers)?/").Replace(embedLink, "").Split("/");

		if(temp.Length < 1 || string.IsNullOrWhiteSpace(temp[0]) || !int.TryParse(temp[0], out int ret))
		{
			throw new InvalidDataException("Invalid embedLink to be parsed.");
		}

		return ret;
	}

	public static int[,] ParseTopPointsFromBathbotEmbedDescription(string embedDescription)
	{
		string[] strTops = new Regex("\n?```\n?").Replace(embedDescription, "").Split("\n");
		List<int[]> arrTops = strTops.Select(strTop => strTop.Replace("Top ", "") // 15 :  1,242   #198
				.Replace(" ", "") // 15:1,242#198
				.Replace(",", "") // 15:1242#198
				.Split("#")[0] // 15:1242
				.Split(':'))
			.Select(temp => new[] { int.Parse(temp[0]), int.Parse(temp[1]) })
			.ToList();

		int arrTopsLength = arrTops.Count;
		int[,] ret = new int[arrTopsLength, 2];
		for(int i = 0; i < arrTopsLength; i++)
		{
			ret[i, 0] = arrTops[i][0];
			ret[i, 1] = arrTops[i][1];
		}

		return ret;
	}
}
