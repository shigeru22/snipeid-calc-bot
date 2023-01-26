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
		Log.WriteVerbose("DBRoles", "Roles table class instance created.");
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
			Log.WriteVerbose("GetRoles", "roles: Returned 0 rows.");
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

		Log.WriteInfo("GetRoles", $"roles: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
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

		Log.WriteVerbose("GetServerRoles", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetServerRoles", "roles: Returned 0 rows.");
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
		Log.WriteVerbose("GetServerRoles", "Database connection closed.");

		Log.WriteInfo("GetServerRoles", $"roles: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
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

		Log.WriteVerbose("GetRoleByRoleID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetRoleByRoleID", "roles: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetRoleByRoleID", $"roles: Returned {reader.Rows} rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("roles", "roleid");
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
		Log.WriteVerbose("GetRoleByRoleID", "Database connection closed.");

		Log.WriteInfo("GetRoleByRoleID", "roles: Returned 1 row.");
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

		Log.WriteVerbose("GetRoleByDiscordID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetRoleByDiscordID", "roles: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetRoleByDiscordID", $"roles: Returned {reader.Rows} rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("roles", "discordid");
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
		Log.WriteVerbose("GetRoleByDiscordID", "Database connection closed.");

		Log.WriteInfo("GetRoleByDiscordID", "roles: Returned 1 row.");
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

		Log.WriteVerbose("GetServerRoleByOsuID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetServerRoleByOsuID", "roles: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetServerRoleByOsuID", $"roles: Returned {reader.Rows} rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("roles", "osuid or discordid");
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
		Log.WriteVerbose("GetServerRoleByOsuID", "Database connection closed.");

		Log.WriteInfo("GetServerRoleByOsuID", "roles: Returned 1 row.");
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

		Log.WriteVerbose("GetTargetServerRoleByPoints", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetTargetServerRoleByPoints", "roles: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetTargetServerRoleByPoints", $"roles: Returned {reader.Rows} rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("roles", "osuid or discordid");
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
		Log.WriteVerbose("GetTargetServerRoleByPoints", "Database connection closed.");

		Log.WriteInfo("GetTargetServerRoleByPoints", "roles: Returned 1 row.");
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

		Log.WriteVerbose("InsertRole", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("InsertRole", "Database connection closed.");
			Log.WriteVerbose("InsertRole", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		Log.WriteInfo("InsertRole", "roles: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("InsertRole", "Database connection closed.");
	}

	public async Task InsertRole(int roleId, string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		const string query = @"
			INSERT INTO roles (roleid, discordid, rolename, minpoints, serverid)
				VALUES ($1, $2, $3, $4, $5)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("InsertRole", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("InsertRole", "Database connection closed.");
			Log.WriteVerbose("InsertRole", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		Log.WriteInfo("InsertRole", "roles: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("InsertRole", "Database connection closed.");
	}

	public async Task UpdateRole(int roleId, string? roleName = null, int? minPoints = null)
	{
		if (string.IsNullOrEmpty(roleName) && minPoints < 0)
		{
			Log.WriteVerbose("UpdateRole", "Invalid argument(s). Throwing argument exception.");
			throw new ArgumentException("Either or both roleName or minPoints must be specified and valid.");
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

		Log.WriteVerbose("UpdateRole", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("UpdateRole", "Database connection closed.");
			Log.WriteVerbose("UpdateRole", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		Log.WriteInfo("UpdateRole", "roles: Updated 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("UpdateRole", "Database connection closed.");
	}

	public async Task DeleteRoleByRoleID(int roleId)
	{
		const string query = @"
			DELETE FROM roles
			WHERE roles.""roleid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteRoleByRoleID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("DeleteRoleByRoleID", "Database connection closed.");
			Log.WriteVerbose("DeleteRoleByRoleID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteRoleByRoleID", "roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteRoleByRoleID", "Database connection closed.");
	}

	public async Task DeleteRoleByDiscordID(int discordId)
	{
		const string query = @"
			DELETE FROM roles
				WHERE roles.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteRoleByDiscordID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("DeleteRoleByDiscordID", "Database connection closed.");
			Log.WriteVerbose("DeleteRoleByDiscordID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteRoleByDiscordID", "roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteRoleByDiscordID", "Database connection closed.");
	}

	public async Task DeleteServerRoles(int guildId)
	{
		const string query = @"
			DELETE FROM roles
				WHERE roles.""serverid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteServerRoles", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("DeleteServerRoles", "Database connection closed.");
			Log.WriteVerbose("DeleteServerRoles", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteServerRoles", "roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteServerRoles", "Database connection closed.");
	}
}
