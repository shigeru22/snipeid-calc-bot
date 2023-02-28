// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.Caching;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Caching.Objects;

public class GuildCache : CacheWrapperBase
{
	private const string CACHE_KEY_PREFIX = "GUILD"; // format: GUILD_(guild Discord ID)

	private enum CacheItemSuffixes
	{
		DATABASE_ID = 1,
		DISCORD_ID,
		COUNTRY,
		VERIFY_CHANNEL_ID,
		VERIFIED_ROLE_ID,
		COMMANDS_CHANNEL_ID,
		LEADERBOARDS_CHANNEL_ID
	}

	public GuildCache(MemoryCache cache) : base(cache)
	{
		Log.WriteVerbose("GuildCache instance created.");
	}

	public ServersQuerySchema.ServersTableData? GetDatabaseCache(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		try
		{
			int serverId = GetCache<int>($"{keyPrefix}_{CacheItemSuffixes.DATABASE_ID}");
			string discordId = GetCache<string>($"{keyPrefix}_{CacheItemSuffixes.DISCORD_ID}");
			string country = GetCache<string>($"{keyPrefix}_{CacheItemSuffixes.COUNTRY}");
			string verifyChannelId = GetCache<string>($"{keyPrefix}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}");
			string verifiedRoleId = GetCache<string>($"{keyPrefix}_{CacheItemSuffixes.VERIFIED_ROLE_ID}");
			string commandsChannelId = GetCache<string>($"{keyPrefix}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}");
			string leaderboardsChannelId = GetCache<string>($"{keyPrefix}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}");

			return new ServersQuerySchema.ServersTableData()
			{
				ServerID = serverId,
				DiscordID = discordId,
				Country = string.IsNullOrWhiteSpace(country) ? null : country,
				VerifyChannelID = string.IsNullOrWhiteSpace(verifyChannelId) ? null : verifyChannelId,
				VerifiedRoleID = string.IsNullOrWhiteSpace(verifiedRoleId) ? null : verifiedRoleId,
				CommandsChannelID = string.IsNullOrWhiteSpace(commandsChannelId) ? null : commandsChannelId,
				LeaderboardsChannelID = string.IsNullOrWhiteSpace(leaderboardsChannelId) ? null : leaderboardsChannelId
			};
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public int? GetDatabaseIDCache(string guildDiscordId)
	{
		try
		{
			return GetCache<int>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DATABASE_ID}");
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetDiscordIDCache(string guildDiscordId)
	{
		try
		{
			return GetCache<string>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DISCORD_ID}");
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetCountryCodeCache(string guildDiscordId)
	{
		try
		{
			string ret = GetCache<string>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COUNTRY}");
			return string.IsNullOrWhiteSpace(ret) ? null : ret;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetVerifyChannelIDCache(string guildDiscordId)
	{
		try
		{
			string ret = GetCache<string>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}");
			return string.IsNullOrWhiteSpace(ret) ? null : ret;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetVerifiedRoleIDCache(string guildDiscordId)
	{
		try
		{
			string ret = GetCache<string>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFIED_ROLE_ID}");
			return string.IsNullOrWhiteSpace(ret) ? null : ret;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetCommandsChannelIDCache(string guildDiscordId)
	{
		try
		{
			string ret = GetCache<string>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}");
			return string.IsNullOrWhiteSpace(ret) ? null : ret;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public string? GetLeaderboardsChannelIDCache(string guildDiscordId)
	{
		try
		{
			string ret = GetCache<string>($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}");
			return string.IsNullOrWhiteSpace(ret) ? null : ret;
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	public void AddDatabaseCache(string guildDiscordId, ServersQuerySchema.ServersTableData dbGuildData)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		AddToCache($"{keyPrefix}_{CacheItemSuffixes.DATABASE_ID}", dbGuildData.ServerID); // int
		AddToCache($"{keyPrefix}_{CacheItemSuffixes.DISCORD_ID}", dbGuildData.DiscordID); // string
		AddToCache($"{keyPrefix}_{CacheItemSuffixes.COUNTRY}", dbGuildData.Country ?? string.Empty); // string?
		AddToCache($"{keyPrefix}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}", dbGuildData.VerifyChannelID ?? string.Empty); // string?
		AddToCache($"{keyPrefix}_{CacheItemSuffixes.VERIFIED_ROLE_ID}", dbGuildData.VerifiedRoleID ?? string.Empty); // string?
		AddToCache($"{keyPrefix}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}", dbGuildData.CommandsChannelID ?? string.Empty); // string?
		AddToCache($"{keyPrefix}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}", dbGuildData.LeaderboardsChannelID ?? string.Empty); // string?
	}

	public void AddDatabaseIDCache(string guildDiscordId, int guildDatabaseId)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DATABASE_ID}", guildDatabaseId); // int

	public void AddDiscordIDCache(string guildDiscordId)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DISCORD_ID}", guildDiscordId); // string

	public void AddCountryCodeCache(string guildDiscordId, string? country)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COUNTRY}", country ?? string.Empty); // string?

	public void AddVerifyChannelIDCache(string guildDiscordId, string? verifyChannelId)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}", verifyChannelId ?? string.Empty); // string?

	public void AddVerifiedRoleIDCache(string guildDiscordId, string? verifiedRoleId)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFIED_ROLE_ID}", verifiedRoleId ?? string.Empty); // string?

	public void AddCommandsChannelIDCache(string guildDiscordId, string? commandsChannelId)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}", commandsChannelId ?? string.Empty); // string?

	public void AddLeaderboardsChannelIDCache(string guildDiscordId, string? leaderboardsChannelId)
		=> AddToCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}", leaderboardsChannelId ?? string.Empty); // string?

	public void SetFromDatabaseCache(string guildDiscordId, ServersQuerySchema.ServersTableData dbGuildData)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		SetInCache($"{keyPrefix}_{CacheItemSuffixes.DATABASE_ID}", dbGuildData.ServerID); // int
		SetInCache($"{keyPrefix}_{CacheItemSuffixes.DISCORD_ID}", dbGuildData.DiscordID); // string
		SetInCache($"{keyPrefix}_{CacheItemSuffixes.COUNTRY}", dbGuildData.Country ?? string.Empty); // string?
		SetInCache($"{keyPrefix}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}", dbGuildData.VerifyChannelID ?? string.Empty); // string?
		SetInCache($"{keyPrefix}_{CacheItemSuffixes.VERIFIED_ROLE_ID}", dbGuildData.VerifiedRoleID ?? string.Empty); // string?
		SetInCache($"{keyPrefix}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}", dbGuildData.CommandsChannelID ?? string.Empty); // string?
		SetInCache($"{keyPrefix}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}", dbGuildData.LeaderboardsChannelID ?? string.Empty); // string?
	}

	public void SetDatabaseIDCache(string guildDiscordId, int guildDatabaseId)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DATABASE_ID}", guildDatabaseId); // int

