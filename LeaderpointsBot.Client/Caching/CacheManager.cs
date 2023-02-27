// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Client.Caching.Objects;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Caching;

public class CacheManager
{
	// reminder: this class acts as the MemoryCache
	// wrapper as a separation of concern.

	private static readonly CacheManager instance = new CacheManager();

	public static CacheManager Instance => instance;

	private readonly MemoryCache cache;

	private readonly GuildCache guildCacheInstance;

	public GuildCache GuildCacheInstance => guildCacheInstance;

	private CacheManager()
	{
		Log.WriteVerbose("CacheManager instance created. Using default MemoryCache instance.");

		cache = MemoryCache.Default;
		guildCacheInstance = new GuildCache(cache);

		Log.WriteVerbose("Cache instances created.");
	}

	public static CacheItemPolicy GenerateCachePolicy() => new CacheItemPolicy()
	{
		AbsoluteExpiration = DateTime.Now.AddDays(1D)
	};

	public static CacheItemPolicy GenerateCachePolicy(double expSeconds) => new CacheItemPolicy()
	{
		AbsoluteExpiration = DateTime.Now.AddSeconds(expSeconds)
	};
}
