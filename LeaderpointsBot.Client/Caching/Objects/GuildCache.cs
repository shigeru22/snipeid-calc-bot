// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Caching.Objects;

public class GuildCache : CacheWrapperBase
{
	private const string CACHE_KEY_PREFIX = "GUILD"; // format: GUILD_(guild Discord ID)

	public GuildCache(MemoryCache cache) : base(cache)
	{
		Log.WriteVerbose("GuildCache instance created.");
	}

	public ServersQuerySchema.ServersTableData? GetDatabaseCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return dbGuildData;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public int? GetDatabaseIDCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return dbGuildData.ServerID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetDiscordIDCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return dbGuildData.DiscordID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetCountryCodeCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return string.IsNullOrWhiteSpace(dbGuildData.Country) ? null : dbGuildData.Country;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetVerifyChannelIDCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return string.IsNullOrWhiteSpace(dbGuildData.VerifyChannelID) ? null : dbGuildData.VerifyChannelID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetVerifiedRoleIDCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return string.IsNullOrWhiteSpace(dbGuildData.VerifiedRoleID) ? null : dbGuildData.VerifiedRoleID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetCommandsChannelIDCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return string.IsNullOrWhiteSpace(dbGuildData.CommandsChannelID) ? null : dbGuildData.CommandsChannelID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetLeaderboardsChannelIDCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			ServersQuerySchema.ServersTableData dbGuildData = GetCache<ServersQuerySchema.ServersTableData>(keyPrefix);
			return string.IsNullOrWhiteSpace(dbGuildData.LeaderboardsChannelID) ? null : dbGuildData.LeaderboardsChannelID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public void AddDatabaseCache(string guildDiscordId, ServersQuerySchema.ServersTableData dbGuildData)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}", dbGuildData);

	public void SetDatabaseCache(string guildDiscordId, ServersQuerySchema.ServersTableData dbGuildData)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}", dbGuildData);

	public void RemoveDatabaseCache(string guildDiscordId) => RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}");
}
