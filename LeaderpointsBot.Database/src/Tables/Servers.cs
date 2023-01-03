using System.Data;
using Npgsql;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database.Tables;

public class DBServers : DBConnectorBase
{
	public DBServers(NpgsqlDataSource dataSource) : base(dataSource)
	{
		Log.WriteVerbose("DBServers", "Servers table class instance created.");
	}

	public async Task<ServersQuerySchema.ServersTableData[]> GetServers()
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

		await using NpgsqlCommand command = DataSource.CreateCommand(query);
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetServers", "servers: Returned 0 rows.");
			return Array.Empty<ServersQuerySchema.ServersTableData>();
		}

		List<ServersQuerySchema.ServersTableData> ret = new();

		while(await reader.ReadAsync())
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

		await Log.WriteInfo("GetServers", $"servers: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<ServersQuerySchema.ServersTableData[]> GetServersByCountry(string countryCode)
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServersByCountry", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = countryCode }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetServersByCountry", "servers: Returned 0 rows.");
			return Array.Empty<ServersQuerySchema.ServersTableData>();
		}

		List<ServersQuerySchema.ServersTableData> ret = new();

		while(await reader.ReadAsync())
		{
			ret.Add(new ServersQuerySchema.ServersTableData
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

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetServersByCountry", "Database connection closed.");

		await Log.WriteInfo("GetServersByCountry", $"servers: Returned { reader.Rows } row{ (reader.Rows != 1 ? "s" : "") }.");
		return ret.ToArray();
	}

	public async Task<ServersQuerySchema.ServersTableData> GetServerByServerID(int serverId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServerByServerID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = serverId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetServerByServerID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetServerByServerID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("servers", "serverid");
		}

		await reader.ReadAsync();

		ServersQuerySchema.ServersTableData ret = new()
		{
			ServerID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			Country = reader.GetString(2),
			VerifyChannelID = reader.GetString(3),
			VerifiedRoleID = reader.GetString(4),
			CommandsChannelID = reader.GetString(5),
			LeaderboardsChannelID = reader.GetString(6)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetServerByServerID", "Database connection closed.");

		await Log.WriteInfo("GetServerByServerID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task<ServersQuerySchema.ServersTableData> GetServerByDiscordID(string guildDiscordId)
	{
		string query = @"
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

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("GetServerByDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};
		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

		if(!reader.HasRows)
		{
			await Log.WriteVerbose("GetServerByDiscordID", "servers: Returned 0 rows. Throwing not found exception.");
			throw new DataNotFoundException();
		}

		if(reader.Rows > 1)
		{
			await Log.WriteInfo("GetServerByDiscordID", $"servers: Returned { reader.Rows } rows. Throwing duplicate record exception.");
			throw new DuplicateRecordException("servers", "discordid");
		}

		await reader.ReadAsync();

		ServersQuerySchema.ServersTableData ret = new()
		{
			ServerID = reader.GetInt32(0),
			DiscordID = reader.GetString(1),
			Country = reader.GetString(2),
			VerifyChannelID = reader.GetString(3),
			VerifiedRoleID = reader.GetString(4),
			CommandsChannelID = reader.GetString(5),
			LeaderboardsChannelID = reader.GetString(6)
		};

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("GetServerByDiscordID", "Database connection closed.");

		await Log.WriteInfo("GetServerByDiscordID", "servers: Returned 1 row.");
		return ret;
	}

	public async Task InsertServer(string guildDiscordId)
	{
		const string query = @"
			INSERT INTO servers (discordid)
				VALUES ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertServer", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertServer", "Database connection closed.");
			await Log.WriteVerbose("InsertServer", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertServer", "servers: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertRole", "Database connection closed.");
	}

	public async Task InsertServer(int serverId, string guildDiscordId)
	{
		const string query = @"
			INSERT INTO servers (serverId, discordid)
				VALUES ($1), ($2)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("InsertServer", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = serverId },
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertServer", "Database connection closed.");
			await Log.WriteVerbose("InsertServer", "Insertion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Insertion query failed.");
		}

		await Log.WriteInfo("InsertServer", "servers: Inserted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertRole", "Database connection closed.");
	}

	public async Task UpdateServerCountry(string guildDiscordId, string? countryCode)
	{
		if(countryCode != null && ((countryCode == string.Empty) || (countryCode != string.Empty && countryCode.Length != 2)))
		{
			await Log.WriteVerbose("UpdateRole", "Invalid argument. Throwing argument exception.");
			throw new ArgumentException("countryCode must be a valid 2-country code.");
		}

		string query = $@"
			UPDATE servers
			SET
				country = { (countryCode != null ? "$1" : "NULL") }
			WHERE
				discordid = { (countryCode != null ? "$2" : "$1") }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateServerCountry", "Database connection created and opened from data source.");

		NpgsqlCommand command;

		if(countryCode != null)
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = countryCode.ToUpper() },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("InsertServer", "Database connection closed.");
			await Log.WriteVerbose("InsertServer", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("InsertServer", "servers: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("InsertServer", "Database connection closed.");
	}

	public async Task UpdateServerVerifiedRoleID(string guildDiscordId, string? roleId)
	{
		if(!string.IsNullOrEmpty(roleId))
		{
			await Log.WriteVerbose("UpdateServerVerifiedRoleID", "Invalid argument. Throwing argument exception.");
			throw new ArgumentException("roleId must be null or not empty.");
		}

		string query = $@"
			UPDATE servers
			SET
				verifiedroleid = { (roleId != null ? "$1" : "NULL") }
			WHERE
				discordid = { (roleId != null ? "$2" : "$1") }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateServerVerifiedRoleID", "Database connection created and opened from data source.");

		NpgsqlCommand command;

		if(roleId != null)
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = roleId },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateServerVerifiedRoleID", "Database connection closed.");
			await Log.WriteVerbose("UpdateServerVerifiedRoleID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateServerVerifiedRoleID", "servers: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateServerVerifiedRoleID", "Database connection closed.");
	}

	public async Task UpdateServerCommandsChannelID(string guildDiscordId, string? channelDiscordId)
	{
		if(!string.IsNullOrEmpty(channelDiscordId))
		{
			await Log.WriteVerbose("UpdateServerCommandsChannelID", "Invalid argument. Throwing argument exception.");
			throw new ArgumentException("channelId must be null or not empty.");
		}

		string query = $@"
			UPDATE servers
			SET
				commandschannelid = { (channelDiscordId != null ? "$1" : "NULL") }
			WHERE
				discordid = { (channelDiscordId != null ? "$2" : "$1") }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateServerCommandsChannelID", "Database connection created and opened from data source.");

		NpgsqlCommand command;

		if(channelDiscordId != null)
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = channelDiscordId },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateServerCommandsChannelID", "Database connection closed.");
			await Log.WriteVerbose("UpdateServerCommandsChannelID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateServerCommandsChannelID", "servers: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateServerCommandsChannelID", "Database connection closed.");
	}

	public async Task UpdateServerLeaderboardsChannelID(string guildDiscordId, string? channelDiscordId)
	{
		if(!string.IsNullOrEmpty(channelDiscordId))
		{
			await Log.WriteVerbose("UpdateServerLeaderboardsChannelID", "Invalid argument. Throwing argument exception.");
			throw new ArgumentException("channelId must be null or not empty.");
		}

		string query = $@"
			UPDATE servers
			SET
				leaderboardschannelid = { (channelDiscordId != null ? "$1" : "NULL") }
			WHERE
				discordid = { (channelDiscordId != null ? "$2" : "$1") }
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("UpdateServerLeaderboardsChannelID", "Database connection created and opened from data source.");

		NpgsqlCommand command;

		if(channelDiscordId != null)
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = channelDiscordId },
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}
		else
		{
			command = new NpgsqlCommand(query, tempConnection)
			{
				Parameters = {
					new NpgsqlParameter() { Value = guildDiscordId }
				}
			};
		}

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("UpdateServerLeaderboardsChannelID", "Database connection closed.");
			await Log.WriteVerbose("UpdateServerLeaderboardsChannelID", "Update query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Update query failed.");
		}

		await Log.WriteInfo("UpdateServerLeaderboardsChannelID", "servers: Updated 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("UpdateServerLeaderboardsChannelID", "Database connection closed.");
	}

	public async Task DeleteServerByServerID(int serverId)
	{
		const string query = $@"
			DELETE FROM servers
			WHERE servers.""serverid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteServerByServerID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = serverId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteServerByServerID", "Database connection closed.");
			await Log.WriteVerbose("DeleteServerByServerID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteServerByServerID", "servers: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteServerByServerID", "Database connection closed.");
	}

	public async Task DeleteServerByDiscordID(string guildDiscordId)
	{
		const string query = $@"
			DELETE FROM servers
			WHERE servers.""discordid"" = ($1)
		";

		await using NpgsqlConnection tempConnection = DataSource.CreateConnection();
		await tempConnection.OpenAsync();

		await Log.WriteVerbose("DeleteServerByDiscordID", "Database connection created and opened from data source.");

		await using NpgsqlCommand command = new NpgsqlCommand(query, tempConnection)
		{
			Parameters = {
				new NpgsqlParameter() { Value = guildDiscordId }
			}
		};

		int affectedRows = await command.ExecuteNonQueryAsync();

		if(affectedRows != 1)
		{
			await tempConnection.CloseAsync();
			await Log.WriteVerbose("DeleteServerByDiscordID", "Database connection closed.");
			await Log.WriteVerbose("DeleteServerByDiscordID", "Deletion query failed. Throwing instance exception.");
			throw new DatabaseInstanceException("Deletion query failed.");
		}

		await Log.WriteInfo("DeleteServerByDiscordID", "servers: Deleted 1 row.");

		await tempConnection.CloseAsync();
		await Log.WriteVerbose("DeleteServerByDiscordID", "Database connection closed.");
	}

	public async Task<bool> IsCommandsChannel(string guildDiscordId, string channelDiscordId)
	{
		ServersQuerySchema.ServersTableData guildData = await GetServerByDiscordID(guildDiscordId);
		return guildData.CommandsChannelID == null || guildData.CommandsChannelID == channelDiscordId;
	}

	public async Task<bool> IsLeaderboardsChannel(string guildDiscordId, string channelDiscordId)
	{
		ServersQuerySchema.ServersTableData guildData = await GetServerByDiscordID(guildDiscordId);
		return guildData.LeaderboardsChannelID == null || guildData.LeaderboardsChannelID == channelDiscordId;
	}
}
