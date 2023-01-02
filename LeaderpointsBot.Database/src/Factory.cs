using Npgsql;

namespace LeaderpointsBot.Database;

public class DatabaseFactory
{
	private static readonly DatabaseFactory instance = new();

	public static DatabaseFactory Instance { get => instance; }

	private NpgsqlDataSource? dataSource = null;

	private DatabaseFactory() { }

	public void SetConfig(DatabaseConfig config)
	{

		string connectionString = $"Host={ config.HostName };Port={ config.Port };Username={ config.Username };Password={ config.Password };Database={ config.DatabaseName }";
		if(config.CAFilePath != null)
		{
			connectionString += $";SSL Certificate={ Path.GetFullPath(config.CAFilePath) }";
		}

		this.dataSource = NpgsqlDataSource.Create(connectionString);
	}
}
