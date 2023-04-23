// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database;

public static class Migration
{
	public static async Task CreateAllTables()
	{
		// in relation order:
		// users -> servers -> roles -> assignments

		Log.WriteInfo("Creating users table...");
		await DatabaseFactory.Instance.UsersInstance.CreateUsersTable();

		Log.WriteInfo("Creating servers table...");
		await DatabaseFactory.Instance.ServersInstance.CreateServersTable();

		Log.WriteInfo("Creating roles table...");
		await DatabaseFactory.Instance.RolesInstance.CreateRolesTable();

		Log.WriteInfo("Creating assignments table...");
		await DatabaseFactory.Instance.AssignmentsInstance.CreateAssignmentsTable();

		Log.WriteInfo("Database table creation completed.");
	}
}
