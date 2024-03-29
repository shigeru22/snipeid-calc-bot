// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Caching.Objects;

public class GuildCache : CacheWrapperBase
{
	private const string CACHE_KEY_PREFIX = "GUILD"; // format: GUILD_(guild Discord ID)

	public GuildCache(MemoryCache cache) : base(cache)
	{
		Log.WriteVerbose("GuildCache instance created.");
	}

	public Servers.ServersTableData? GetDatabaseCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
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
			Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
			return string.IsNullOrWhiteSpace(dbGuildData.LeaderboardsChannelID) ? null : dbGuildData.LeaderboardsChannelID;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public void AddDatabaseCache(string guildDiscordId, Servers.ServersTableData dbGuildData)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}", dbGuildData);

	public void SetDatabaseCache(string guildDiscordId, Servers.ServersTableData dbGuildData)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}", dbGuildData);

	public void SetCountryCodeCache(string guildDiscordId, string? countryCode)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
		dbGuildData.Country = countryCode;
		SetInCache(keyPrefix, dbGuildData);
	}

	public void SetVerifyChannelIDCache(string guildDiscordId, string? verifyChannelId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
		dbGuildData.VerifyChannelID = verifyChannelId;
		SetInCache(keyPrefix, dbGuildData);
	}

	public void SetVerifiedRoleIDCache(string guildDiscordId, string? verifiedRoleId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
		dbGuildData.VerifiedRoleID = verifiedRoleId;
		SetInCache(keyPrefix, dbGuildData);
	}

	public void SetCommandsChannelIDCache(string guildDiscordId, string? commandsChannelId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
		dbGuildData.CommandsChannelID = commandsChannelId;
		SetInCache(keyPrefix, dbGuildData);
	}

	public void SetLeaderboardsChannelIDCache(string guildDiscordId, string? leaderboardsChannelId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		Servers.ServersTableData dbGuildData = GetCache<Servers.ServersTableData>(keyPrefix);
		dbGuildData.LeaderboardsChannelID = leaderboardsChannelId;
		SetInCache(keyPrefix, dbGuildData);
	}

	public void RemoveDatabaseCache(string guildDiscordId) => RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}");
}
