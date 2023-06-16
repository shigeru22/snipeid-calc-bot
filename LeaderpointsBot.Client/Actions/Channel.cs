// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class Channel
{
	public const string SNIPEID_DISCORD_ID = "862267167100502046";

	public enum GuildChannelType
	{
		COMMANDS,
		VERIFY,
		LEADERBOARDS
	}

	public static async Task CheckCommandChannelAsync(DatabaseTransaction transaction, SocketGuildChannel guildChannel, GuildChannelType type)
	{
		Log.WriteVerbose($"Determining whether command is allowed in channel ({guildChannel.Id}).");

		bool isChannelAllowed;
		string? allowedChannelId;

		(isChannelAllowed, allowedChannelId) = type switch
		{
			GuildChannelType.COMMANDS => await IsClientCommandsAllowedAsync(transaction, guildChannel),
			GuildChannelType.VERIFY => await IsVerifyCommandsAllowedAsync(transaction, guildChannel),
			GuildChannelType.LEADERBOARDS => await IsLeaderboardCommandsAllowedAsync(transaction, guildChannel),
			_ => throw new InvalidOperationException("Invalid guild channel type."),
		};

		if (!isChannelAllowed)
		{
			throw new SendMessageException($"This command is usable in <#{allowedChannelId}> channel.");
		}
	}

	public static async Task<(bool, string?)> IsClientCommandsAllowedAsync(DatabaseTransaction transaction, SocketGuildChannel guildChannel)
	{
		string guildDiscordId = guildChannel.Guild.Id.ToString();
		string channelId = guildChannel.Id.ToString();

		Log.WriteVerbose($"Checking whether client commands are allowed in the channel (guild ID {guildDiscordId}, channel ID {channelId}).");

		Servers.ServersTableData? guildData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildDiscordId);
		if (guildData == null)
		{
			Log.WriteVerbose("Cache data not found or expired. Fetching database data.");

			try
			{
				Servers.ServersTableData tempGuildData = await Servers.GetServerByDiscordID(transaction, guildDiscordId);
				guildData = tempGuildData;
				CacheManager.Instance.GuildCacheInstance.SetDatabaseCache(guildDiscordId, tempGuildData);
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		if (guildData.Value.CommandsChannelID != null && !guildData.Value.CommandsChannelID.Equals(channelId))
		{
			return (false, guildData.Value.CommandsChannelID);
		}

		return (true, null);
	}

	public static async Task<(bool, string?)> IsVerifyCommandsAllowedAsync(DatabaseTransaction transaction, SocketGuildChannel guildChannel)
	{
		string guildDiscordId = guildChannel.Guild.Id.ToString();
		string channelId = guildChannel.Id.ToString();

		Log.WriteVerbose($"Checking whether verification commands are allowed in the channel (guild ID {guildDiscordId}, channel ID {channelId}).");

		Servers.ServersTableData? guildData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildDiscordId);
		if (guildData == null)
		{
			Log.WriteVerbose("Cache data not found or expired. Fetching database data.");

			try
			{
				Servers.ServersTableData tempGuildData = await Servers.GetServerByDiscordID(transaction, guildDiscordId);
				guildData = tempGuildData;
				CacheManager.Instance.GuildCacheInstance.SetDatabaseCache(guildDiscordId, tempGuildData);
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		// check verification channel, if null check commands channel
		if (guildData.Value.VerifyChannelID != null || guildData.Value.CommandsChannelID != null)
		{
			if (guildData.Value.VerifyChannelID != null && !guildData.Value.VerifyChannelID.Equals(channelId))
			{
				return (false, guildData.Value.VerifyChannelID);
			}

			if (guildData.Value.CommandsChannelID != null && !guildData.Value.CommandsChannelID.Equals(channelId))
			{
				return (false, guildData.Value.CommandsChannelID);
			}
		}

		return (true, null);
	}

	public static async Task<(bool, string?)> IsLeaderboardCommandsAllowedAsync(DatabaseTransaction transaction, SocketGuildChannel guildChannel)
	{
		string guildDiscordId = guildChannel.Guild.Id.ToString();
		string channelId = guildChannel.Id.ToString();

		Log.WriteVerbose($"Checking whether leaderboard commands are allowed in the channel (guild ID {guildDiscordId}, channel ID {channelId}).");

		Servers.ServersTableData? guildData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildDiscordId);
		if (guildData == null)
		{
			Log.WriteVerbose("Cache data not found or expired. Fetching database data.");

			try
			{
				Servers.ServersTableData tempGuildData = await Servers.GetServerByDiscordID(transaction, guildDiscordId);
				guildData = tempGuildData;
				CacheManager.Instance.GuildCacheInstance.SetDatabaseCache(guildDiscordId, tempGuildData);
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		// check leaderboards channel, if null check commands channel
		if (guildData.Value.LeaderboardsChannelID != null || guildData.Value.CommandsChannelID != null)
		{
			if (guildData.Value.LeaderboardsChannelID != null && !guildData.Value.LeaderboardsChannelID.Equals(channelId))
			{
				return (false, guildData.Value.LeaderboardsChannelID);
			}

			if (guildData.Value.CommandsChannelID != null && !guildData.Value.CommandsChannelID.Equals(channelId))
			{
				return (false, guildData.Value.CommandsChannelID);
			}
		}

		return (true, null);
	}

	public static bool IsSnipeIDGuild(string? guildDiscordId) => guildDiscordId != null && guildDiscordId.Equals(SNIPEID_DISCORD_ID);

	public static bool IsSnipeIDGuild(SocketGuild? guild) => guild != null && guild.Id.ToString().Equals(SNIPEID_DISCORD_ID);
}
