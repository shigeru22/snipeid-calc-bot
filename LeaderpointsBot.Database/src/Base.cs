using Npgsql;

namespace LeaderpointsBot.Database;

public abstract class DBConnectorBase
{
	protected readonly NpgsqlDataSource DataSource;

	protected DBConnectorBase(NpgsqlDataSource dataSource)
	{
		this.DataSource = dataSource;
	}
}
