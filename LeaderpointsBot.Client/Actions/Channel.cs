// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class Channel
{
	public const string SNIPEID_DISCORD_ID = "862267167100502046";

	public static async Task<(bool, string?)> IsClientCommandsAllowedAsync(string guildDiscordId, string channelId)
	{
		Log.WriteVerbose($"Checking whether client commands are allowed in the channel (guild ID {guildDiscordId}, channel ID {channelId}).");

		ServersQuerySchema.ServersTableData? guildData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildDiscordId);
		if (guildData == null)
		{
			Log.WriteVerbose("Cache data not found or expired. Fetching database data.");

			try
			{
				ServersQuerySchema.ServersTableData tempGuildData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guildDiscordId);
				guildData = tempGuildData;
				CacheManager.Instance.GuildCacheInstance.SetFromDatabaseCache(guildDiscordId, tempGuildData);
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		bool isAllowed = guildData.Value.CommandsChannelID == null || guildData.Value.CommandsChannelID.Equals(channelId);
		string? allowedChannelId = guildData.Value.CommandsChannelID;

		return (isAllowed, allowedChannelId);
	}

	public static async Task<(bool, string?)> IsVerifyCommandsAllowedAsync(string guildDiscordId, string channelId)
	{
		Log.WriteVerbose($"Checking whether verification commands are allowed in the channel (guild ID {guildDiscordId}, channel ID {channelId}).");

		ServersQuerySchema.ServersTableData? guildData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildDiscordId);
		if (guildData == null)
		{
			Log.WriteVerbose("Cache data not found or expired. Fetching database data.");

			try
			{
				ServersQuerySchema.ServersTableData tempGuildData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guildDiscordId);
				guildData = tempGuildData;
				CacheManager.Instance.GuildCacheInstance.SetFromDatabaseCache(guildDiscordId, tempGuildData);
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		// check commands channel, then verification
		bool isAllowed = guildData.Value.CommandsChannelID == null || guildData.Value.CommandsChannelID.Equals(channelId);
		if (!string.IsNullOrWhiteSpace(guildData.Value.VerifyChannelID))
		{
			isAllowed = guildData.Value.VerifyChannelID.Equals(channelId);
		}

		string? allowedChannelId = guildData.Value.VerifyChannelID ?? guildData.Value.CommandsChannelID;

		return (isAllowed, allowedChannelId);
	}

	public static async Task<(bool, string?)> IsLeaderboardCommandsAllowedAsync(string guildDiscordId, string channelId)
	{
		Log.WriteVerbose($"Checking whether leaderboard commands are allowed in the channel (guild ID {guildDiscordId}, channel ID {channelId}).");

		ServersQuerySchema.ServersTableData? guildData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildDiscordId);
		if (guildData == null)
		{
			Log.WriteVerbose("Cache data not found or expired. Fetching database data.");

			try
			{
				ServersQuerySchema.ServersTableData tempGuildData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guildDiscordId);
				guildData = tempGuildData;
				CacheManager.Instance.GuildCacheInstance.SetFromDatabaseCache(guildDiscordId, tempGuildData);
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		// check commands channel, then leaderboards
		bool isAllowed = guildData.Value.CommandsChannelID == null || guildData.Value.CommandsChannelID.Equals(channelId);
		if (!string.IsNullOrWhiteSpace(guildData.Value.LeaderboardsChannelID))
		{
			isAllowed = guildData.Value.LeaderboardsChannelID.Equals(channelId);
		}

		string? allowedChannelId = guildData.Value.LeaderboardsChannelID ?? guildData.Value.CommandsChannelID;

		return (isAllowed, allowedChannelId);
	}

	public static bool IsSnipeIDGuild(string? guildDiscordId) => guildDiscordId != null && guildDiscordId.Equals(SNIPEID_DISCORD_ID);

	public static bool IsSnipeIDGuild(SocketGuild? guild) => guild != null && guild.Id.ToString().Equals(SNIPEID_DISCORD_ID);
}
