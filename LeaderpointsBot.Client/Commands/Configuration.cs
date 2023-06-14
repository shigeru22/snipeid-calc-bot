// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class Configuration
{
	public static async Task<ReturnMessage> GetGuildConfigurationAsync(SocketGuild guild)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Fetching server in database (guild ID {guild.Id}).");

		ServersQuerySchema.ServersTableData guildData;

		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Embed = Embeds.Configuration.CreateServerConfigurationEmbed(configData, Actions.Channel.IsSnipeIDGuild(guild))
		};
	}

	public static async Task<ReturnMessage> SetGuildCountryConfigurationAsync(SocketGuild guild, string? targetCountryCode = null)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting country for guild ID {guild.Id} to {(string.IsNullOrWhiteSpace(targetCountryCode) ? "null" : targetCountryCode)}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		await Database.Tables.Servers.UpdateServerCountry(transaction, guild.Id.ToString(), targetCountryCode);

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = string.IsNullOrWhiteSpace(targetCountryCode) ? "Server country restriction disabled." : $"Set server country restriction to **{targetCountryCode}**."
		};
	}

	public static async Task<ReturnMessage> SetGuildVerifiedRoleConfigurationAsync(SocketGuild guild, SocketRole? targetRole = null)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting verified role for guild ID {guild.Id} to {(targetRole == null ? "null" : targetRole.Id.ToString())}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		await Database.Tables.Servers.UpdateServerVerifiedRoleID(transaction, guild.Id.ToString(), targetRole?.Id.ToString());

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = targetRole == null ? "Server verified role disabled." : $"Set server verified role to **{targetRole.Name}**."
		};
	}

	public static async Task<ReturnMessage> SetGuildVerifiedRoleConfigurationAsync(SocketGuild guild, string targetRoleDiscordId)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting verified role for guild ID {guild.Id} to {targetRoleDiscordId}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		await Database.Tables.Servers.UpdateServerVerifiedRoleID(transaction, guild.Id.ToString(), targetRole.Id.ToString());

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Set server verified role to **{targetRole.Name}**"
		};
	}

	public static async Task<ReturnMessage> SetGuildCommandsChannelConfigurationAsync(SocketGuild guild, SocketGuildChannel? targetChannel = null)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting commands channel restriction for guild ID {guild.Id} to {(targetChannel == null ? "null" : targetChannel.Id.ToString())}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		await Database.Tables.Servers.UpdateServerCommandsChannelID(transaction, guild.Id.ToString(), targetChannel?.Id.ToString());

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = targetChannel == null ? "Server commands channel restriction disabled." : $"Set server commands channel restriction to **{targetChannel.Name}**."
		};
	}

	public static async Task<ReturnMessage> SetGuildCommandsChannelConfigurationAsync(SocketGuild guild, string targetChannelDiscordId)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting commands channel restriction for guild ID {guild.Id} to {targetChannelDiscordId}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		await Database.Tables.Servers.UpdateServerCommandsChannelID(transaction, guild.Id.ToString(), targetChannel.Id.ToString());

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Set server commands channel restriction to **{targetChannel.Name}**."
		};
	}

	public static async Task<ReturnMessage> SetGuildLeaderboardsChannelConfigurationAsync(SocketGuild guild, SocketGuildChannel? targetChannel = null)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting leaderboards channel restriction for guild ID {guild.Id} to {(targetChannel == null ? "null" : targetChannel.Id.ToString())}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		await Database.Tables.Servers.UpdateServerLeaderboardsChannelID(transaction, guild.Id.ToString(), targetChannel?.Id.ToString());

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = targetChannel == null ? "Server leaderboards channel restriction disabled." : $"Set server leaderboards channel restriction to **{targetChannel.Name}**."
		};
	}

	public static async Task<ReturnMessage> SetGuildLeaderboardsChannelConfigurationAsync(SocketGuild guild, string targetChannelDiscordId)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Setting leaderboards channel restriction for guild ID {guild.Id} to {targetChannelDiscordId}.");

		try
		{
			// check if server exists
			_ = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		await Database.Tables.Servers.UpdateServerLeaderboardsChannelID(transaction, guild.Id.ToString(), targetChannel.Id.ToString());

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Set server leaderboards channel restriction to **{targetChannel.Name}**."
		};
	}

	public static async Task<ReturnMessage> GetGuildRolePointsAsync(SocketGuild guild)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Fetching server in database (guild ID {guild.Id}).");

		ServersQuerySchema.ServersTableData guildData;

		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		Log.WriteVerbose($"Retrieving guild role points list (guild ID {guild.Id}).");
		RolesQuerySchema.RolesTableData[] guildRoles = await Database.Tables.Roles.GetServerRoles(transaction, guild.Id.ToString());

		await transaction.CommitAsync();

		if (guildRoles.Length <= 1) // which should be "no role" role
		{
			return new ReturnMessage()
			{
				Message = "No guild roles set for this server."
			};
		}
		else
		{
			return new ReturnMessage()
			{
				Embed = Embeds.Configuration.CreateGuildRoleConfigurationEmbed(guildRoles, guild.Name, guild.IconUrl, Actions.Channel.IsSnipeIDGuild(guild))
			};
		}
	}

	public static async Task<ReturnMessage> AddGuildRolePointsConfigurationAsync(SocketGuild guild, SocketRole targetRole, int minPoints)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Adding role ({targetRole.Id}, {minPoints} pts.) for guild ID {guild.Id}.");

		// check if server exists
		ServersQuerySchema.ServersTableData guildData;
		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		// check if server role exists in database
		RolesQuerySchema.RolesTableData[] guildRoles = await Database.Tables.Roles.GetServerRoles(transaction, guild.Id.ToString());
		if (guildRoles.Length > 1) // note: 0 points count here and not removable
		{
			if (guildRoles.Where(role => !string.IsNullOrWhiteSpace(role.DiscordID) && role.DiscordID.Equals(guild.Id.ToString())).Count() == 1)
			{
				Log.WriteWarning($"Role ID {targetRole.Id} already exists for guild ID {guild.Id}. Sending message.");
				throw new SendMessageException("Target role already added to server configuration.");
			}

			if (guildRoles.Where(role => role.MinPoints == minPoints).Count() == 1)
			{
				Log.WriteWarning($"Role with {minPoints} points already added for guild ID {guild.Id}. Sending message.");
				throw new SendMessageException("Role with specified points already added to server configuration.");
			}
		}

		await Database.Tables.Roles.InsertRole(transaction, targetRole.Id.ToString(), targetRole.Name, minPoints, guildData.ServerID);

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Added role **{targetRole.Name}** with a minimum of {minPoints} points for this server."
		};
	}

	public static async Task<ReturnMessage> AddGuildRolePointsConfigurationAsync(SocketGuild guild, string targetRoleDiscordId, int minPoints)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Adding role ({targetRoleDiscordId}, {minPoints} pts.) for guild ID {guild.Id}.");

		// check if server exists
		ServersQuerySchema.ServersTableData guildData;
		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		// check if server role exists in database
		RolesQuerySchema.RolesTableData[] guildRoles = await Database.Tables.Roles.GetServerRoles(transaction, guild.Id.ToString());
		if (guildRoles.Length > 1) // note: 0 points count here and not removable
		{
			if (guildRoles.Where(role => !string.IsNullOrWhiteSpace(role.DiscordID) && role.DiscordID.Equals(guild.Id.ToString())).Count() == 1)
			{
				Log.WriteWarning($"Role ID {targetRole.Id} already exists for guild ID {guild.Id}. Sending message.");
				throw new SendMessageException("Target role already added to server configuration.");
			}

			if (guildRoles.Where(role => role.MinPoints == minPoints).Count() == 1)
			{
				Log.WriteWarning($"Role with {minPoints} points already added for guild ID {guild.Id}. Sending message.");
				throw new SendMessageException("Role with specified points already added to server configuration.");
			}
		}

		await Database.Tables.Roles.InsertRole(transaction, targetRole.Id.ToString(), targetRole.Name, minPoints, guildData.ServerID);

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Added role **{targetRole.Name}** with a minimum of {minPoints} points for this server."
		};
	}

	public static async Task<ReturnMessage> RemoveGuildRolePointsConfigurationAsync(SocketGuild guild, SocketRole targetRole)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Removing role ({targetRole.Id}) for guild ID {guild.Id}.");

		// check if server exists
		ServersQuerySchema.ServersTableData guildData;
		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		// check if server role exists in database
		RolesQuerySchema.RolesTableData[] guildRoles = await Database.Tables.Roles.GetServerRoles(transaction, guild.Id.ToString());
		RolesQuerySchema.RolesTableData[] targetGuildRole = guildRoles.Where(
			role => !string.IsNullOrWhiteSpace(role.DiscordID) && role.DiscordID.Equals(targetRole.Id.ToString())
		).ToArray();
		if (!targetGuildRole.Any())
		{
			Log.WriteWarning($"Role ID {targetRole.Id} not found for guild ID {guild.Id}. Sending message.");
			throw new SendMessageException("Target role not found in server configuration.");
		}

		await Database.Tables.Roles.DeleteRoleByRoleID(transaction, targetGuildRole[0].RoleID);

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Removed role **{targetRole.Name}** for this server."
		};
	}

	public static async Task<ReturnMessage> RemoveGuildRolePointsConfigurationAsync(SocketGuild guild, string targetRoleDiscordId)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Removing role ({targetRoleDiscordId}) for guild ID {guild.Id}.");

		// check if server exists
		ServersQuerySchema.ServersTableData guildData;
		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
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

		// check if server role exists in database
		RolesQuerySchema.RolesTableData[] guildRoles = await Database.Tables.Roles.GetServerRoles(transaction, guild.Id.ToString());
		RolesQuerySchema.RolesTableData[] targetGuildRole = guildRoles.Where(
			role => !string.IsNullOrWhiteSpace(role.DiscordID) && role.DiscordID.Equals(targetRole.Id.ToString())
		).ToArray();
		if (!targetGuildRole.Any())
		{
			Log.WriteWarning($"Role ID {targetRole.Id} not found for guild ID {guild.Id}. Sending message.");
			throw new SendMessageException("Target role not found in server configuration.");
		}

		await Database.Tables.Roles.DeleteRoleByRoleID(transaction, targetGuildRole[0].RoleID);

		await transaction.CommitAsync();

		return new ReturnMessage()
		{
			Message = $"Removed role **{targetRole.Name}** for this server."
		};
	}
}
