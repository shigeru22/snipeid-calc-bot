// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public static class Users
{
	public static async Task<UsersQuerySchema.UsersTableData[]> GetUsers(DatabaseTransaction transaction)
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

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteInfo("users: Returned 0 rows.");
			return Array.Empty<UsersQuerySchema.UsersTableData>();
		}

		List<UsersQuerySchema.UsersTableData> ret = new List<UsersQuerySchema.UsersTableData>();

		while (await reader.ReadAsync())
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

		Log.WriteInfo($"users: Returned {ret.Count} row{(ret.Count != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<UsersQuerySchema.UsersTableData> GetUserByUserID(DatabaseTransaction transaction, int userId)
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

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			await reader.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"User with userId = {userId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			await reader.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteError($"Duplicated record found in users table (userId = {userId}).");
			throw new DuplicateRecordException("users", "userid"); // D0302
		}

		_ = await reader.ReadAsync();

		UsersQuerySchema.UsersTableData ret = new UsersQuerySchema.UsersTableData()
		{
			UserID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			OsuID = reader.GetInt32(2),
			Username = reader.GetString(3),
			Points = reader.GetInt32(4),
			Country = reader.GetString(5),
			LastUpdate = reader.GetDateTime(6)
		};

		Log.WriteInfo("users: Returned 1 row.");
		return ret;
	}

	public static async Task<UsersQuerySchema.UsersTableData> GetUserByOsuID(DatabaseTransaction transaction, int osuId)
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

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = osuId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteError($"User with osuId = {osuId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteError($"Duplicated record found in users table (osuId = {osuId}).");
			throw new DuplicateRecordException("users", "osuid"); // D0302
		}

		_ = await reader.ReadAsync();

		UsersQuerySchema.UsersTableData ret = new UsersQuerySchema.UsersTableData()
		{
			UserID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			OsuID = reader.GetInt32(2),
			Username = reader.GetString(3),
			Points = reader.GetInt32(4),
			Country = reader.GetString(5),
			LastUpdate = reader.GetDateTime(6)
		};

		Log.WriteInfo("users: Returned 1 row.");
		return ret;
	}

	public static async Task<UsersQuerySchema.UsersTableData> GetUserByDiscordID(DatabaseTransaction transaction, string userDiscordId)
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

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteError($"User with discordId = {userDiscordId} not found.");
			throw new DataNotFoundException(); // D0301
		}

		if (reader.Rows > 1)
		{
			Log.WriteError($"Duplicated record found in users table (discordId = {userDiscordId}).");
			throw new DuplicateRecordException("users", "discordid");
		}

		_ = await reader.ReadAsync();

		UsersQuerySchema.UsersTableData ret = new UsersQuerySchema.UsersTableData()
		{
			UserID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			OsuID = reader.GetInt32(2),
			Username = reader.GetString(3),
			Points = reader.GetInt32(4),
			Country = reader.GetString(5),
			LastUpdate = reader.GetDateTime(6)
		};

		Log.WriteInfo("users: Returned 1 row.");
		return ret;
	}

	public static async Task<UsersQuerySchema.UsersLeaderboardData[]> GetServerPointsLeaderboard(DatabaseTransaction transaction, string guildDiscordId, bool descending = true)
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
				users.""points"" {(descending ? "DESC" : string.Empty)}
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
			await reader.CloseAsync();
			Log.WriteVerbose("Database connection closed.");
			Log.WriteInfo("users: Returned 0 rows.");
			return Array.Empty<UsersQuerySchema.UsersLeaderboardData>();
		}

		List<UsersQuerySchema.UsersLeaderboardData> ret = new List<UsersQuerySchema.UsersLeaderboardData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new UsersQuerySchema.UsersLeaderboardData()
			{
				UserID = reader.GetInt32(0),
				Username = reader.GetString(1),
				Points = reader.GetInt32(2)
			});
		}

		Log.WriteInfo($"users: Returned {ret.Count} row{(ret.Count != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<UsersQuerySchema.UsersLeaderboardData[]> GetServerPointsLeaderboardByCountry(DatabaseTransaction transaction, string guildDiscordId, string countryCode, bool descending = true)
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
				users.""points"" {(descending ? "DESC" : string.Empty)}
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = guildDiscordId },
				new NpgsqlParameter() { Value = countryCode }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if (!reader.HasRows)
		{
			Log.WriteInfo("users: Returned 0 rows.");
			return Array.Empty<UsersQuerySchema.UsersLeaderboardData>();
		}

		List<UsersQuerySchema.UsersLeaderboardData> ret = new List<UsersQuerySchema.UsersLeaderboardData>();

		while (await reader.ReadAsync())
		{
			ret.Add(new UsersQuerySchema.UsersLeaderboardData()
			{
				UserID = reader.GetInt32(0),
				Username = reader.GetString(1),
				Points = reader.GetInt32(2)
			});
		}

		Log.WriteInfo($"users: Returned {ret.Count} row{(ret.Count != 1 ? "s" : string.Empty)}.");
		return ret.ToArray();
	}

	public static async Task<DateTime> GetServerLastPointUpdate(DatabaseTransaction transaction, string guildDiscordId)
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
			Log.WriteError($"No user found in database.");
			throw new DataNotFoundException(); // D0301
		}

		_ = await reader.ReadAsync();

		DateTime ret = reader.GetDateTime(0);

		Log.WriteInfo("users: Returned 1 row.");
		return ret;
	}

	public static async Task InsertUser(DatabaseTransaction transaction, string userDiscordId, int osuId, string username, string countryCode)
	{
		const string query = @"
			INSERT INTO users (discordid, osuid, username, country)
				VALUES ($1, $2, $3, $4)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userDiscordId },
				new NpgsqlParameter() { Value = osuId },
				new NpgsqlParameter() { Value = username },
				new NpgsqlParameter() { Value = countryCode }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Insert query execution failed (discordId = {userDiscordId}, osuId = {osuId}, username = {username}, country = {countryCode}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("users: Inserted 1 row.");
	}

	public static async Task InsertUser(DatabaseTransaction transaction, int userId, string userDiscordId, int osuId, string username, string countryCode)
	{
		const string query = @"
			INSERT INTO users (userid, discordid, osuid, username, country)
				VALUES ($1, $2, $3, $4, $5)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId },
				new NpgsqlParameter() { Value = userDiscordId },
				new NpgsqlParameter() { Value = osuId },
				new NpgsqlParameter() { Value = username },
				new NpgsqlParameter() { Value = countryCode }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Insert query execution failed (userId = {userId}, discordId = {userDiscordId}, osuId = {osuId}, username = {username}, country = {countryCode}).");
			throw new DatabaseInstanceException("Insertion query failed."); // D0201
		}

		Log.WriteInfo("users: Inserted 1 row.");
	}

	public static async Task UpdateUser(DatabaseTransaction transaction, int osuId, int points, string? username = null, string? countryCode = null)
	{
		// only points, username, and country code are updatable
		// uses osuId as user identification

		string query = $@"
			UPDATE users
			SET
				points = ($1),
				lastupdate = ($2){(username != null || countryCode != null ? "," : string.Empty)}
				{(username != null ? "username = $3" : string.Empty)}{(username != null && countryCode != null ? "," : string.Empty)}
				{(countryCode != null ? $"country = {(username != null ? "$4" : "$3")}" : string.Empty)}
			WHERE
				osuid = {(username != null && countryCode != null ? "$5" : (username != null || countryCode != null ? "$4" : "$3"))}
		";

		NpgsqlCommand command;

		if (username != null && countryCode != null)
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = points },
					new NpgsqlParameter() { Value = DateTime.Now },
					new NpgsqlParameter() { Value = username },
					new NpgsqlParameter() { Value = countryCode },
					new NpgsqlParameter() { Value = osuId }
				}
			};
		}
		else if (username != null || countryCode != null)
		{
			if (username != null)
			{
				command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
				{
					Parameters =
					{
						new NpgsqlParameter() { Value = points },
						new NpgsqlParameter() { Value = DateTime.Now },
						new NpgsqlParameter() { Value = username },
						new NpgsqlParameter() { Value = osuId }
					}
				};
			}
			else if (countryCode != null)
			{
				command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
				{
					Parameters =
					{
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

				command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
				{
					Parameters =
					{
						new NpgsqlParameter() { Value = points },
						new NpgsqlParameter() { Value = DateTime.Now },
						new NpgsqlParameter() { Value = osuId }
					}
				};
			}
		}
		else
		{
			command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
			{
				Parameters =
				{
					new NpgsqlParameter() { Value = points },
					new NpgsqlParameter() { Value = DateTime.Now },
					new NpgsqlParameter() { Value = osuId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Update query execution failed (osuId = {osuId}, points = {points}, username = {username ?? "null"}, country = {countryCode ?? "null"}).");
			throw new DatabaseInstanceException("Update query failed."); // D0201
		}

		Log.WriteInfo("users: Updated 1 row.");
	}

	public static async Task DeleteUserByUserID(DatabaseTransaction transaction, int userId)
	{
		const string query = @"
			DELETE FROM users
			WHERE users.""userid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Delete query execution failed (userId = {userId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("users: Deleted 1 row.");
	}

	public static async Task DeleteUserByDiscordID(DatabaseTransaction transaction, string userDiscordId)
	{
		const string query = @"
			DELETE FROM users
			WHERE users.""discordid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = userDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Delete query execution failed (discordId = {userDiscordId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("users: Deleted 1 row.");
	}

	public static async Task DeleteUserByOsuID(DatabaseTransaction transaction, int osuId)
	{
		const string query = @"
			DELETE FROM users
			WHERE users.""osuid"" = ($1)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction)
		{
			Parameters =
			{
				new NpgsqlParameter() { Value = osuId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if (affectedRows != 1)
		{
			Log.WriteError($"Delete query execution failed (osuId = {osuId}).");
			throw new DatabaseInstanceException("Deletion query failed."); // D0201
		}

		Log.WriteInfo("users: Deleted 1 row.");
	}

	internal static async Task CreateUsersTable(DatabaseTransaction transaction)
	{
		const string query = @"
			CREATE TABLE users (
				userid SERIAL PRIMARY KEY,
				discordid VARCHAR(255) NOT NULL,
				osuid INTEGER NOT NULL,
				username VARCHAR(255) NOT NULL,
				country VARCHAR(2) NOT NULL,
				points INTEGER NOT NULL DEFAULT 0,
				lastupdate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
			)
		";

		await using NpgsqlCommand command = new NpgsqlCommand(query, transaction.Connection, transaction.Transaction);
		_ = await command.ExecuteNonQueryAsync();
	}

	internal static async Task AlterUsersTableV2(DatabaseTransaction transaction, string currentCountryCode)
	{
		string modifyTableQuery = $@"
			ALTER TABLE users
			ADD COLUMN country VARCHAR(2) NOT NULL DEFAULT '{currentCountryCode}',
			ADD COLUMN points INTEGER NOT NULL DEFAULT 0,
			ADD COLUMN lastupdate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
		";

		const string removeDefaultQuery = @"
			ALTER TABLE users
			ALTER COLUMN country DROP DEFAULT
		";

		await using NpgsqlCommand modifyTableCommand = new NpgsqlCommand(modifyTableQuery, transaction.Connection, transaction.Transaction);
		_ = await modifyTableCommand.ExecuteNonQueryAsync();

		await using NpgsqlCommand removeDefaultCommand = new NpgsqlCommand(removeDefaultQuery, transaction.Connection, transaction.Transaction);
		_ = await removeDefaultCommand.ExecuteNonQueryAsync();
	}

	internal static async Task MigratePointsDataV2(DatabaseTransaction transaction)
	{
		const string migrateQuery = @"
			DO $$
			DECLARE
				f RECORD;
			BEGIN
				FOR f IN SELECT userid, points, lastupdate FROM assignments ORDER BY userid
				LOOP 
					UPDATE users
					SET points=f.points, lastupdate=f.lastupdate
					WHERE userid=f.userid;
				END LOOP;
			END;
			$$
		";

		await using NpgsqlCommand addColumnCommand = new NpgsqlCommand(migrateQuery, transaction.Connection, transaction.Transaction);
		_ = await addColumnCommand.ExecuteNonQueryAsync();
	}
}
