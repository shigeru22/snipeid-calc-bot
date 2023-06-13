// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Data;
using LeaderpointsBot.Utils;
using Npgsql;

namespace LeaderpointsBot.Database;

public class DatabaseTransaction
{
	private static int connectionNumber = 0;

	public readonly NpgsqlConnection Connection;
	public readonly NpgsqlTransaction Transaction;

	public readonly int ConnectionID;
	private bool hasFinalized;

	internal DatabaseTransaction(NpgsqlConnection connection)
	{
		int tempConnectionNumber = ++connectionNumber;

		Log.WriteVerbose($"[{tempConnectionNumber}] Transaction instance initialized. Starting transaction.");

		Connection = connection;
		if (Connection.State != ConnectionState.Open)
		{
			Connection.Open();
		}

		Transaction = Connection.BeginTransaction();
		ConnectionID = tempConnectionNumber;
		hasFinalized = false;
	}

	~DatabaseTransaction()
	{
		if (!hasFinalized)
		{
			Log.WriteWarning($"[{ConnectionID}] Transaction not finalized! Rolling back transaction changes.");
			Transaction.Rollback();
			Connection.Close();
		}
	}

	public async Task CommitAsync()
	{
		Log.WriteInfo($"[{ConnectionID}] Committing transaction changes to database.");
		await Transaction.CommitAsync();
		await Connection.CloseAsync();
		hasFinalized = true;
	}

	public async Task RollbackAsync()
	{
		Log.WriteInfo($"[{ConnectionID}] Rolling back transaction changes.");
		await Transaction.RollbackAsync();
		await Connection.CloseAsync();
		hasFinalized = false;
	}
}
