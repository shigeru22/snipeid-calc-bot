// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Client.Exceptions.Actions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class UserData
{
	public static async Task<Structures.Actions.UserData.AssignmentResult> InsertOrUpdateAssignment(string serverDiscordId, int osuId, string osuUsername, string osuCountryCode, int points)
	{
		UsersQuerySchema.UsersTableData currentUser;
		RolesQuerySchema.RolesTableData? currentRole;
		RolesQuerySchema.RolesTableData targetRole;
		AssignmentsQuerySchema.AssignmentsTableData? currentServerAssignment;

		DateTime? lastServerUserUpdate;

		// check if server exists
		try
		{
			_ = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(serverDiscordId);
		}
		catch (DataNotFoundException)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"Server with Discord ID {serverDiscordId} not found in database.");
			throw new SendMessageException("Server not found.", true);
		}
		catch (Exception)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"An unhandled error occurred while querying server.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		// get current user from database
		try
		{
			currentUser = await DatabaseFactory.Instance.UsersInstance.GetUserByOsuID(osuId);
		}
		catch (DataNotFoundException)
		{
			await Log.WriteVerbose("InsertOrUpdateAssignment", $"User with osu! ID {osuId} not found in database. Skipping data update.");
			throw new SkipUpdateException();
		}
		catch (Exception)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"An unhandled error occurred while querying user.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		// get current role from database
		try
		{
			currentRole = await DatabaseFactory.Instance.RolesInstance.GetServerRoleByOsuID(serverDiscordId, osuId);
		}
		catch (DataNotFoundException)
		{
			// user found but current role not found in database, continue with null
			currentRole = null;
		}
		catch (Exception)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"An unhandled error occurred while querying current server role.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		// get target role from database
		try
		{
			targetRole = await DatabaseFactory.Instance.RolesInstance.GetTargetServerRoleByPoints(serverDiscordId, points);
		}
		catch (DataNotFoundException)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"Role for {points} points (server ID {serverDiscordId} not found in database. Make sure 0 points role exists.");
			throw new SendMessageException("Role error. Contact server administrator for configuration.", true);
		}
		catch (Exception)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"An unhandled error occurred while querying target server role.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		// get current assignment from database
		try
		{
			currentServerAssignment = await DatabaseFactory.Instance.AssignmentsInstance.GetAssignmentByOsuID(serverDiscordId, osuId);
		}
		catch (DataNotFoundException)
		{
			// current server assignment not found, continue with null
			currentServerAssignment = null;
		}
		catch (Exception)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"An unhandled error occurred while querying user assignment.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		try
		{
			// fetch if assignment in database
			if (currentServerAssignment != null)
			{
				lastServerUserUpdate = await DatabaseFactory.Instance.UsersInstance.GetServerLastPointUpdate(serverDiscordId);
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
		catch (Exception)
		{
			await Log.WriteError("InsertOrUpdateAssignment", $"An unhandled error occurred while querying user assignment.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		{
			// update user data
			await Log.WriteVerbose("InsertOrUpdateAssignment", $"Updating user data from osu!api with ID {osuId}");

			// determine which data to update if any
			string? argOsuUsername = !currentUser.Username.Equals(osuUsername) ? osuUsername : null;
			string? argOsuCountryCode = !currentUser.Country.Equals(osuCountryCode) ? osuCountryCode : null;

			await DatabaseFactory.Instance.UsersInstance.UpdateUser(osuId, points, argOsuUsername, argOsuCountryCode);
		}

		await Log.WriteDebug("InsertOrUpdateAssignment", $"points = {points}, rolename = {targetRole.RoleName}, minpoints = {targetRole.MinPoints}");

		if (currentServerAssignment.HasValue)
		{
			// update server assignment data
			await DatabaseFactory.Instance.AssignmentsInstance.UpdateAssignmentByAssignmentID(currentServerAssignment.Value.AssignmentID, targetRole.RoleID);
		}
		else
		{
			// insert server assignment data
			try
			{
				ServersQuerySchema.ServersTableData serverData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(serverDiscordId);
				await DatabaseFactory.Instance.AssignmentsInstance.InsertAssignment(serverData.ServerID, currentUser.UserID, targetRole.RoleID);
			}
			catch (Exception e)
			{
				await Log.WriteError("InsertOrUpdateAssignment", $"Error inserting assignment:\n{e}");
				throw new SendMessageException("Error while inserting assignment.");
			}
		}

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
