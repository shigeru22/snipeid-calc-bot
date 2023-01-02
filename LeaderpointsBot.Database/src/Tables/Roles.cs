using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public class DBRoles : DBConnectorBase
{
	public DBRoles(NpgsqlDataSource dataSource) : base(dataSource)
	{
		Log.WriteVerbose("DBRoles", "Roles table class instance created.");
	}

	public async Task<RolesQuerySchema.RolesTableData[]> GetRoles()
	{
		string query = @"
			SELECT
				roles.""roleid"",
				roles.""discordid"",
				roles.""rolename"",
				roles.""minpoints""
			FROM
				roles
		";

		await using NpgsqlCommand command = dataSource.CreateCommand(query);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetRoles", "roles: Returned 0 rows.");
			return new RolesQuerySchema.RolesTableData[0];
		}

		List<RolesQuerySchema.RolesTableData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new()
			{
				RoleID = reader.GetInt32(0),
				DiscordID = reader.GetString(1),
				RoleName = reader.GetString(2),
				MinPoints = reader.GetInt32(3)
			});
		}

		await Log.WriteInfo("GetRoles", $"roles: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<RolesQuerySchema.RolesTableData[]> GetServerRoles(string guildDiscordId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServerRoles", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetServerRoles", "roles: Returned 0 rows.");
			return new RolesQuerySchema.RolesTableData[0];
		}

		List<RolesQuerySchema.RolesTableData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new()
			{
				RoleID = reader.GetInt32(0),
				DiscordID = reader.GetString(1),
				RoleName = reader.GetString(2),
				MinPoints = reader.GetInt32(3)
			});
		}

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetServerRoles", "Database connection closed.");

		await Log.WriteInfo("GetServerRoles", $"roles: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<RolesQuerySchema.RolesTableData> GetRoleByRoleID(int roleId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetRoleByRoleID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetRoleByRoleID", "roles: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetRoleByRoleID", $"roles: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("roles", "roleid");
		}

		await reader.ReadAsync();

		RolesQuerySchema.RolesTableData ret = new()
		{
			RoleID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			RoleName = reader.GetString(2),
			MinPoints = reader.GetInt32(3)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetRoleByRoleID", "Database connection closed.");

		await Log.WriteInfo("GetRoleByRoleID", "roles: Returned 1 row.");
		return ret;
	}

	public async Task<RolesQuerySchema.RolesTableData> GetRoleByDiscordID(string roleDiscordId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetRoleByDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetRoleByDiscordID", "roles: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetRoleByDiscordID", $"roles: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("roles", "discordid");
		}

		await reader.ReadAsync();

		RolesQuerySchema.RolesTableData ret = new()
		{
			RoleID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			RoleName = reader.GetString(2),
			MinPoints = reader.GetInt32(3)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetRoleByDiscordID", "Database connection closed.");

		await Log.WriteInfo("GetRoleByDiscordID", "roles: Returned 1 row.");
		return ret;
	}

	public async Task InsertRole(string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		string query = @"
			INSERT INTO roles (discordid, rolename, minpoints, serverid)
				VALUES ($1), ($2), ($3), ($4)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertRole", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleDiscordId },
				new() { Value = roleName },
				new() { Value = minPoints },
				new() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertRole", "Database connection closed.");
			await Log.WriteVerbose("InsertRole", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertRole", "roles: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertRole", "Database connection closed.");
	}

	public async Task InsertRole(int roleId, string roleDiscordId, string roleName, int minPoints, int serverId)
	{
		string query = @"
			INSERT INTO roles (roleid, discordid, rolename, minpoints, serverid)
				VALUES ($1), ($2), ($3), ($4), ($5)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertRole", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleId },
				new() { Value = roleDiscordId },
				new() { Value = roleName },
				new() { Value = minPoints },
				new() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertRole", "Database connection closed.");
			await Log.WriteVerbose("InsertRole", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertRole", "roles: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertRole", "Database connection closed.");
	}

	public async Task UpdateRole(int roleId, string? roleName = null, int? minPoints = null)
	{
		if((roleName == null || roleName == string.Empty) && (minPoints == null || minPoints < 0))
		{
			await Log.WriteVerbose("UpdateRole", "Invalid argument(s). Throwing argument exception.");
			throw new ArgumentException("Either or both roleName or minPoints must be specified and valid.");
		}

		// TODO: test query results
		string query = $@"
			UPDATE roles
			SET
				{ (roleName != null && roleName != string.Empty ? $"rolename = ($1){ (minPoints != null && minPoints >= 0 ? "," : "") }" : "") }
				{ (minPoints != null && minPoints >= 0 ? $"minpoints = ({ (roleName != null && roleName != string.Empty ? "$2" : "$1") })" : "") }
			WHERE
				roleid = ({ ((roleName != null && roleName != string.Empty) && (minPoints != null && minPoints >= 0) ? "$3" : "$2") })
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateRole", "Database connection created and opened from data source.");

		NpgsqlCommand command;

		if((roleName != null && roleName != string.Empty) && (minPoints != null && minPoints >= 0))
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new() { Value = roleName },
					new() { Value = minPoints },
					new() { Value = roleId }
				}
			};
		}
		else
		{
			if(roleName != null && roleName != string.Empty)
			{
				command = new NpgsqlCommand(query, tempConnection)
				{
					Parameters = {
						new() { Value = roleName },
						new() { Value = roleId }
					}
				};
			}
			else if(minPoints != null && minPoints >= 0)
			{
				command = new NpgsqlCommand(query, tempConnection)
				{
					Parameters = {
						new() { Value = minPoints },
						new() { Value = roleId }
					}
				};
			}
			else
			{
				// should not fall here

				command = new NpgsqlCommand(query, tempConnection)
				{
					Parameters = {
						new() { Value = roleId }
					}
				};
			}
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateRole", "Database connection closed.");
			await Log.WriteVerbose("UpdateRole", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateRole", "roles: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateRole", "Database connection closed.");
	}

	public async Task DeleteRoleByRoleID(int roleId)
	{
		string query = @"
			DELETE FROM roles
			WHERE roles.""roleid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteRoleByRoleID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = roleId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteRoleByRoleID", "Database connection closed.");
			await Log.WriteVerbose("DeleteRoleByRoleID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteRoleByRoleID", "roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteRoleByRoleID", "Database connection closed.");
	}

	public async Task DeleteRoleByDiscordID(int discordId)
	{
		string query = @"
			DELETE FROM roles
				WHERE roles.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteRoleByDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = discordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteRoleByDiscordID", "Database connection closed.");
			await Log.WriteVerbose("DeleteRoleByDiscordID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteRoleByDiscordID", "roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteRoleByDiscordID", "Database connection closed.");
	}

	public async Task DeleteServerRoles(int guildId)
	{
		string query = @"
			DELETE FROM roles
				WHERE roles.""serverid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = dataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteServerRoles", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new() { Value = guildId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteServerRoles", "Database connection closed.");
			await Log.WriteVerbose("DeleteServerRoles", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteServerRoles", "roles: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteServerRoles", "Database connection closed.");
	}
}
