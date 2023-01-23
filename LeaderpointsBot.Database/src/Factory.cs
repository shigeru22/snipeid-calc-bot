// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;
using Npgsql;

namespace LeaderpointsBot.Database;

public class DatabaseFactory
{
	[SuppressMessage("csharp", "SA1311", Justification = "Private static readonly instance names should be lowercased (styling not yet configurable).")]
	private static readonly DatabaseFactory instance = new DatabaseFactory();

	private NpgsqlDataSource? dataSource;

	private DBUsers? dbUsers;
	private DBRoles? dbRoles;
	private DBServers? dbServers;
	private DBAssignments? dbAssignments;

	private DatabaseFactory()
	{
		Log.WriteVerbose("DatabaseFactory", "DatabaseFactory instance created.");
	}

	public static DatabaseFactory Instance { get => instance; }

	public DBUsers UsersInstance
	{
		get
		{
			if (dbUsers == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return dbUsers;
		}
	}

	public DBRoles RolesInstance
	{
		get
		{
			if (dbRoles == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return dbRoles;
		}
	}

	public DBServers ServersInstance
	{
		get
		{
			if (dbServers == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return dbServers;
		}
	}

	public DBAssignments AssignmentsInstance
	{
		get
		{
			if (dbAssignments == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return dbAssignments;
		}
	}

	public void SetConfig(DatabaseConfig config)
	{
		Log.WriteVerbose("SetConfig", "Setting configuration based on config parameter.");

		dataSource = NpgsqlDataSource.Create(config.ToConnectionString());

		Log.WriteVerbose("SetConfig", "Database data source created. Initializing per table instance.");

		dbUsers = new DBUsers(dataSource);
		dbRoles = new DBRoles(dataSource);
		dbServers = new DBServers(dataSource);
		dbAssignments = new DBAssignments(dataSource);

		Log.WriteVerbose("SetConfig", "Database table wrapper instances created.");
	}
}
