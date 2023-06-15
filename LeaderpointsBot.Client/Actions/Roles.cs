// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Actions;

public static class Roles
{
	public static async Task SetVerifiedRoleAsync(DatabaseTransaction transaction, SocketGuild guild, SocketUser user)
	{
		try
		{
			Servers.ServersTableData dbGuild;
			try
			{
				Log.WriteVerbose($"Fetching server data from database (server ID {guild.Id}).");
				dbGuild = await Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				Log.WriteError("No server found in database. Sending error message.");
				throw new SendMessageException("Server not found in our end!", true);
			}

			if (string.IsNullOrWhiteSpace(dbGuild.VerifiedRoleID))
			{
				Log.WriteVerbose($"Server verified role not set. Skipping verified role grant.");
				return;
			}

			SocketRole targetRole;
			try
			{
				targetRole = guild.Roles
					.Where(guildRole => guildRole.Id.ToString() == dbGuild.VerifiedRoleID)
					.First();
			}
			catch (InvalidOperationException)
			{
				Log.WriteInfo("Server verified role is set, but not found in server roles list. Sending error message.");
				throw new SendMessageException("Verified role for this server is missing.", true);
			}

			// should be found, else why he/she is in the server?
			SocketGuildUser targetGuildUser = guild.Users
				.Where(guildUser => guildUser.Id == user.Id)
				.First();

			Log.WriteInfo($"Granting server verified role (server ID {guild.Id}, user ID {user.Id}).");

			await targetGuildUser.AddRoleAsync(targetRole);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("An error occurred while checking or granting user verified role.");
		}
	}

	public static async Task SetAssignmentRolesAsync(DatabaseTransaction transaction, SocketGuildUser user, Structures.Actions.UserData.AssignmentResult assignmentResult)
	{
		if (assignmentResult.OldRole.HasValue && assignmentResult.NewRole.RoleDiscordID.Equals(assignmentResult.OldRole.Value.RoleDiscordID))
		{
			Log.WriteVerbose("Role is currently equal. Skipping role assignment.");
			return;
		}

		Users.UsersTableData dbUser;
		try
		{
			dbUser = await Users.GetUserByDiscordID(transaction, user.Id.ToString());
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Failed to grant role.", true);
		}

		if (assignmentResult.OldRole.HasValue)
		{
			Log.WriteVerbose($"Old role found. Removing role from user ({dbUser.DiscordID}).");
			await user.RemoveRoleAsync(ulong.Parse(assignmentResult.OldRole.Value.RoleDiscordID));
		}

		Log.WriteVerbose($"Adding role to user ({dbUser.DiscordID}).");
		await user.AddRoleAsync(ulong.Parse(assignmentResult.NewRole.RoleDiscordID));
	}

	public static async Task SetAssignmentRolesAsync(DatabaseTransaction transaction, SocketGuild guild, int osuId, Structures.Actions.UserData.AssignmentResult assignmentResult)
	{
		if (assignmentResult.OldRole.HasValue && assignmentResult.NewRole.RoleDiscordID.Equals(assignmentResult.OldRole.Value.RoleDiscordID))
		{
			Log.WriteVerbose("Role is currently equal. Skipping role assignment.");
			return;
		}

		Users.UsersTableData dbUser;

		try
		{
			dbUser = await Users.GetUserByOsuID(transaction, osuId);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Failed to grant role.", true);
		}

		SocketGuildUser user = guild.Users.First(guildUser => guildUser.Id.ToString().Equals(dbUser.DiscordID));

		if (assignmentResult.OldRole.HasValue)
		{
			Log.WriteVerbose($"Old role found. Removing role from user ({dbUser.DiscordID}).");
			await user.RemoveRoleAsync(ulong.Parse(assignmentResult.OldRole.Value.RoleDiscordID));
		}

		Log.WriteVerbose($"Adding role to user ({dbUser.DiscordID}).");
		await user.AddRoleAsync(ulong.Parse(assignmentResult.NewRole.RoleDiscordID));
	}
}
