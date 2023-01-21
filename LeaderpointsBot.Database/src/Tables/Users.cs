using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public class DBUsers : DBConnectorBase
{
	public DBUsers(NpgsqlDataSource dataSource) : base(dataSource)
	{
		Log.WriteVerbose("DBUsers", "Users table class instance created.");
	}

	public async Task<UsersQuerySchema.UsersTableData[]> GetUsers()
	{
		const string query = @"
			SELECT
				users.""userid"",
				users.""discordid"",
				users.""osuid"",
				users.""username"",
				users.""points"",
				users.""country"",
				users.""lastupdate""
			FROM
				users
		";

		await using NpgsqlCommand command = DataSource.CreateCommand(query);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteInfo("GetUsers", "users: Returned 0 rows.");
			return Array.Empty<UsersQuerySchema.UsersTableData>();
		}

		List<UsersQuerySchema.UsersTableData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new UsersQuerySchema.UsersTableData()
			{
				UserID = reader.GetInt32(0),
				DiscordID = reader.GetString(1),
				OsuID = reader.GetInt32(2),
				Username = reader.GetString(3),
				Points = reader.GetInt32(4),
				Country = reader.GetString(5),
				LastUpdate = reader.GetDateTime(6)
			});
		}

		await Log.WriteInfo("GetUsers", $"users: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<UsersQuerySchema.UsersTableData> GetUserByUserID(int userId)
	{
		const string query = @"
			SELECT
				users.""userid"",
				users.""discordid"",
				users.""osuid"",
				users.""username"",
				users.""points"",
				users.""country"",
				users.""lastupdate""
			FROM
				users
			WHERE
				users.""userid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetUserByUserID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = userId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await reader.CloseAsync();
			await Log.WriteVerbose("GetUserByUserID", "Database connection closed.");
			await Log.WriteInfo("GetUserByUserID", "users: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await reader.CloseAsync();
			await Log.WriteVerbose("GetUserByUserID", "Database connection closed.");
			await Log.WriteInfo("GetUserByUserID", $"users: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("users", "osuid");
		}

		await reader.ReadAsync();

		UsersQuerySchema.UsersTableData ret = new()
		{
			UserID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			OsuID = reader.GetInt32(2),
			Username = reader.GetString(3),
			Points = reader.GetInt32(4),
			Country = reader.GetString(5),
			LastUpdate = reader.GetDateTime(6)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetUserByUserID", "Database connection closed.");

		await Log.WriteInfo("GetUserByUserID", "users: Returned 1 row.");
		return ret;
	}

	public async Task<UsersQuerySchema.UsersTableData> GetUserByOsuID(int osuId)
	{
		const string query = @"
			SELECT
				users.""userid"",
				users.""discordid"",
				users.""osuid"",
				users.""username"",
				users.""points"",
				users.""country"",
				users.""lastupdate""
			FROM
				users
			WHERE
				users.""osuid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetUserByOsuID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = osuId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("GetUserByOsuID", "Database connection closed.");
			await Log.WriteInfo("GetUserByOsuID", "users: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("GetUserByOsuID", "Database connection closed.");
			await Log.WriteInfo("GetUserByOsuID", $"users: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("users", "osuid");
		}

		await reader.ReadAsync();

		UsersQuerySchema.UsersTableData ret = new()
		{
			UserID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			OsuID = reader.GetInt32(2),
			Username = reader.GetString(3),
			Points = reader.GetInt32(4),
			Country = reader.GetString(5),
			LastUpdate = reader.GetDateTime(6)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetUserByOsuID", "Database connection closed.");

		await Log.WriteInfo("GetUserByOsuID", "users: Returned 1 row.");
		return ret;
	}

	public async Task<UsersQuerySchema.UsersTableData> GetUserByDiscordID(string userDiscordId)
	{
		const string query = @"
			SELECT
				users.""userid"",
				users.""discordid"",
				users.""osuid"",
				users.""username"",
				users.""points"",
				users.""country"",
				users.""lastupdate""
			FROM
				users
			WHERE
				users.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetUserByDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = userDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("GetUserByDiscordID", "Database connection closed.");
			await Log.WriteInfo("GetUserByDiscordID", "users: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("GetUserByDiscordID", "Database connection closed.");
			await Log.WriteInfo("GetUserByDiscordID", $"users: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("users", "discordid");
		}

		await reader.ReadAsync();

		UsersQuerySchema.UsersTableData ret = new()
		{
			UserID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			OsuID = reader.GetInt32(2),
			Username = reader.GetString(3),
			Points = reader.GetInt32(4),
			Country = reader.GetString(5),
			LastUpdate = reader.GetDateTime(6)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetUserByDiscordID", "Database connection closed.");

		await Log.WriteInfo("GetUserByDiscordID", "users: Returned 1 row.");
		return ret;
	}

	public async Task<UsersQuerySchema.UsersLeaderboardData[]> GetServerPointsLeaderboard(string guildDiscordId, bool descending = true)
	{
		string query = $@"
			SELECT
				users.""userid"",
				users.""username"",
				users.""points""
			FROM
				users
			JOIN
				assignments ON assignments.""userid"" = users.""userid""
			JOIN
				servers ON servers.""serverid"" = assignments.""serverid""
			WHERE
				servers.""discordid"" = ($1)
			ORDER BY
				users.""points"" { (descending ? "DESC" : "") }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServerPointsLeaderboard", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await reader.CloseAsync();
			await Log.WriteVerbose("GetServerPointsLeaderboard", "Database connection closed.");
			await Log.WriteInfo("GetServerPointsLeaderboard", "users: Returned 0 rows.");
			return Array.Empty<UsersQuerySchema.UsersLeaderboardData>();
		}

		List<UsersQuerySchema.UsersLeaderboardData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new UsersQuerySchema.UsersLeaderboardData()
			{
				UserID = reader.GetInt32(0),
				Username = reader.GetString(1),
				Points = reader.GetInt32(2)
			});
		}

		await reader.CloseAsync();
		await Log.WriteVerbose("GetServerPointsLeaderboard", "Database connection closed.");

		await Log.WriteInfo("GetServerPointsLeaderboard", $"users: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<UsersQuerySchema.UsersLeaderboardData[]> GetServerPointsLeaderboardByCountry(string guildDiscordId, string countryCode, bool descending = true)
	{
		string query = $@"
			SELECT
				users.""userid"",
				users.""username"",
				users.""points""
			FROM
				users
			JOIN
				assignments ON assignments.""userid"" = users.""userid""
			JOIN
				servers ON servers.""serverid"" = assignments.""serverid""
			WHERE
				servers.""discordid"" = ($1) AND users.""country"" = ($2)
			ORDER BY
				users.""points"" { (descending ? "DESC" : "") }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServerPointsLeaderboardByCountry", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = guildDiscordId },
				new NpgsqlParameter() { Value = countryCode }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await reader.CloseAsync();
			await Log.WriteVerbose("GetServerPointsLeaderboardByCountry", "Database connection closed.");
			await Log.WriteInfo("GetServerPointsLeaderboardByCountry", "users: Returned 0 rows.");
			return Array.Empty<UsersQuerySchema.UsersLeaderboardData>();
		}

		List<UsersQuerySchema.UsersLeaderboardData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new UsersQuerySchema.UsersLeaderboardData()
			{
				UserID = reader.GetInt32(0),
				Username = reader.GetString(1),
				Points = reader.GetInt32(2)
			});
		}

		await reader.CloseAsync();
		await Log.WriteVerbose("GetServerPointsLeaderboardByCountry", "Database connection closed.");

		await Log.WriteInfo("GetServerPointsLeaderboardByCountry", $"users: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<DateTime> GetServerLastPointUpdate(string guildDiscordId)
	{
		const string query = @"
			SELECT
				users.""lastupdate""
			FROM
				users
			JOIN
				assignments ON assignments.""userid"" = users.""userid""
			JOIN
				servers ON assignments.""serverid"" = servers.""serverid""
			WHERE
				servers.""discordid"" = ($1)
			ORDER BY
				users.""lastupdate"" DESC
			LIMIT 1
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServerLastPointUpdate", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("GetServerLastPointUpdate", "Database connection closed.");
			await Log.WriteInfo("GetServerLastPointUpdate", "users: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		await reader.ReadAsync();

		DateTime ret = reader.GetDateTime(0);

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetServerLastPointUpdate", "Database connection closed.");

		await Log.WriteInfo("GetServerLastPointUpdate", "users: Returned 1 row.");
		return ret;
	}

	public async Task InsertUser(string userDiscordId, int osuId, string username, string countryCode)
	{
		const string query = @"
			INSERT INTO users (discordid, osuid, username, country)
				VALUES ($1, $2, $3, $4)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertUser", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = userDiscordId },
				new NpgsqlParameter() { Value = osuId },
				new NpgsqlParameter() { Value = username },
				new NpgsqlParameter() { Value = countryCode }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertUser", "Database connection closed.");
			await Log.WriteVerbose("InsertUser", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertUser", "users: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertUser", "Database connection closed.");
	}

	public async Task InsertUser(int userId, string userDiscordId, int osuId, string username, string countryCode)
	{
		const string query = @"
			INSERT INTO users (userid, discordid, osuid, username, country)
				VALUES ($1, $2, $3, $4, $5)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertUser", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = userId},
				new NpgsqlParameter() { Value = userDiscordId },
				new NpgsqlParameter() { Value = osuId },
				new NpgsqlParameter() { Value = username },
				new NpgsqlParameter() { Value = countryCode }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertUser", "Database connection closed.");
			await Log.WriteVerbose("InsertUser", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertUser", "users: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertUser", "Database connection closed.");
	}

	public async Task UpdateUser(int osuId, int points, string? username = null, string? countryCode = null)
	{
		// only points, username, and country code are updatable
		// uses osuId as user identification

		string query = $@"
			UPDATE users
			SET
				points = ($1),
				lastupdate = ($2){ (username != null || countryCode != null ? "," : "") }
				{ (username != null ? "username = $3" : "") }{ (username != null && countryCode != null ? "," : "") }
				{ (countryCode != null ? $"country = { (username != null ? "$4" : "$3") }" : "") }
			WHERE
				osuid = { (username != null && countryCode != null ? "$5" : (username != null || countryCode != null ? "$4" : "$3")) }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertUser", "Database connection created and opened from data source.");

		NpgsqlCommand command;

		if(username != null && countryCode != null)
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = points },
					new NpgsqlParameter() { Value = DateTime.Now },
					new NpgsqlParameter() { Value = username },
					new NpgsqlParameter() { Value = countryCode },
					new NpgsqlParameter() { Value = osuId }
				}
			};
		}
		else if(username != null || countryCode != null)
		{
			if(username != null)
			{
				command = new NpgsqlCommand(query, tempConnection)
				{
					Parameters = {
						new NpgsqlParameter() { Value = points },
						new NpgsqlParameter() { Value = DateTime.Now },
						new NpgsqlParameter() { Value = username },
						new NpgsqlParameter() { Value = osuId }
					}
				};
			}
			else if(countryCode != null)
			{
				command = new NpgsqlCommand(query, tempConnection)
				{
					Parameters = {
						new NpgsqlParameter() { Value = points },
						new NpgsqlParameter() { Value = DateTime.Now },
						new NpgsqlParameter() { Value = countryCode },
						new NpgsqlParameter() { Value = osuId }
					}
				};
			}
			else
			{
				// basically the same as the last else for type checking purpose

				command = new NpgsqlCommand(query, tempConnection)
				{
					Parameters = {
						new NpgsqlParameter() { Value = points },
						new NpgsqlParameter() { Value = DateTime.Now },
						new NpgsqlParameter() { Value = osuId }
					}
				};
			}
		}
		else
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = points },
					new NpgsqlParameter() { Value = DateTime.Now },
					new NpgsqlParameter() { Value = osuId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateUser", "Database connection closed.");
			await Log.WriteVerbose("UpdateUser", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateUser", "users: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateUser", "Database connection closed.");
	}

	public async Task DeleteUserByUserID(int userId)
	{
		const string query = @"
			DELETE FROM users
			WHERE users.""userid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteUserByUserID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = userId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteUserByUserID", "Database connection closed.");
			await Log.WriteVerbose("DeleteUserByUserID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteUserByUserID", "users: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteUserByUserID", "Database connection closed.");
	}

	public async Task DeleteUserByDiscordID(string userDiscordId)
	{
		const string query = @"
			DELETE FROM users
			WHERE users.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteUserByDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = userDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteUserByDiscordID", "Database connection closed.");
			await Log.WriteVerbose("DeleteUserByDiscordID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteUserByDiscordID", "users: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteUserByDiscordID", "Database connection closed.");
	}

	public async Task DeleteUserByOsuID(int osuId)
	{
		const string query = @"
			DELETE FROM users
			WHERE users.""osuid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteUserByOsuID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = osuId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteUserByOsuID", "Database connection closed.");
			await Log.WriteVerbose("DeleteUserByOsuID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteUserByOsuID", "users: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteUserByOsuID", "Database connection closed.");
	}
}
