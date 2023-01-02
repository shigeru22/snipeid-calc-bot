using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Database.Exceptions;
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
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentsByServerID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = serverId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentsByServerID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		List<AssignmentsQuerySchema.AssignmentsTableData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new()
			{
				AssignmentID = reader.GetInt32(0),
				Username = reader.GetString(1),
				RoleName = reader.GetString(2)
			});
		}

		await Log.WriteInfo("GetAssignmentsByServerID", $"assignments: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData[]> GetAssignmentsByServerDiscordID(string guildDiscordId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentsByServerDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentsByServerDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		List<AssignmentsQuerySchema.AssignmentsTableData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new()
			{
				AssignmentID = reader.GetInt32(0),
				Username = reader.GetString(1),
				RoleName = reader.GetString(2)
			});
		}

		await Log.WriteInfo("GetAssignmentsByServerDiscordID", $"assignments: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByAssignmentID(int assignmentId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentByAssignmentID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = assignmentId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentByAssignmentID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetAssignmentByAssignmentID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("assignments", "assignmentid");
		}

		await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetServerByServerID", "Database connection closed.");

		await Log.WriteInfo("GetServerByServerID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserID(int userId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentByUserID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentByUserID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetAssignmentByUserID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetAssignmentByUserID", "Database connection closed.");

		await Log.WriteInfo("GetAssignmentByUserID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserID(string guildDiscordId, int userId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentByUserID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userId },
				new() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentByUserID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetAssignmentByUserID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetAssignmentByUserID", "Database connection closed.");

		await Log.WriteInfo("GetAssignmentByUserID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserDiscordID(string userDiscordId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentByUserDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetAssignmentByUserDiscordID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection closed.");

		await Log.WriteInfo("GetAssignmentByUserDiscordID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByUserDiscordID(string guildDiscordId, string userDiscordId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userDiscordId },
				new() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentByUserDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetAssignmentByUserDiscordID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetAssignmentByUserDiscordID", "Database connection closed.");

		await Log.WriteInfo("GetAssignmentByUserDiscordID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task<AssignmentsQuerySchema.AssignmentsTableData> GetAssignmentByOsuID(string guildDiscordId, int osuId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetAssignmentByOsuID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = osuId },
				new() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetAssignmentByOsuID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetAssignmentByOsuID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("assignments", "osuid");
		}

		await reader.ReadAsync();

		AssignmentsQuerySchema.AssignmentsTableData ret = new()
		{
			AssignmentID = reader.GetInt32(0),
			Username = reader.GetString(1),
			RoleName = reader.GetString(2)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetAssignmentByOsuID", "Database connection closed.");

		await Log.WriteInfo("GetAssignmentByOsuID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task InsertAssignment(string guildDiscordId, int userId, int roleId)
	{
		string query = @"
			INSERT INTO assignments (userid, roleid, serverid)
				VALUES ($1), ($2), ($3)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertAssignmentByUserID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userId },
				new() { Value = roleId },
				new() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
			await Log.WriteVerbose("InsertAssignmentByUserID", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertAssignmentByUserID", "assignments: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
	}

	public async Task InsertAssignment(int assignmentId, string guildDiscordId, int userId, int roleId)
	{
		string query = @"
			INSERT INTO assignments (assignmentid, userid, roleid, serverid)
				VALUES ($1), ($2), ($3), ($4)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertAssignmentByUserID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = assignmentId },
				new() { Value = userId },
				new() { Value = roleId },
				new() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
			await Log.WriteVerbose("InsertAssignmentByUserID", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertAssignmentByUserID", "assignments: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertAssignmentByUserID", "Database connection closed.");
	}

	public async Task UpdateAssignmentByAssignmentID(int assignmentId, int roleId)
	{
		string query = @"
			UPDATE assignments
			SET
				roleid = ($1)
			WHERE
				assignmentid = ($2)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleId },
				new() { Value = assignmentId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Database connection closed.");
			await Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateAssignmentByAssignmentID", "assignments: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateAssignmentByAssignmentID", "Database connection closed.");
	}

	public async Task UpdateAssignmentByUserDatabaseID(int serverId, int userId, int roleId)
	{
		string query = @"
			UPDATE assignments
			SET
				roleid = ($1)
			WHERE
				userid = ($2) AND serverid = ($3)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleId },
				new() { Value = userId },
				new() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Database connection closed.");
			await Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateAssignmentByUserDatabaseID", "assignments: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateAssignmentByUserDatabaseID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByAssignmentID(int assignmentId)
	{
		string query = @"
			DELETE FROM assignments
			WHERE assignmentid = ($1)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = assignmentId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Database connection closed.");
			await Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteAssignmentByAssignmentID", "assignments: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteAssignmentByAssignmentID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByUserDatabaseID(int userId)
	{
		string query = @"
			DELETE FROM assignments
			WHERE userid = ($1)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows <= 0)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
			await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteAssignmentByUserDatabaseID", $"assignments: Deleted { affectedRows } row{ (affectedRows != 1 ? "s" : "") }.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByUserDatabaseID(int userId, int serverId)
	{
		string query = @"
			DELETE FROM assignments
			WHERE userid = ($1) AND serverid = ($2)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = userId },
				new() { Value = serverId },
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
			await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteAssignmentByUserDatabaseID", "assignments: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteAssignmentByUserDatabaseID", "Database connection closed.");
	}

	public async Task DeleteAssignmentByServerID(int serverId)
	{
		string query = @"
			DELETE FROM assignments
			WHERE serverid = ($1)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteAssignmentByServerID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = serverId },
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows <= 0)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteAssignmentByServerID", "Database connection closed.");
			await Log.WriteVerbose("DeleteAssignmentByServerDatabaseID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteAssignmentByServerID", $"assignments: Deleted { affectedRows } row{ (affectedRows != 1 ? "s" : "") }.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteAssignmentByServerDatabaseID", "Database connection closed.");
	}
}
