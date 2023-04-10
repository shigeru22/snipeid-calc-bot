// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Api.OsuStats;
using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public class OsuStatsCache : CacheWrapperBase
{
	private const string CACHE_KEY_PREFIX = "OSU_STATS"; // format: OSU_STATS_(osu! username)_(maximum rank)

	public OsuStatsCache(MemoryCache cache) : base(cache)
	{
		Log.WriteVerbose("OsuStateCache instance created.");
	}

	public OsuStatsDataTypes.OsuStatsResponseData? GetOsuStatsCache(string osuUsername, int maxRank)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{osuUsername}_{maxRank}";

		try
		{
			OsuStatsDataTypes.OsuStatsResponseData osuStatsData = GetCache<OsuStatsDataTypes.OsuStatsResponseData>(keyPrefix);
			return osuStatsData;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public void AddOsuStatsCache(string osuUsername, int maxRank, OsuStatsDataTypes.OsuStatsResponseData osuStatsData)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{osuUsername}_{maxRank}", osuStatsData);

	public void SetOsuStatsCache(string osuUsername, int maxRank, OsuStatsDataTypes.OsuStatsResponseData osuStatsData)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{osuUsername}_{maxRank}", osuStatsData);

	public void RemoveOsuStatsCache(string osuUsername, int maxRank) => RemoveFromCache($"{CACHE_KEY_PREFIX}_{osuUsername}_{maxRank}");
}
