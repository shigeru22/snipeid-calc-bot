// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public class Assignments : DBConnectorBase
{
	public Assignments(NpgsqlDataSource dataSource) : base(dataSource)
	{
		Log.WriteVerbose("Servers table class instance created.");
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData[]> GetAssignmentsByServerID(int serverId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			WHERE
				assignments.""serverid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
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
			Log.WriteWarning($"Assignments with assignments.serverId = {serverId} not found. Returning empty array.");
			return Array.Empty<AssignmentsQuerySchema.AssignmentsTableData>();
		}

		List<AssignmentsQuerySchema.AssignmentsTableData> ret = new List<AssignmentsQuerySchema.AssignmentsTableData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new AssignmentsQuerySchema.AssignmentsTableData()
			{
				AssignmentID = reader.GetInt32(0),
				Username = reader.GetString(1),
				RoleName = reader.GetString(2)
			});
		}

		Log.WriteInfo($"assignments: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData[]> GetAssignmentsByServerDiscordID(string guildDiscordId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			JOIN
				servers ON assignments.""serverid"" = servers.""serverid""
			WHERE
				servers.""discordid"" = ($1)
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
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteWarning($"Assignments with servers.discordId = {guildDiscordId} not found. Returning empty array.");
			return Array.Empty<AssignmentsQuerySchema.AssignmentsTableData>();
		}

		List<AssignmentsQuerySchema.AssignmentsTableData> ret = new List<AssignmentsQuerySchema.AssignmentsTableData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new AssignmentsQuerySchema.AssignmentsTableData()
			{
				AssignmentID = reader.GetInt32(0),
				Username = reader.GetString(1),
				RoleName = reader.GetString(2)
			});
		}

		Log.WriteInfo($"assignments: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByAssignmentID(int assignmentId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			WHERE
				assignments.""assignmentid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = assignmentId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Assignment with assignments.assignmentId = {assignmentId} not found.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in assignments table (assignments.assignmentId = {assignmentId}).");
			throw new DuplicateRecordException("assignments", "assignmentid");
		}

		_ = await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new AssignmentsQuerySchema.AssignmentsTableData()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserID(int userId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			WHERE
				assignments.""userid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Assignment with assignments.userId = {userId} not found.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in assignments table (assignments.userId = {userId}).");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		_ = await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new AssignmentsQuerySchema.AssignmentsTableData()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserID(string guildDiscordId, int userId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			JOIN
				servers ON assignments.""serverid"" = servers.""serverid""
			WHERE
				assignments.""userid"" = ($1) AND servers.""discordid"" = ($2)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Assignment with assignments.userId = {userId} and servers.discordId = {guildDiscordId} not found.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in assignments table (assignments.userId = {userId}, servers.discordId = {guildDiscordId}).");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		_ = await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new AssignmentsQuerySchema.AssignmentsTableData()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserDiscordID(string userDiscordId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			WHERE
				users.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Assignment with users.discordId = {userDiscordId} not found.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in assignments table (users.discordId = {userDiscordId}).");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		_ = await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new AssignmentsQuerySchema.AssignmentsTableData()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserDiscordID(string guildDiscordId, string userDiscordId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
			JOIN
				servers ON assignments.""serverid"" = servers.""serverid""
			WHERE
				users.""discordid"" = ($1) AND servers.""discordid"" = ($2)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userDiscordId },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Assignment with users.discordId = {userDiscordId} and servers.discordId = {guildDiscordId} not found.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in assignments table (users.discordId = {userDiscordId}, servers.discordId = {guildDiscordId}).");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		_ = await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new AssignmentsQuerySchema.AssignmentsTableData()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByOsuID(string guildDiscordId, int osuId)
	{
		const string query = @"
			SELECT
				assignments.""assignmentid"",
				users.""username"",
				roles.""rolename""
			FROM
				assignments
			JOIN
				users ON assignments.""userid"" = users.""userid""
			JOIN
				roles ON assignments.""roleid"" = roles.""roleid""
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
			Log.WriteVerbose("servers: Returned 0 rows.");
			Log.WriteError($"Assignment with osuId = {osuId} not found.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo($"servers: Returned {reader.Rows} rows.");
			Log.WriteError($"Duplicated record found in assignments table (users.osuId = {osuId}, servers.discordId = {guildDiscordId}).");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		_ = await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new AssignmentsQuerySchema.AssignmentsTableData()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");

		Log.WriteInfo("servers: Returned 1 row.");
		return ret;
	}

	public async Task InsertAssignment(int serverId, int userId, int roleId)
	{
		const string query = @"
			INSERT INTO assignments (userid, roleid, serverid)
				VALUES ($1, $2, $3)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId },
				new NpgsqlParameter() { Value = roleId },
				new NpgsqlParameter() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Insert query execution failed (userId = {userId}, roleId = {roleId}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("assignments: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task InsertAssignment(int assignmentId, string guildDiscordId, int userId, int roleId)
	{
		const string query = @"
			INSERT INTO assignments (assignmentid, userid, roleid, serverid)
				VALUES ($1, $2, $3, $4)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = assignmentId },
				new NpgsqlParameter() { Value = userId },
				new NpgsqlParameter() { Value = roleId },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Insert query execution failed (assignmentId = {assignmentId}, userId = {userId}, roleId = {roleId}, serverId = {guildDiscordId}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("assignments: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task UpdateAssignmentByAssignmentID(int assignmentId, int roleId)
	{
		const string query = @"
			UPDATE assignments
			SET
				roleid = ($1)
			WHERE
				assignmentid = ($2)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleId },
				new NpgsqlParameter() { Value = assignmentId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Update query execution failed (roleId = {roleId} -> assignmentId = {assignmentId}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("assignments: Updated 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task UpdateAssignmentByUserDatabaseID(int serverId, int userId, int roleId)
	{
		const string query = @"
			UPDATE assignments
			SET
				roleid = ($1)
			WHERE
				userid = ($2) AND serverid = ($3)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = roleId },
				new NpgsqlParameter() { Value = userId },
				new NpgsqlParameter() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Update query execution failed (roleId = {roleId} -> userId = {userId}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("assignments: Updated 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteAssignmentByAssignmentID(int assignmentId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE assignmentid = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = assignmentId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (assignmentId = {assignmentId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("assignments: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteAssignmentByUserID(int userId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE userid = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows <= 0)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (userId = {userId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo($"assignments: Deleted {affectedRows} row{(affectedRows != 1 ? "s" : string.Empty)}.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteAssignmentByUserID(int userId, int serverId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE userid = ($1) AND serverid = ($2)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId },
				new NpgsqlParameter() { Value = serverId },
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (userId = {userId}, serverId = {serverId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("assignments: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	public async Task DeleteAssignmentsByServerID(int serverId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE serverid = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = serverId },
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows <= 0)
		{
			await tempConnection.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Delete query execution failed (serverId = {serverId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo($"assignments: Deleted {affectedRows} row{(affectedRows != 1 ? "s" : string.Empty)}.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("Database connection closed.");
	}

	internal async Task CreateAssignmentsTable()
	{
		const string query = @"
			CREATE TABLE assignments (
				assignmentid SERIAL PRIMARY KEY,
				userid INTEGER NOT NULL,
				serverid INTEGER NOT NULL,
				roleid INTEGER NOT NULL,
				CONSTRAINT fk_user
					FOREIGN KEY(userid) REFERENCES users(userid),
				CONSTRAINT fk_server
					FOREIGN KEY(serverid) REFERENCES servers(serverid),
				CONSTRAINT fk_role
					FOREIGN KEY(roleid) REFERENCES roles(roleid)
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
