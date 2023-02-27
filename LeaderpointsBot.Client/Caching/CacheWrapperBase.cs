// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Caching;

public abstract class CacheWrapperBase
{
	private readonly MemoryCache cache;

	protected MemoryCache Cache => cache;

	protected CacheWrapperBase(MemoryCache cache)
	{
		this.cache = cache;
	}

	protected object GetCache(string key)
	{
		Log.WriteVerbose($"Retrieving item (key: {key}) from cache.");

		object ret = cache.Get(key);
		if (ret == null)
		{
			throw new KeyNotFoundException("Cache item with specified key not found.");
		}

		return ret;
	}

	protected T GetCache<T>(string key)
	{
		Log.WriteVerbose($"Retrieving item (key: {key}) from cache.");

		object ret = cache.Get(key);
		if (ret == null)
		{
			throw new KeyNotFoundException("Cache item with specified key not found.");
		}

		return (T)ret;
	}

	protected void AddToCache(string key, object value)
	{
		Log.WriteVerbose($"Inserting item (key: {key}) to cache.");

		if (!cache.Add(new CacheItem(key, value), CacheManager.GenerateCachePolicy()))
		{
			throw new InvalidOperationException("Unable to insert cache item.");
		}
	}

	protected void AddToCache(string key, object value, CacheItemPolicy policy)
	{
		Log.WriteVerbose($"Inserting item (key: {key}) to cache.");

		if (!cache.Add(new CacheItem(key, value), policy))
		{
			throw new InvalidOperationException("Unable to insert cache item.");
		}
	}

	protected void SetInCache(string key, object value)
	{
		Log.WriteVerbose($"Modifying item (key: {key}) in cache.");
		cache.Set(key, value, CacheManager.GenerateCachePolicy());
	}

	protected void SetInCache(string key, object value, CacheItemPolicy policy)
	{
		Log.WriteVerbose($"Modifying item (key: {key}) in cache.");
		cache.Set(key, value, policy);
	}

	protected void RemoveFromCache(string key)
	{
		Log.WriteVerbose($"Removing item (key: {key}) from cache.");
		_ = cache.Remove(key);
	}

	protected bool IsKeyExist(string key)
	{
		Log.WriteVerbose($"Checking for item (key: {key}) in cache.");
		return cache.Contains(key);
	}
}
