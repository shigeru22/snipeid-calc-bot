using Npgsql;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database;

public class DatabaseFactory
{
	private static readonly DatabaseFactory instance = new();

	public static DatabaseFactory Instance { get => instance; }

	private NpgsqlDataSource? dataSource = null;

	private DBUsers? dbUsers = null;
	private DBRoles? dbRoles = null;
	private DBServers? dbServers = null;
	private DBAssignments? dbAssignments = null;

	public DBUsers UsersInstance
	{
		get
		{
			if(this.dbUsers == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return this.dbUsers;
		}
	}

	public DBRoles RolesInstance
	{
		get
		{
			if(this.dbRoles == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return this.dbRoles;
		}
	}

	public DBServers ServersInstance
	{
		get
		{
			if(this.dbServers == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return this.dbServers;
		}
	}

	public DBAssignments AssignmentsInstance
	{
		get
		{
			if(this.dbAssignments == null)
			{
				throw new DatabaseInstanceException("Factory has not been configured. Call SetConfig() before invoking.");
			}

			return this.dbAssignments;
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
		if(config.CAFilePath != null && config.CAFilePath != string.Empty)
		{
			connectionString += $";SSL Certificate={ Path.GetFullPath(config.CAFilePath) }";
		}

		dataSource = NpgsqlDataSource.Create(connectionString);

		Log.WriteVerbose("SetConfig", "Database data source created. Initializing per table instance.");

		dbUsers = new DBUsers(dataSource);

		Log.WriteVerbose("SetConfig", "Database table wrapper instances created.");
	}
}
