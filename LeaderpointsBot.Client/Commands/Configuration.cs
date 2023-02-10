// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class Configuration
{
	public static async Task<Embed> GetGuildConfigurationAsync(SocketGuild guild)
	{
		Log.WriteVerbose($"Fetching server in database (guild ID {guild.Id}).");

		ServersQuerySchema.ServersTableData guildData;

		try
		{
			guildData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		SocketRole? currentVerifiedRole = null;
		SocketTextChannel? currentCommandsChannel = null;
		SocketTextChannel? currentLeaderboardsChannel = null;

		try
		{
			if (!string.IsNullOrWhiteSpace(guildData.VerifiedRoleID))
			{
				SocketRole tempRole = guild.GetRole(ulong.Parse(guildData.VerifiedRoleID));
				if (tempRole == null)
				{
					Log.WriteError($"Role with specified ID not found (guild ID {guild.Id}). Sending error message.");
					throw new SendMessageException("Configuration error occurred in our end!", true);
				}
				currentVerifiedRole = tempRole;
			}

			if (!string.IsNullOrWhiteSpace(guildData.CommandsChannelID))
			{
				SocketTextChannel tempChannel = guild.GetTextChannel(ulong.Parse(guildData.CommandsChannelID));
				if (tempChannel == null)
				{
					Log.WriteError($"Channel with specified ID not found (guild ID {guild.Id}). Sending error message.");
					throw new SendMessageException("Configuration error occurred in our end!", true);
				}
				currentCommandsChannel = tempChannel;
			}

			if (!string.IsNullOrWhiteSpace(guildData.LeaderboardsChannelID))
			{
				SocketTextChannel tempChannel = guild.GetTextChannel(ulong.Parse(guildData.LeaderboardsChannelID));
				if (tempChannel == null)
				{
					Log.WriteError($"Channel with specified ID not found (guild ID {guild.Id}). Sending error message.");
					throw new SendMessageException("Configuration error occurred in our end!", true);
				}
				currentLeaderboardsChannel = tempChannel;
			}
		}
		catch (FormatException)
		{
			Log.WriteError("Invalid Discord ID configuration in database. Sending error message.");
			throw new SendMessageException("Configuration error occurred in our end!", true);
		}

		Structures.Embeds.Configuration.ServerConfigurations configData = new Structures.Embeds.Configuration.ServerConfigurations()
		{
			GuildName = guild.Name,
			GuildIconURL = guild.IconUrl,
			CountryCode = guildData.Country,
			VerifiedRoleName = currentVerifiedRole?.Name,
			CommandsChannelName = currentCommandsChannel?.Name,
			LeaderboardsChannelName = currentLeaderboardsChannel?.Name
		};

		return Embeds.Configuration.CreateServerConfigurationEmbed(configData);
	}

	public static async Task<string> SetGuildCountryConfigurationAsync(SocketGuild guild, string? targetCountryCode = null)
	{
		Log.WriteVerbose($"Setting country for guild ID {guild.Id} to {(string.IsNullOrWhiteSpace(targetCountryCode) ? "null" : targetCountryCode)}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerCountry(guild.Id.ToString(), targetCountryCode);

		if (string.IsNullOrWhiteSpace(targetCountryCode))
		{
			return "Server country restriction disabled.";
		}
		else
		{
			return $"Set server country restriction to **{targetCountryCode}**.";
		}
	}

	public static async Task<string> SetGuildVerifiedRoleConfigurationAsync(SocketGuild guild, SocketRole? targetRole = null)
	{
		Log.WriteVerbose($"Setting verified role for guild ID {guild.Id} to {(targetRole == null ? "null" : targetRole.Id.ToString())}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerVerifiedRoleID(guild.Id.ToString(), targetRole?.Id.ToString());

		if (targetRole == null)
		{
			return "Server verified role disabled.";
		}
		else
		{
			return $"Set server verified role to **{targetRole.Name}**.";
		}
	}

	public static async Task<string> SetGuildVerifiedRoleConfigurationAsync(SocketGuild guild, string targetRoleDiscordId)
	{
		Log.WriteVerbose($"Setting verified role for guild ID {guild.Id} to {targetRoleDiscordId}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		// check if role exists in server
		SocketRole? targetRole;

		try
		{
			targetRole = guild.GetRole(ulong.Parse(targetRoleDiscordId));
		}
		catch (FormatException)
		{
			Log.WriteWarning("Invalid role Discord ID. Sending error message.");
			throw new SendMessageException("Invalid ID entered.", true);
		}

		if (targetRole == null)
		{
			Log.WriteWarning("Role not found in guild. Sending error message.");
			throw new SendMessageException("Role with specified ID not found.", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerVerifiedRoleID(guild.Id.ToString(), targetRole.Id.ToString());

		return $"Set server verified role to **{targetRole.Name}**";
	}

	public static async Task<string> SetGuildCommandsChannelConfigurationAsync(SocketGuild guild, SocketGuildChannel? targetChannel = null)
	{
		Log.WriteVerbose($"Setting commands channel restriction for guild ID {guild.Id} to {(targetChannel == null ? "null" : targetChannel.Id.ToString())}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		if (targetChannel != null && targetChannel.GetChannelType() != ChannelType.Text)
		{
			Log.WriteWarning("Target channel is not text channel. Sending error message.");
			throw new SendMessageException("Specified channel is not text channel.", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerCommandsChannelID(guild.Id.ToString(), targetChannel?.Id.ToString());

		if (targetChannel == null)
		{
			return "Server commands channel restriction disabled.";
		}
		else
		{
			return $"Set server commands channel restriction to **{targetChannel.Name}**.";
		}
	}

	public static async Task<string> SetGuildCommandsChannelConfigurationAsync(SocketGuild guild, string targetChannelDiscordId)
	{
		Log.WriteVerbose($"Setting commands channel restriction for guild ID {guild.Id} to {targetChannelDiscordId}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		// check if (text) channel exists in server
		SocketTextChannel targetChannel;

		try
		{
			targetChannel = guild.GetTextChannel(ulong.Parse(targetChannelDiscordId));
		}
		catch (FormatException)
		{
			Log.WriteWarning("Invalid channel Discord ID. Sending error message.");
			throw new SendMessageException("Invalid ID entered.", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerCommandsChannelID(guild.Id.ToString(), targetChannel.Id.ToString());

		return $"Set server commands channel restriction to **{targetChannel.Name}**.";
	}

	public static async Task<string> SetGuildLeaderboardsChannelConfigurationAsync(SocketGuild guild, SocketGuildChannel? targetChannel = null)
	{
		Log.WriteVerbose($"Setting leaderboards channel restriction for guild ID {guild.Id} to {(targetChannel == null ? "null" : targetChannel.Id.ToString())}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		if (targetChannel != null && targetChannel.GetChannelType() != ChannelType.Text)
		{
			Log.WriteWarning("Target channel is not text channel. Sending error message.");
			throw new SendMessageException("Specified channel is not text channel.", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerLeaderboardsChannelID(guild.Id.ToString(), targetChannel?.Id.ToString());

		if (targetChannel == null)
		{
			return "Server leaderboards channel restriction disabled.";
		}
		else
		{
			return $"Set server leaderboards channel restriction to **{targetChannel.Name}**.";
		}
	}

	public static async Task<string> SetGuildLeaderboardsChannelConfigurationAsync(SocketGuild guild, string targetChannelDiscordId)
	{
		Log.WriteVerbose($"Setting leaderboards channel restriction for guild ID {guild.Id} to {targetChannelDiscordId}.");

		try
		{
			// check if server exists
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		// check if (text) channel exists in server
		SocketTextChannel targetChannel;

		try
		{
			targetChannel = guild.GetTextChannel(ulong.Parse(targetChannelDiscordId));
		}
		catch (FormatException)
		{
			Log.WriteWarning("Invalid channel Discord ID. Sending error message.");
			throw new SendMessageException("Invalid ID entered.", true);
		}

		await DatabaseFactory.Instance.ServersInstance.UpdateServerLeaderboardsChannelID(guild.Id.ToString(), targetChannel.Id.ToString());

		return $"Set server leaderboards channel restriction to **{targetChannel.Name}**.";
	}
}
