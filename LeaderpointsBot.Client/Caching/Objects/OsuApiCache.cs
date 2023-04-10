// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Caching.Objects;

public class OsuApiCache : CacheWrapperBase
{
	private const string CACHE_KEY_PREFIX = "OSU_API"; // format: OSU_API_(osu! user ID)

	public OsuApiCache(MemoryCache cache) : base(cache)
	{
		Log.WriteVerbose("OsuApiCache instance created.");
	}

	public OsuDataTypes.OsuApiUserResponseData? GetOsuUserCache(int osuId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{osuId}";

		try
		{
			OsuDataTypes.OsuApiUserResponseData osuUser = GetCache<OsuDataTypes.OsuApiUserResponseData>(keyPrefix);
			return osuUser;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public void AddOsuUserCache(int osuId, OsuDataTypes.OsuApiUserResponseData osuUserData)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{osuId}", osuUserData);

	public void SetOsuUserCache(int osuId, OsuDataTypes.OsuApiUserResponseData osuUserData)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{osuId}", osuUserData);

	public void RemoveOsuUserCache(int osuId) => RemoveFromCache($"{CACHE_KEY_PREFIX}_{osuId}");
}
