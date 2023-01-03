using Npgsql;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database;

public class DatabaseFactory
{
	private static readonly DatabaseFactory instance = new();

	public static DatabaseFactory Instance { get => instance; }

	private NpgsqlDataSource? dataSource;

	private DBUsers? dbUsers;
	private DBRoles? dbRoles;
	private DBServers? dbServers;
	private DBAssignments? dbAssignments;

	public DBUsers UsersInstance
	{
		get
		{
			if(dbUsers == null)
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
			if(dbRoles == null)
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
			if(dbServers == null)
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
			if(dbAssignments == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return dbAssignments;
		}
	}

	private DatabaseFactory()
	{
		Log.WriteVerbose("DatabaseFactory", "DatabaseFactory instance created.");
	}

	public void SetConfig(DatabaseConfig config)
	{
		Log.WriteVerbose("SetConfig", "Setting configuration based on config parameter.");

		string connectionString = $"Host={ config.HostName };Port={ config.Port };Username={ config.Username };Password={ config.Password };Database={ config.DatabaseName }";
		if(!string.IsNullOrEmpty(config.CAFilePath))
		{
			connectionString += $";SSL Certificate={ Path.GetFullPath(config.CAFilePath) }";
		}

		dataSource = NpgsqlDataSource.Create(connectionString);

		Log.WriteVerbose("SetConfig", "Database data source created. Initializing per table instance.");

		dbUsers = new DBUsers(dataSource);
		dbRoles = new DBRoles(dataSource);
		dbServers = new DBServers(dataSource);
		dbAssignments = new DBAssignments(dataSource);

		Log.WriteVerbose("SetConfig", "Database table wrapper instances created.");
	}
}
