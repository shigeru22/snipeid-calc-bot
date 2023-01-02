using Npgsql;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database;

public class DatabaseFactory
{
	private static readonly DatabaseFactory instance = new();

	public static DatabaseFactory Instance { get => instance; }

	private NpgsqlDataSource? dataSource = null;

	private DatabaseFactory()
	{
		Log.WriteVerbose("DatabaseFactory", "DatabaseFactory instance created.");
	}

	public void SetConfig(DatabaseConfig config)
	{
		Log.WriteVerbose("SetConfig", "Setting configuration based on config parameter.");

		string connectionString = $"Host={ config.HostName };Port={ config.Port };Username={ config.Username };Password={ config.Password };Database={ config.DatabaseName }";
		if(config.CAFilePath != null)
		{
			connectionString += $";SSL Certificate={ Path.GetFullPath(config.CAFilePath) }";
		}

		dataSource = NpgsqlDataSource.Create(connectionString);

		Log.WriteVerbose("SetConfig", "Database data source created.");
	}
}
