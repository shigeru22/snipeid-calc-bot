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

	private DBUsers? dbUsers;
	private DBRoles? dbRoles;
	private DBServers? dbServers;
	private DBAssignments? dbAssignments;

	public DBUsers UsersInstance => dbUsers ?? throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
	public DBRoles RolesInstance => dbRoles ?? throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
	public DBServers ServersInstance => dbServers ?? throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
	public DBAssignments AssignmentsInstance => dbAssignments ?? throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");

	private DatabaseFactory()
	{
		Log.WriteVerbose("DatabaseFactory", "DatabaseFactory instance created.");
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
