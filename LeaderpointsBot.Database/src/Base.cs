using Npgsql;

namespace LeaderpointsBot.Database;

public class DBConnectorBase
{
	protected NpgsqlDataSource dataSource;

	public DBConnectorBase(NpgsqlDataSource dataSource)
	{
		this.dataSource = dataSource;
	}
}
