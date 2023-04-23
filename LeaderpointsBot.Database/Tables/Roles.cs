// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public class Roles : DBConnectorBase
{
	public Roles(NpgsqlDataSource dataSource) : base(dataSource)
	{
		Log.WriteVerbose("Roles table class instance created.");
	}

	public async Task<RolesQuerySchema.RolesTableData[]> GetRoles()
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

		await using NpgsqlCommand command = DataSource.CreateCommand(query);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("roles: Returned 0 rows.");
			Log.WriteWarning("No roles found. Returning empty array.");
			return Array.Empty<RolesQuerySchema.RolesTableData>();
		}

		List<RolesQuerySchema.RolesTableData> ret = new List<RolesQuerySchema.RolesTableData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new RolesQuerySchema.RolesTableData()
			{
				RoleID = reader.GetInt32(0),
				DiscordID = reader.GetString(1),
				RoleName = reader.GetString(2),
				MinPoints = reader.GetInt32(3)
			});
		}

		Log.WriteInfo($"roles: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public async Task<RolesQuerySchema.RolesTableData[]> GetServerRoles(string guildDiscordId)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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
			return Array.Empty<RolesQuerySchema.RolesTableData>();
		}

		List<RolesQuerySchema.RolesTableData> ret = new List<RolesQuerySchema.RolesTableData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new RolesQuerySchema.RolesTableData()
			{
				RoleID = reader.GetInt32(0),
				DiscordID = reader.GetString(1),
				RoleName = reader.GetString(2),
				MinPoints = reader.GetInt32(3)
			});
		}

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo($"roles: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public async Task<RolesQuerySchema.RolesTableData> GetRoleByRoleID(int roleId)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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

		RolesQuerySchema.RolesTableData ret = new RolesQuerySchema.RolesTableData()
		{
			RoleID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			RoleName = reader.GetString(2),
			MinPoints = reader.GetInt32(3)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public async Task<RolesQuerySchema.RolesTableData> GetRoleByDiscordID(string roleDiscordId)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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

		RolesQuerySchema.RolesTableData ret = new RolesQuerySchema.RolesTableData()
		{
			RoleID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			RoleName = reader.GetString(2),
			MinPoints = reader.GetInt32(3)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public async Task<RolesQuerySchema.RolesTableData> GetServerRoleByOsuID(string guildDiscordId, int osuId)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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

		RolesQuerySchema.RolesTableData ret = new RolesQuerySchema.RolesTableData()
		{
			RoleID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			RoleName = reader.GetString(2),
			MinPoints = reader.GetInt32(3)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public async Task<RolesQuerySchema.RolesTableData> GetTargetServerRoleByPoints(string guildDiscordId, int points)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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

		RolesQuerySchema.RolesTableData ret = new RolesQuerySchema.RolesTableData()
		{
			RoleID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			RoleName = reader.GetString(2),
			MinPoints = reader.GetInt32(3)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("roles: Returned 1 row.");
		return ret;
	}

	public async Task InsertRole(string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		const string query = @"
			INSERT INTO roles (discordid, rolename, minpoints, serverid)
				VALUES ($1, $2, $3, $4)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Insert query execution failed (discordId = {roleDiscordId}, roleName = {roleName}, minPoints = {minPoints}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("roles: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task InsertRole(int roleId, string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		const string query = @"
			INSERT INTO roles (roleid, discordid, rolename, minpoints, serverid)
				VALUES ($1, $2, $3, $4, $5)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Insert query execution failed (roleId = {roleId}, discordId = {roleDiscordId}, roleName = {roleName}, minPoints = {minPoints}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("roles: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task UpdateRole(int roleId, string? roleName = null, int? minPoints = null)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		NpgsqlCommand command;

		if (!string.IsNullOrEmpty(roleName) && minPoints >= 0)
		{
			command = new NpgsqlCommand(query, tempConnection)
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
				command = new NpgsqlCommand(query, tempConnection)
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
				command = new NpgsqlCommand(query, tempConnection)
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

				command = new NpgsqlCommand(query, tempConnection)
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
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Update query execution failed (roleId = {roleId}, roleName = {roleName ?? "null"}, minPoints = {minPoints}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("roles: Updated 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteRoleByRoleID(int roleId)
	{
		const string query = @"
			DELETE FROM roles
			WHERE roles.""roleid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (roleId = {roleId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteRoleByDiscordID(int discordId)
	{
		const string query = @"
			DELETE FROM roles
				WHERE roles.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = discordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (discordId = {discordId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteServerRoles(int guildId)
	{
		const string query = @"
			DELETE FROM roles
				WHERE roles.""serverid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows <= 0)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (serverId = {guildId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo($"roles: Deleted {affectedRows} row{(affectedRows != 1 ? "s" : string.Empty)}.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	internal async Task CreateRolesTable()
	{
		const string query = @"
			CREATE TABLE roles (
				roleid SERIAL PRIMARY KEY,
				discordid VARCHAR(255) NOT NULL,
				serverid INTEGER NOT NULL,
				rolename VARCHAR(255) NOT NULL,
				minpoints INTEGER DEFAULT 0 NOT NULL,
				CONSTRAINT fk_server
					FOREIGN KEY(serverid) REFERENCES servers(serverid)
			)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection);
		_ = await command.ExecuteNonQueryAsync();

		await tempConnection.CloseAsync();
	}
}
