// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Database;

public static class Migration
{
	public static async Task CreateAllTables()
	{
		// in relation order:
		// users -> servers -> roles -> assignments

		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteInfo("Creating users table...");
		await Users.CreateUsersTable(transaction);

		Log.WriteInfo("Creating servers table...");
		await Servers.CreateServersTable(transaction);

		Log.WriteInfo("Creating roles table...");
		await DatabaseFactory.Instance.RolesInstance.CreateRolesTable();

		Log.WriteInfo("Creating assignments table...");
		await DatabaseFactory.Instance.AssignmentsInstance.CreateAssignmentsTable();

		Log.WriteInfo("Database table creation completed.");
	}

	public static async Task MigrateData()
	{
		string? guildDiscordId = null;
		string? currentCountryCode = null;

		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		while (string.IsNullOrWhiteSpace(guildDiscordId))
		{
			guildDiscordId = Input.PromptLine("Current active server's Discord ID: ");
		}

		while (string.IsNullOrWhiteSpace(currentCountryCode))
		{
			currentCountryCode = Input.PromptLine("Current active server's country code: ");
		}

		currentCountryCode = currentCountryCode.ToUpper();

		Log.WriteInfo("(1/6) Creating servers table...");
		await Servers.CreateServersTable(transaction);

		Log.WriteInfo("(2/6) Insert current server data...");
		await Servers.InsertServer(transaction, guildDiscordId);

		Log.WriteInfo("(3/6) Altering users table...");
		await Users.AlterUsersTableV2(transaction, currentCountryCode);

		Log.WriteInfo("(4/6) Migrating points data to users table...");
		await Users.MigratePointsDataV2(transaction);

		Log.WriteInfo("(5/6) Altering assignments table...");
		await DatabaseFactory.Instance.AssignmentsInstance.AlterAssignmentsTableV2();

		Log.WriteInfo("(6/6) Modifying roles table...");
		await DatabaseFactory.Instance.RolesInstance.RenameOldTable();
		await DatabaseFactory.Instance.RolesInstance.CreateRolesTable();
		await DatabaseFactory.Instance.RolesInstance.MigrateRolesDataV2();
	}
}
