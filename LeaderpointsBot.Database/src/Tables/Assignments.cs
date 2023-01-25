// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public class DBAssignments : DBConnectorBase
{
	public DBAssignments(NpgsqlDataSource dataSource) : base(dataSource)
	{
		Log.WriteVerbose("DBAssignments", "Servers table class instance created.");
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

		Log.WriteVerbose("GetAssignmentsByServerID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentsByServerID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
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

		Log.WriteInfo("GetAssignmentsByServerID", $"assignments: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
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

		Log.WriteVerbose("GetAssignmentsByServerDiscordID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentsByServerDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
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

		Log.WriteInfo("GetAssignmentsByServerDiscordID", $"assignments: Returned {reader.Rows} row{(reader.Rows != 1 ? "s" : string.Empty)}.");
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

		Log.WriteVerbose("GetAssignmentByAssignmentID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentByAssignmentID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetAssignmentByAssignmentID", $"servers: Returned {reader.Rows} rows. Throwing duplicate record exception.");
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
		Log.WriteVerbose("GetServerByServerID", "Database connection closed.");

		Log.WriteInfo("GetServerByServerID", "servers: Returned 1 row.");
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

		Log.WriteVerbose("GetAssignmentByUserID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentByUserID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetAssignmentByUserID", $"servers: Returned {reader.Rows} rows. Throwing duplicate record exception.");
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
		Log.WriteVerbose("GetAssignmentByUserID", "Database connection closed.");

		Log.WriteInfo("GetAssignmentByUserID", "servers: Returned 1 row.");
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

		Log.WriteVerbose("GetAssignmentByUserID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentByUserID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetAssignmentByUserID", $"servers: Returned {reader.Rows} rows. Throwing duplicate record exception.");
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
		Log.WriteVerbose("GetAssignmentByUserID", "Database connection closed.");

		Log.WriteInfo("GetAssignmentByUserID", "servers: Returned 1 row.");
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

		Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentByUserDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetAssignmentByUserDiscordID", $"servers: Returned {reader.Rows} rows. Throwing duplicate record exception.");
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
		Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection closed.");

		Log.WriteInfo("GetAssignmentByUserDiscordID", "servers: Returned 1 row.");
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

		Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentByUserDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetAssignmentByUserDiscordID", $"servers: Returned {reader.Rows} rows. Throwing duplicate record exception.");
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
		Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection closed.");

		Log.WriteInfo("GetAssignmentByUserDiscordID", "servers: Returned 1 row.");
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

		Log.WriteVerbose("GetAssignmentByOsuID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("GetAssignmentByOsuID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if (reader.Rows > 1)
		{
			Log.WriteInfo("GetAssignmentByOsuID", $"servers: Returned {reader.Rows} rows. Throwing duplicate record exception.");
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
		Log.WriteVerbose("GetAssignmentByOsuID", "Database connection closed.");

		Log.WriteInfo("GetAssignmentByOsuID", "servers: Returned 1 row.");
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

		Log.WriteVerbose("InsertAssignmentByUserID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
			Log.WriteVerbose("InsertAssignmentByUserID", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		Log.WriteInfo("InsertAssignmentByUserID", "assignments: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
	}

	public async Task InsertAssignment(int assignmentId, string guildDiscordId, int userId, int roleId)
	{
		const string query = @"
			INSERT INTO assignments (assignmentid, userid, roleid, serverid)
				VALUES ($1, $2, $3, $4)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("InsertAssignmentByUserID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
			Log.WriteVerbose("InsertAssignmentByUserID", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		Log.WriteInfo("InsertAssignmentByUserID", "assignments: Inserted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
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

		Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Database connection closed.");
			Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		Log.WriteInfo("UpdateAssignmentByAssignmentID", "assignments: Updated 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Database connection closed.");
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

		Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Database connection closed.");
			Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		Log.WriteInfo("UpdateAssignmentByUserDatabaseID", "assignments: Updated 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByAssignmentID(int assignmentId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE assignmentid = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Database connection closed.");
			Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteAssignmentByAssignmentID", "assignments: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByUserDatabaseID(int userId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE userid = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
			Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteAssignmentByUserDatabaseID", $"assignments: Deleted {affectedRows} row{(affectedRows != 1 ? "s" : string.Empty)}.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByUserDatabaseID(int userId, int serverId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE userid = ($1) AND serverid = ($2)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
			Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteAssignmentByUserDatabaseID", "assignments: Deleted 1 row.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByServerID(int serverId)
	{
		const string query = @"
			DELETE FROM assignments
			WHERE serverid = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		Log.WriteVerbose("DeleteAssignmentByServerID", "Database connection created and opened from data source.");

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
			Log.WriteVerbose("DeleteAssignmentByServerID", "Database connection closed.");
			Log.WriteVerbose("DeleteAssignmentByServerDatabaseID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		Log.WriteInfo("DeleteAssignmentByServerID", $"assignments: Deleted {affectedRows} row{(affectedRows != 1 ? "s" : string.Empty)}.");

		await tempConnection.CloseAsync();
		Log.WriteVerbose("DeleteAssignmentByServerDatabaseID", "Database connection closed.");
	}
}
