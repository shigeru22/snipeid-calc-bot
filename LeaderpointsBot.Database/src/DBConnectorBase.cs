// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Npgsql;

namespace LeaderpointsBot.Database;

public abstract class DBConnectorBase
{
	private readonly NpgsqlDataSource dataSource;

	protected DBConnectorBase(NpgsqlDataSource dataSource)
	{
		this.dataSource = dataSource;
	}

	protected NpgsqlDataSource DataSource => dataSource;
}
