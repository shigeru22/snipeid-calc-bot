namespace LeaderpointsBot.Utils;

public static class ArrayOperations
{
	public static int[] SearchRankAtTopsArray(int[,] topsCount, IReadOnlyList<int> ranks)
	{
		List<int> tempTops = new();

		int ranksLength = ranks.Count;
		for(int i = 0; i < ranksLength; i++)
		{
			tempTops.Add(SearchRankArray(topsCount, ranks[i]));
		}

		return tempTops.ToArray();
	}

	public static int[] SearchRankAtTopsArray(List<int[]> topsCount, IReadOnlyList<int> ranks)
	{
		List<int> tempTops = new();

		int ranksLength = ranks.Count;
		for(int i = 0; i < ranksLength; i++)
		{
			tempTops.Add(SearchRankArray(topsCount, ranks[i]));
		}

		return tempTops.ToArray();
	}

	private static int SearchRankArray(int[,] topsCount, int rank)
	{
		int min = 0;
		int max = topsCount.Length - 1;

		// binary search anyone?
		while (min <= max) {
			int mid = (min + max) / 2;

			if (rank == topsCount[mid, 0])
			{
				return topsCount[mid, 1];
			}

			if (rank < topsCount[mid, 0])
			{
				max = mid - 1;
			}
			else
			{
				min = mid + 1;
			}
		}

		throw new KeyNotFoundException("No ranks found in topsCount array.");
	}

	private static int SearchRankArray(List<int[]> topsCount, int rank)
	{
		int min = 0;
		int max = topsCount.Count - 1;

		// binary search anyone?
		while (min <= max) {
			int mid = (min + max) / 2;

			if (rank == topsCount[mid][0])
			{
				return topsCount[mid][1];
			}

			if (rank < topsCount[mid][0])
			{
				max = mid - 1;
			}
			else
			{
				min = mid + 1;
			}
		}

		throw new KeyNotFoundException("No ranks found in topsCount list.");
	}
}
