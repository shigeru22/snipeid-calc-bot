// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Api.OsuStats;

public static class OsuStatsDataTypes
{
	public readonly struct OsuStatsRequestRawData
	{
#pragma warning disable IDE1006 // Naming Styles
		public double accMin { get; init; } // set to 0.0
		public double accMax { get; init; } // set to 100.0
		public int rankMin { get; init; } // set to 1
		public int rankMax { get; init; }
		public int sortBy { get; init; } // defaults to 2 (rank), find out others as enum
		public int sortOrder { get; init; } // defaults to 0 (ascending), find out others as enum
		public int page { get; init; } // set to 1, not sure what happens if changed
		public string u1 { get; init; } // username
#pragma warning restore IDE1006 // Naming Styles

		public OsuStatsRequestData ToStandardData()
		{
			return new OsuStatsRequestData()
			{
				AccMin = accMin,
				AccMax = accMax,
				RankMin = rankMin,
				RankMax = rankMax,
				SortBy = sortBy,
				SortOrder = sortOrder,
				Page = page,
				Username = u1
			};
		}
	}

	public readonly struct OsuStatsRequestData
	{
		public double AccMin { get; init; } // set to 0.0
		public double AccMax { get; init; } // set to 100.0
		public int RankMin { get; init; } // set to 1
		public int RankMax { get; init; }
		public int SortBy { get; init; } // defaults to 2 (rank), find out others as enum
		public int SortOrder { get; init; } // defaults to 0 (ascending), find out others as enum
		public int Page { get; init; } // set to 1, not sure what happens if changed
		public string Username { get; init; } // username

		public OsuStatsRequestRawData ToRawData()
		{
			return new OsuStatsRequestRawData()
			{
				accMin = AccMin,
				accMax = AccMax,
				rankMin = RankMin,
				rankMax = RankMax,
				sortBy = SortBy,
				sortOrder = SortOrder,
				page = Page,
				u1 = Username
			};
		}
	}

	public readonly struct OsuStatsResponseData
	{
		public string Username { get; init; }
		public int MaxRank { get; init; }
		public int Count { get; init; }
	}

	public readonly struct OsuStatsRespektiveResponseRawData
	{
#pragma warning disable IDE1006 // Naming Styles
		public string? username { get; init; }
		public int? top1s { get; init; }
		public int? top8s { get; init; }
		public int? top25s { get; init; }
		public int? top50s { get; init; }
#pragma warning restore IDE1006 // Naming Styles

		public OsuStatsRespektiveResponseData ToStandardData()
		{
			return new OsuStatsRespektiveResponseData()
			{
				Username = username,
				Top1 = top1s,
				Top8 = top8s,
				Top25 = top25s,
				Top50 = top50s
			};
		}
	}

	public readonly struct OsuStatsRespektiveResponseData
	{
		public string? Username { get; init; }
		public int? Top1 { get; init; }
		public int? Top8 { get; init; }
		public int? Top25 { get; init; }
		public int? Top50 { get; init; }

		public OsuStatsRespektiveResponseRawData ToRawData()
		{
			return new OsuStatsRespektiveResponseRawData()
			{
				username = Username,
				top1s = Top1,
				top8s = Top8,
				top25s = Top25,
				top50s = Top50
			};
		}
	}
}
