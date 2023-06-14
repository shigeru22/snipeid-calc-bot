// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Npgsql;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database;

public class DatabaseFactory
{
	private static readonly DatabaseFactory instance = new DatabaseFactory();
	public static DatabaseFactory Instance => instance;

	private NpgsqlDataSource? dataSource;

	private DatabaseFactory()
	{
		Log.WriteVerbose("DatabaseFactory instance created.");
	}

	public void SetConfig(DatabaseConfig config)
	{
		Log.WriteVerbose("Setting configuration based on config parameter.");
		dataSource = NpgsqlDataSource.Create(config.ToConnectionString());
	}

	public DatabaseTransaction InitializeTransaction()
	{
		if (dataSource == null)
		{
			throw new DatabaseInstanceException("Data source has not been configured. Call SetConfig() before invoking.");
		}

		return new DatabaseTransaction(dataSource.CreateConnection());
	}
}
