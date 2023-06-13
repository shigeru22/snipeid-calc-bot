// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public static class Servers
{
	public static async Task<ServersQuerySchema.ServersTableData[]> GetServers(DatabaseTransaction transaction)
	{
		const string query = @"
			SELECT
				servers.""serverid"",
				servers.""discordid"",
				servers.""country"",
				servers.""verifychannelid"",
				servers.""verifiedroleid"",
				servers.""commandschannelid"",
				servers.""leaderboardschannelid""
			FROM
				servers
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			return Array.Empty<ServersQuerySchema.ServersTableData>();
		}

		List<ServersQuerySchema.ServersTableData> ret = new List<ServersQuerySchema.ServersTableData>();

		while (await reader.ReadAsync())
		{
			int serverId = reader.GetInt32(0);
			string discordId = reader.GetString(1);
			string? country;
			string? verifyChannelId;
			string? verifiedRoleId;
			string? commandsChannelId;
			string? leaderboardsChannelId;

			try
			{
				country = reader.GetString(2);
			}
			catch (InvalidCastException)
			{
				country = null;
			}

			try
			{
				verifyChannelId = reader.GetString(3);
			}
			catch (InvalidCastException)
			{
				verifyChannelId = null;
			}

			try
			{
				verifiedRoleId = reader.GetString(4);
			}
			catch (InvalidCastException)
			{
				verifiedRoleId = null;
			}

			try
			{
				commandsChannelId = reader.GetString(5);
			}
			catch (InvalidCastException)
			{
				commandsChannelId = null;
			}

			try
			{
				leaderboardsChannelId = reader.GetString(6);
			}
			catch (InvalidCastException)
			{
				leaderboardsChannelId = null;
			}

			ret.Add(new ServersQuerySchema.ServersTableData()
			{
				ServerID = serverId,
				DiscordID = discordId,
				Country = country,
				VerifyChannelID = verifyChannelId,
				VerifiedRoleID = verifiedRoleId,
				CommandsChannelID = commandsChannelId,
				LeaderboardsChannelID = leaderboardsChannelId
			});
		}

		Log.WriteInfo($"servers: Returned {ret.Count} row{(ret.Count != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<ServersQuerySchema.ServersTableData[]> GetServersByCountry(DatabaseTransaction transaction, string countryCode)
	{
		const string query = @"
			SELECT
				servers.""serverid"",
				servers.""discordid"",
				servers.""country"",
				servers.""verifychannelid"",
				servers.""verifiedroleid"",
				servers.""commandschannelid"",
				servers.""leaderboardschannelid""
			FROM
				servers
			WHERE
				servers.""country"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = countryCode }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			return Array.Empty<ServersQuerySchema.ServersTableData>();
		}

		List<ServersQuerySchema.ServersTableData> ret = new List<ServersQuerySchema.ServersTableData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new ServersQuerySchema.ServersTableData()
			{
				ServerID = reader.GetInt32(0),
				DiscordID = reader.GetString(1),
				Country = reader.GetString(2),
				VerifyChannelID = reader.GetString(3),
				VerifiedRoleID = reader.GetString(4),
				CommandsChannelID = reader.GetString(5),
				LeaderboardsChannelID = reader.GetString(6)
			});
		}

		Log.WriteInfo($"servers: Returned {ret.Count} row{(ret.Count != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<ServersQuerySchema.ServersTableData> GetServerByServerID(DatabaseTransaction transaction, int serverId)
	{
		const string query = @"
			SELECT
				servers.""serverid"",
				servers.""discordid"",
				servers.""country"",
				servers.""verifychannelid"",
				servers.""verifiedroleid"",
				servers.""commandschannelid"",
				servers.""leaderboardschannelid""
			FROM
				servers
			WHERE
				servers.""serverid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = serverId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Server with serverId = {serverId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in servers table (serverId = {serverId}).");
			throw new DuplicateRecordException("servers", "serverid"); // D0302
		}

		_ = await reader.ReadAsync();

		ServersQuerySchema.ServersTableData ret = new ServersQuerySchema.ServersTableData()
		{
			ServerID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			Country = reader.GetString(2),
			VerifyChannelID = reader.GetString(3),
			VerifiedRoleID = reader.GetString(4),
			CommandsChannelID = reader.GetString(5),
			LeaderboardsChannelID = reader.GetString(6)
		};

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public static async Task<ServersQuerySchema.ServersTableData> GetServerByDiscordID(DatabaseTransaction transaction, string guildDiscordId)
	{
		const string query = @"
			SELECT
				servers.""serverid"",
				servers.""discordid"",
				servers.""country"",
				servers.""verifychannelid"",
				servers.""verifiedroleid"",
				servers.""commandschannelid"",
				servers.""leaderboardschannelid""
			FROM
				servers
			WHERE
				servers.""discordid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Server with discordId = {guildDiscordId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in servers table (discordId = ${guildDiscordId}).");
			throw new DuplicateRecordException("servers", "discordid"); // D0302
		}

		_ = await reader.ReadAsync();

		ServersQuerySchema.ServersTableData ret;
		{
			int serverId = reader.GetInt32(0);
			string discordId = reader.GetString(1);
			string? country;
			string? verifyChannelId;
			string? verifiedRoleId;
			string? commandsChannelId;
			string? leaderboardsChannelId;

			try
			{
				country = reader.GetString(2);
			}
			catch (InvalidCastException)
			{
				country = null;
			}

			try
			{
				verifyChannelId = reader.GetString(3);
			}
			catch (InvalidCastException)
			{
				verifyChannelId = null;
			}

			try
			{
				verifiedRoleId = reader.GetString(4);
			}
			catch (InvalidCastException)
			{
				verifiedRoleId = null;
			}

			try
			{
				commandsChannelId = reader.GetString(5);
			}
			catch (InvalidCastException)
			{
				commandsChannelId = null;
			}

			try
			{
				leaderboardsChannelId = reader.GetString(6);
			}
			catch (InvalidCastException)
			{
				leaderboardsChannelId = null;
			}

			ret = new ServersQuerySchema.ServersTableData()
			{
				ServerID = serverId,
				DiscordID = discordId,
				Country = country,
				VerifyChannelID = verifyChannelId,
				VerifiedRoleID = verifiedRoleId,
				CommandsChannelID = commandsChannelId,
				LeaderboardsChannelID = leaderboardsChannelId
			};
		}

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public static async Task InsertServer(DatabaseTransaction transaction, string guildDiscordId)
	{
		const string query = @"
			INSERT INTO servers (discordid)
				VALUES ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Insert query execution failed (discordId = {guildDiscordId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("servers: Inserted 1 row.");
	}

	public static async Task InsertServer(DatabaseTransaction transaction, int serverId, string guildDiscordId)
	{
		const string query = @"
			INSERT INTO servers (serverId, discordid)
				VALUES ($1, $2)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = serverId },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Insert query execution failed (serverId = {serverId}, discordId = {guildDiscordId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("servers: Inserted 1 row.");
	}

	public static async Task UpdateServerCountry(DatabaseTransaction transaction, string guildDiscordId, string? countryCode)
	{
		if (countryCode != null && ((countryCode == string.Empty) || (countryCode != string.Empty && countryCode.Length != 2)))
		{
			Log.WriteVerbose("Invalid argument. Throwing argument exception.");
			throw new ArgumentException("countryCode must be a valid 2-country code.");
		}

		string query = $@"
			UPDATE servers
			SET
				country = {(countryCode != null ? "$1" : "NULL")}
			WHERE
				discordid = {(countryCode != null ? "$2" : "$1")}
		";

		NpgsqlCommand command;

		if (countryCode != null)
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = countryCode.ToUpper() },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Update query execution failed (discordId = {guildDiscordId}, country = {countryCode ?? "null"}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("servers: Updated 1 row.");
	}

	public static async Task UpdateServerVerifiedRoleID(DatabaseTransaction transaction, string guildDiscordId, string? roleDiscordId)
	{
		if (roleDiscordId != null && roleDiscordId.Equals(string.Empty))
		{
			Log.WriteVerbose("Invalid argument. Throwing argument exception.");
			throw new ArgumentException("roleId must be null or not empty.");
		}

		string query = $@"
			UPDATE servers
			SET
				verifiedroleid = {(roleDiscordId != null ? "$1" : "NULL")}
			WHERE
				discordid = {(roleDiscordId != null ? "$2" : "$1")}
		";

		NpgsqlCommand command;

		if (roleDiscordId != null)
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = roleDiscordId },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Update query execution failed (discordId = {guildDiscordId}, country = {roleDiscordId ?? "null"}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("servers: Updated 1 row.");
	}

	public static async Task UpdateServerCommandsChannelID(DatabaseTransaction transaction, string guildDiscordId, string? channelDiscordId)
	{
		if (channelDiscordId != null && channelDiscordId.Equals(string.Empty))
		{
			Log.WriteVerbose("Invalid argument. Throwing argument exception.");
			throw new ArgumentException("channelId must be null or not empty.");
		}

		string query = $@"
			UPDATE servers
			SET
				commandschannelid = {(channelDiscordId != null ? "$1" : "NULL")}
			WHERE
				discordid = {(channelDiscordId != null ? "$2" : "$1")}
		";

		NpgsqlCommand command;

		if (channelDiscordId != null)
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = channelDiscordId },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Update query execution failed (discordId = {guildDiscordId}, commandsChannelId = {channelDiscordId ?? "null"}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("servers: Updated 1 row.");
	}

	public static async Task UpdateServerLeaderboardsChannelID(DatabaseTransaction transaction, string guildDiscordId, string? channelDiscordId)
	{
		if (channelDiscordId != null && channelDiscordId.Equals(string.Empty))
		{
			Log.WriteVerbose("Invalid argument. Throwing argument exception.");
			throw new ArgumentException("channelId must be null or not empty.");
		}

		string query = $@"
			UPDATE servers
			SET
				leaderboardschannelid = {(channelDiscordId != null ? "$1" : "NULL")}
			WHERE
				discordid = {(channelDiscordId != null ? "$2" : "$1")}
		";

		NpgsqlCommand command;

		if (channelDiscordId != null)
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = channelDiscordId },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Update query execution failed (discordId = {guildDiscordId}, country = {channelDiscordId ?? "null"}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("servers: Updated 1 row.");
	}

	public static async Task DeleteServerByServerID(DatabaseTransaction transaction, int serverId)
	{
		const string query = $@"
			DELETE FROM servers
			WHERE servers.""serverid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Delete query execution failed (serverId = {serverId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("servers: Deleted 1 row.");
	}

	public static async Task DeleteServerByDiscordID(DatabaseTransaction transaction, string guildDiscordId)
	{
		const string query = $@"
			DELETE FROM servers
			WHERE servers.""discordid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteVerbose($"Delete query execution failed (discordId = {guildDiscordId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("servers: Deleted 1 row.");
	}

	public static async Task<bool> IsCommandsChannel(DatabaseTransaction transaction, string guildDiscordId, string channelDiscordId)
	{
		ServersQuerySchema.ServersTableData guildData = await GetServerByDiscordID(transaction, guildDiscordId);
		return guildData.CommandsChannelID == null || guildData.CommandsChannelID == channelDiscordId;
	}

	public static async Task<bool> IsLeaderboardsChannel(DatabaseTransaction transaction, string guildDiscordId, string channelDiscordId)
	{
		ServersQuerySchema.ServersTableData guildData = await GetServerByDiscordID(transaction, guildDiscordId);
		return guildData.LeaderboardsChannelID == null || guildData.LeaderboardsChannelID == channelDiscordId;
	}

	internal static async Task CreateServersTable(DatabaseTransaction transaction)
	{
		const string query = @"
			CREATE TABLE servers (
				serverid SERIAL PRIMARY KEY,
				discordid VARCHAR(255) NOT NULL,
				country VARCHAR(2),
				verifychannelid VARCHAR(255) DEFAULT NULL,
				verifiedroleid VARCHAR(255) DEFAULT NULL,
				commandschannelid VARCHAR(255) DEFAULT NULL,
				leaderboardschannelid VARCHAR(255) DEFAULT NULL
			)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		_ = await command.ExecuteNonQueryAsync();
	}
}
