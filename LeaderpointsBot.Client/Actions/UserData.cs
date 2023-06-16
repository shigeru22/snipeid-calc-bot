// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Client.Exceptions.Actions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;
using RolesTable = LeaderpointsBot.Database.Tables.Roles;

namespace LeaderpointsBot.Client.Actions;

public static class UserData
{
	public static async Task<Structures.Actions.UserData.AssignmentResult> InsertOrUpdateAssignment(DatabaseTransaction transaction, string serverDiscordId, int osuId, string osuUsername, string osuCountryCode, int points)
	{
		Users.UsersTableData currentUser;
		RolesTable.RolesTableData? currentRole;
		RolesTable.RolesTableData targetRole;
		Assignments.AssignmentsTableData? currentServerAssignment;

		DateTime? lastServerUserUpdate;

		// check if server exists
		try
		{
			_ = await Servers.GetServerByDiscordID(transaction, serverDiscordId);
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {serverDiscordId} not found in database.");
			throw new SendMessageException("Server not found.", true);
		}

		// get current user from database
		try
		{
			currentUser = await Users.GetUserByOsuID(transaction, osuId);
		}
		catch (DataNotFoundException)
		{
			Log.WriteVerbose($"User with osu! ID {osuId} not found in database. Skipping data update.");
			throw new SkipUpdateException();
		}

		// get current role from database
		try
		{
			currentRole = await RolesTable.GetServerRoleByOsuID(transaction, serverDiscordId, osuId);
		}
		catch (DataNotFoundException)
		{
			// user found but current role not found in database, continue with null
			currentRole = null;
		}

		// get target role from database
		try
		{
			targetRole = await RolesTable.GetTargetServerRoleByPoints(transaction, serverDiscordId, points);
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Role for {points} points (server ID {serverDiscordId} not found in database. Make sure 0 points role exists.");
			throw new SendMessageException("Role error. Contact server administrator for configuration.", true);
		}

		// get current assignment from database
		try
		{
			currentServerAssignment = await Assignments.GetAssignmentByOsuID(transaction, serverDiscordId, osuId);
		}
		catch (DataNotFoundException)
		{
			// current server assignment not found, continue with null
			currentServerAssignment = null;
		}

		try
		{
			// fetch if assignment in database
			if (currentServerAssignment != null)
			{
				lastServerUserUpdate = await Users.GetServerLastPointUpdate(transaction, serverDiscordId);
			}
			else
			{
				lastServerUserUpdate = null;
			}
		}
		catch (DataNotFoundException)
		{
			// no server user data (will be created), continue with null
			lastServerUserUpdate = null;
		}

		{
			// update user data
			Log.WriteVerbose($"Updating user data from osu!api with ID {osuId}");

			// determine which data to update if any
			string? argOsuUsername = !currentUser.Username.Equals(osuUsername) ? osuUsername : null;
			string? argOsuCountryCode = !currentUser.Country.Equals(osuCountryCode) ? osuCountryCode : null;

			await Users.UpdateUser(transaction, osuId, points, argOsuUsername, argOsuCountryCode);
		}

		Log.WriteDebug($"points = {points}, rolename = {targetRole.RoleName}, minpoints = {targetRole.MinPoints}");

		if (currentServerAssignment.HasValue)
		{
			// update server assignment data
			await Assignments.UpdateAssignmentByAssignmentID(transaction, currentServerAssignment.Value.AssignmentID, targetRole.RoleID);
		}
		else
		{
			// insert server assignment data
			Servers.ServersTableData serverData = await Servers.GetServerByDiscordID(transaction, serverDiscordId);
			await Assignments.InsertAssignment(transaction, serverData.ServerID, currentUser.UserID, targetRole.RoleID);
		}

		Log.WriteVerbose("Returning assignment result message data.");

		return new Structures.Actions.UserData.AssignmentResult()
		{
			OldRole = !currentRole.HasValue ? null : new Structures.Actions.UserData.AssignmentResultRoleData()
			{
				RoleDiscordID = currentRole.Value.DiscordID,
				RoleName = currentRole.Value.RoleName
			},
			NewRole = new Structures.Actions.UserData.AssignmentResultRoleData()
			{
				RoleDiscordID = targetRole.DiscordID,
				RoleName = targetRole.RoleName
			},
			UserDiscordID = currentUser.DiscordID,
			DeltaPoints = currentServerAssignment.HasValue ? points - currentUser.Points : points,
			LastUpdate = lastServerUserUpdate
		};
	}
}