	public void SetDiscordIDCache(string guildDiscordId)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DISCORD_ID}", guildDiscordId); // string

	public void SetCountryCodeCache(string guildDiscordId, string? country)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COUNTRY}", country ?? string.Empty); // string?

	public void SetVerifyChannelIDCache(string guildDiscordId, string? verifyChannelId)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}", verifyChannelId ?? string.Empty); // string?

	public void SetVerifiedRoleIDCache(string guildDiscordId, string? verifiedRoleId)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFIED_ROLE_ID}", verifiedRoleId ?? string.Empty); // string?

	public void SetCommandsChannelIDCache(string guildDiscordId, string? commandsChannelId)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}", commandsChannelId ?? string.Empty); // string?

	public void SetLeaderboardsChannelIDCache(string guildDiscordId, string? leaderboardsChannelId)
		=> SetInCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}", leaderboardsChannelId ?? string.Empty); // string?

	public void RemoveAllCaches(string guildDiscordId)
	{
		string keyPrefix = $"{CACHE_KEY_PREFIX}_{guildDiscordId}";

		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.DATABASE_ID}");
		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.DISCORD_ID}");
		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.COUNTRY}");
		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}");
		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.VERIFIED_ROLE_ID}");
		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}");
		RemoveFromCache($"{keyPrefix}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}");
	}

	public void RemoveDatabaseIDCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DATABASE_ID}");

	public void RemoveDiscordIDCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.DISCORD_ID}");

	public void RemoveCountryCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COUNTRY}");

	public void RemoveVerifyChannelIDCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFY_CHANNEL_ID}");

	public void RemoveVerifiedRoleIDCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.VERIFIED_ROLE_ID}");

	public void RemoveCommandsChannelIDCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.COMMANDS_CHANNEL_ID}");

	public void RemoveLeaderboardsChannelIDCache(string guildDiscordId)
		=> RemoveFromCache($"{CACHE_KEY_PREFIX}_{guildDiscordId}_{CacheItemSuffixes.LEADERBOARDS_CHANNEL_ID}");
}
