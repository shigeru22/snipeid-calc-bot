// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public static class Roles
{
	public struct RolesTableData
	{
		public int RoleID { get; set; }
		public string? DiscordID { get; set; }
		public string RoleName { get; set; }
		public int MinPoints { get; set; }
	}

	public struct RoleAssignmentData
	{
		public string? OldRoleID { get; set; }
		public string? OldRoleName { get; set; }
		public string NewRoleID { get; set; }
		public string NewRoleName { get; set; }
	}

	public static async Task<RolesTableData[]> GetRoles(DatabaseTransaction transaction)
	{
		const string query = @"
			SELECT
				roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteWarning("No roles found. Returning empty array.");
			return Array.Empty<RolesTableData>();
		}

		List<RolesTableData> ret = new List<RolesTableData>();

		while (await reader.ReadAsync())
		{
			int roleID = reader.GetInt32(0);
			string? discordId;

			try
			{
				discordId = reader.GetString(1);
			}
			catch (InvalidCastException)
			{
				discordId = null;
			}

			string roleName = reader.GetString(2);
			int minPoints = reader.GetInt32(3);

			ret.Add(new RolesTableData()
			{
				RoleID = roleID,
				DiscordID = discordId,
				RoleName = roleName,
				MinPoints = minPoints
			});
		}

		Log.WriteInfo($"roles: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<RolesTableData[]> GetServerRoles(DatabaseTransaction transaction, string guildDiscordId)
	{
		const string query = @"
			SELECT
				roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
			JOIN
				servers ON servers.""serverid"" = roles.""serverid""
			WHERE
				servers.""discordid"" = ($1)
			ORDER BY
				minPoints DESC
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
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteWarning($"Roles with servers.discordId = {guildDiscordId} not found. Returning empty array.");
			return Array.Empty<RolesTableData>();
		}

		List<RolesTableData> ret = new List<RolesTableData>();

		while (await reader.ReadAsync())
		{
			int roleID = reader.GetInt32(0);
			string? discordId;

			try
			{
				discordId = reader.GetString(1);
			}
			catch (InvalidCastException)
			{
				discordId = null;
			}

			string roleName = reader.GetString(2);
			int minPoints = reader.GetInt32(3);

			ret.Add(new RolesTableData()
			{
				RoleID = roleID,
				DiscordID = discordId,
				RoleName = roleName,
				MinPoints = minPoints
			});
		}

		Log.WriteInfo($"roles: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<RolesTableData> GetRoleByRoleID(DatabaseTransaction transaction, int roleId)
	{
		const string query = @"
			SELECT
				roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
			WHERE
				roles.""roleid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteError($"Roles with roleId = {roleId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteVerbose($"roles: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in roles table (roleId = {roleId}).");
			throw new DuplicateRecordException("roles", "roleid"); // D0302
		}

		_ = await reader.ReadAsync();

		int roleID = reader.GetInt32(0);
		string? discordId;

		try
		{
			discordId = reader.GetString(1);
		}
		catch (InvalidCastException)
		{
			discordId = null;
		}

		string roleName = reader.GetString(2);
		int minPoints = reader.GetInt32(3);

		RolesTableData ret = new RolesTableData()
		{
			RoleID = roleID,
			DiscordID = discordId,
			RoleName = roleName,
			MinPoints = minPoints
		};

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public static async Task<RolesTableData> GetRoleByDiscordID(DatabaseTransaction transaction, string roleDiscordId)
	{
		const string query = @"
			SELECT
				roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
			WHERE
				roles.""discordid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteError($"Roles with discordId = {roleDiscordId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteVerbose($"roles: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in roles table (discordId = {roleDiscordId}).");
			throw new DuplicateRecordException("roles", "discordid"); // D0302
		}

		_ = await reader.ReadAsync();

		int roleID = reader.GetInt32(0);
		string? discordId;

		try
		{
			discordId = reader.GetString(1);
		}
		catch (InvalidCastException)
		{
			discordId = null;
		}

		string roleName = reader.GetString(2);
		int minPoints = reader.GetInt32(3);

		RolesTableData ret = new RolesTableData()
		{
			RoleID = roleID,
			DiscordID = discordId,
			RoleName = roleName,
			MinPoints = minPoints
		};

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public static async Task<RolesTableData> GetServerRoleByOsuID(DatabaseTransaction transaction, string guildDiscordId, int osuId)
	{
		const string query = @"
			SELECT
				roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
			JOIN
				assignments ON assignments.""roleid"" = roles.""roleid""
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				servers ON assignments.""serverid"" = servers.""serverid""
			WHERE
			    users.""osuid"" = ($1) AND servers.""discordid"" = ($2)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = osuId },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteError($"Roles with osuId = {osuId} and servers.discordId = {guildDiscordId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteVerbose($"roles: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in roles table (osuId = {osuId}, servers.discordId = {guildDiscordId}).");
			throw new DuplicateRecordException("roles", "osuid or discordid"); // D0302
		}

		_ = await reader.ReadAsync();

		int roleID = reader.GetInt32(0);
		string? discordId;

		try
		{
			discordId = reader.GetString(1);
		}
		catch (InvalidCastException)
		{
			discordId = null;
		}

		string roleName = reader.GetString(2);
		int minPoints = reader.GetInt32(3);

		RolesTableData ret = new RolesTableData()
		{
			RoleID = roleID,
			DiscordID = discordId,
			RoleName = roleName,
			MinPoints = minPoints
		};

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public static async Task<RolesTableData> GetTargetServerRoleByPoints(DatabaseTransaction transaction, string guildDiscordId, int points)
	{
		const string query = @"
			SELECT
		        roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
			JOIN
				servers ON servers.""serverid"" = roles.""serverid""
			WHERE
				minpoints <= ($1) AND servers.""discordid"" = ($2)
			ORDER BY
				minpoints DESC
			LIMIT 1
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = points },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteError($"Roles with points <= {points} and servers.discordId = {guildDiscordId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			// should not fall here

			Log.WriteVerbose($"roles: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in roles table (points <= {points}, servers.discordId = {guildDiscordId}).");
			throw new DuplicateRecordException("roles", "osuid or discordid"); // D0302
		}

		_ = await reader.ReadAsync();

		int roleID = reader.GetInt32(0);
		string? discordId;

		try
		{
			discordId = reader.GetString(1);
		}
		catch (InvalidCastException)
		{
			discordId = null;
		}

		string roleName = reader.GetString(2);
		int minPoints = reader.GetInt32(3);

		RolesTableData ret = new RolesTableData()
		{
			RoleID = roleID,
			DiscordID = discordId,
			RoleName = roleName,
			MinPoints = minPoints
		};

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public static async Task InsertRole(DatabaseTransaction transaction, string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		const string query = @"
			INSERT INTO roles (discordid, rolename, minpoints, serverid)
				VALUES ($1, $2, $3, $4)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleDiscordId },
				new NpgsqlParameter() { Value = roleName },
				new NpgsqlParameter() { Value = minPoints },
				new NpgsqlParameter() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Insert query execution failed (discordId = {roleDiscordId}, roleName = {roleName}, minPoints = {minPoints}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("roles: Inserted 1 row.");
	}

	public static async Task InsertRole(DatabaseTransaction transaction, int roleId, string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		const string query = @"
			INSERT INTO roles (roleid, discordid, rolename, minpoints, serverid)
				VALUES ($1, $2, $3, $4, $5)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleId },
				new NpgsqlParameter() { Value = roleDiscordId },
				new NpgsqlParameter() { Value = roleName },
				new NpgsqlParameter() { Value = minPoints },
				new NpgsqlParameter() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Insert query execution failed (roleId = {roleId}, discordId = {roleDiscordId}, roleName = {roleName}, minPoints = {minPoints}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("roles: Inserted 1 row.");
	}

	public static async Task UpdateRole(DatabaseTransaction transaction, int roleId, string? roleName = null, int? minPoints = null)
	{
		if (string.IsNullOrEmpty(roleName) && minPoints < 0)
		{
			Log.WriteError("Invalid argument(s) given for this method.");
			throw new ArgumentException("Either or both roleName or minPoints must be specified and valid."); // D0001
		}

		// TODO: test query results
		string query = $@"
			UPDATE roles
			SET
				{(!string.IsNullOrEmpty(roleName) ? $"rolename = ($1){(minPoints >= 0 ? "," : string.Empty)}" : string.Empty)}
				{(minPoints >= 0 ? $"minpoints = ({(!string.IsNullOrEmpty(roleName) ? "$2" : "$1")})" : string.Empty)}
			WHERE
				roleid = ({(!string.IsNullOrEmpty(roleName) && minPoints >= 0 ? "$3" : "$2")})
		";

		NpgsqlCommand command;

		if (!string.IsNullOrEmpty(roleName) && minPoints >= 0)
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = roleName },
					new NpgsqlParameter() { Value = minPoints },
					new NpgsqlParameter() { Value = roleId }
				}
			};
		}
		else
		{
			if (!string.IsNullOrEmpty(roleName))
			{
				command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
				{
					Parameters =
					{
						new NpgsqlParameter() { Value = roleName },
						new NpgsqlParameter() { Value = roleId }
					}
				};
			}
			else if (minPoints >= 0)
			{
				command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
				{
					Parameters =
					{
						new NpgsqlParameter() { Value = minPoints },
						new NpgsqlParameter() { Value = roleId }
					}
				};
			}
			else
			{
				// should not fall here

				command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
				{
					Parameters =
					{
						new NpgsqlParameter() { Value = roleId }
					}
				};
			}
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Update query execution failed (roleId = {roleId}, roleName = {roleName ?? "null"}, minPoints = {minPoints}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("roles: Updated 1 row.");
	}

	public static async Task DeleteRoleByRoleID(DatabaseTransaction transaction, int roleId)
	{
		const string query = @"
			DELETE FROM roles
			WHERE roles.""roleid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Delete query execution failed (roleId = {roleId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("roles: Deleted 1 row.");
	}

	public static async Task DeleteRoleByDiscordID(DatabaseTransaction transaction, int discordId)
	{
		const string query = @"
			DELETE FROM roles
				WHERE roles.""discordid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = discordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Delete query execution failed (discordId = {discordId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("roles: Deleted 1 row.");
	}

	public static async Task DeleteServerRoles(DatabaseTransaction transaction, int guildId)
	{
		const string query = @"
			DELETE FROM roles
				WHERE roles.""serverid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows <= 0)
		{
			Log.WriteError($"Delete query execution failed (serverId = {guildId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo($"roles: Deleted {affectedRows} row{(affectedRows != 1 ? "s" : string.Empty)}.");
	}

	internal static async Task CreateRolesTable(DatabaseTransaction transaction)
	{
		const string query = @"
			CREATE TABLE roles (
				roleid SERIAL PRIMARY KEY,
				discordid VARCHAR(255) NULL,
				serverid INTEGER NOT NULL,
				rolename VARCHAR(255) NOT NULL,
				minpoints INTEGER DEFAULT 0 NOT NULL,
				CONSTRAINT fk_server
					FOREIGN KEY(serverid) REFERENCES servers(serverid)
			)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		_ = await command.ExecuteNonQueryAsync();
	}

	internal static async Task RenameOldTable(DatabaseTransaction transaction)
	{
		const string query = "ALTER TABLE roles RENAME TO roles_old";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		_ = await command.ExecuteNonQueryAsync();
	}

	internal static async Task MigrateRolesDataV2(DatabaseTransaction transaction)
	{
		const string migrateDataQuery = @"
			INSERT INTO roles (discordid, serverid, rolename, minpoints)
				SELECT discordid, 1, rolename, minpoints FROM roles_old ORDER BY roleid
		";

		const string removeRoleConstraintQuery = @"
			ALTER TABLE assignments
			DROP CONSTRAINT fk_role
		";

		const string dropOldTableQuery = "DROP TABLE roles_old";

		const string addNewRoleConstraintQuery = @"
			ALTER TABLE assignments
			ADD CONSTRAINT fk_role FOREIGN KEY (roleid) REFERENCES roles(roleid)
		";

		// since old rolees table still exist, creating new roles table
		// will create a sequence with the prefix "_seq1",
		// this will rename the sequence table
		const string renameSequenceTableQuery = @"
			ALTER SEQUENCE roles_roleid_seq1 RENAME TO roles_roleid_seq;
		";

		await using NpgsqlCommand migrateDataCommand = new NpgsqlCommand(migrateDataQuery, transaction.Connection, transaction.Transaction);
		_ = await migrateDataCommand.ExecuteNonQueryAsync();

		await using NpgsqlCommand removeRoleConstraintCommand = new NpgsqlCommand(removeRoleConstraintQuery, transaction.Connection, transaction.Transaction);
		_ = await removeRoleConstraintCommand.ExecuteNonQueryAsync();

		await using NpgsqlCommand dropOldTableCommand = new NpgsqlCommand(dropOldTableQuery, transaction.Connection, transaction.Transaction);
		_ = await dropOldTableCommand.ExecuteNonQueryAsync();

		await using NpgsqlCommand addNewRoleConstraintCommand = new NpgsqlCommand(addNewRoleConstraintQuery, transaction.Connection, transaction.Transaction);
		_ = await addNewRoleConstraintCommand.ExecuteNonQueryAsync();

		await using NpgsqlCommand renameSequenceTableCommand = new NpgsqlCommand(renameSequenceTableQuery, transaction.Connection, transaction.Transaction);
		_ = await renameSequenceTableCommand.ExecuteNonQueryAsync();
	}
}
