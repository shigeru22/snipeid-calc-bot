// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class Roles
{
	public static async Task SetVerifiedRoleAsync(SocketGuild guild, SocketUser user)
	{
		try
		{
			ServersQuerySchema.ServersTableData dbGuild;
			try
			{
				Log.WriteVerbose(nameof(SetVerifiedRoleAsync), $"Fetching server data from database (server ID {guild.Id}).");
				dbGuild = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				Log.WriteError(nameof(SetVerifiedRoleAsync), "No server found in database. Sending error message.");
				throw new SendMessageException("Server not found in our end!", true);
			}

			if (string.IsNullOrWhiteSpace(dbGuild.VerifiedRoleID))
			{
				Log.WriteVerbose(nameof(SetVerifiedRoleAsync), $"Server verified role not set. Skipping verified role grant.");
				return;
			}

			SocketRole targetRole;
			try
			{
				targetRole = guild.Roles
					.Where(guildRole => guildRole.Id.ToString() == dbGuild.VerifiedRoleID)
					.First();
			}
			catch(InvalidOperationException)
			{
				Log.WriteInfo(nameof(SetVerifiedRoleAsync), "Server verified role is set, but not found in server roles list. Sending error message.");
				throw new SendMessageException("Verified role for this server is missing.", true);
			}

			// should be found, else why he/she is in the server?
			SocketGuildUser targetGuildUser = guild.Users
				.Where(guildUser => guildUser.Id == user.Id)
				.First();

			Log.WriteError(nameof(SetVerifiedRoleAsync), $"Granting server verified role (server ID {guild.Id}, user ID {user.Id}).");

			await targetGuildUser.AddRoleAsync(targetRole);
		}
		catch (Exception e)
		{
			Log.WriteVerbose(nameof(SetVerifiedRoleAsync), $"An unhandled exception occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			throw new SendMessageException("An error occurred while checking or granting user verified role.");
		}
	}

	public static async Task SetAssignmentRolesAsync(SocketGuild guild, string userDiscordId, Structures.Actions.UserData.AssignmentResult assignmentResult)
	{
		if (assignmentResult.OldRole.HasValue && assignmentResult.NewRole.RoleDiscordID.Equals(assignmentResult.OldRole.Value.RoleDiscordID))
		{
			Log.WriteVerbose("SetAssignmentRolesAsync", "Role is currently equal. Skipping role assignment.");
			return;
		}

		SocketGuildUser user = guild.Users.First(guildUser => guildUser.Id.ToString().Equals(userDiscordId));

		if (assignmentResult.OldRole.HasValue)
		{
			Log.WriteVerbose("SetAssignmentRolesAsync", $"Old role found. Removing role from user ({userDiscordId}).");
			await user.RemoveRoleAsync(ulong.Parse(assignmentResult.OldRole.Value.RoleDiscordID));
		}

		Log.WriteVerbose("SetAssignmentRolesAsync", $"Adding role to user ({userDiscordId}).");
		await user.AddRoleAsync(ulong.Parse(assignmentResult.NewRole.RoleDiscordID));
	}

	public static async Task SetAssignmentRolesAsync(SocketGuild guild, int osuId, Structures.Actions.UserData.AssignmentResult assignmentResult)
	{
		if (assignmentResult.OldRole.HasValue && assignmentResult.NewRole.RoleDiscordID.Equals(assignmentResult.OldRole.Value.RoleDiscordID))
		{
			Log.WriteVerbose("SetAssignmentRolesAsync", "Role is currently equal. Skipping role assignment.");
			return;
		}

		UsersQuerySchema.UsersTableData dbUser;

		try
		{
			dbUser = await DatabaseFactory.Instance.UsersInstance.GetUserByOsuID(osuId);
		}
		catch (Exception e)
		{
			Log.WriteVerbose("SetAssignmentRolesAsync", $"Unhandled exception occurred while querying user in database.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			throw new SendMessageException("An error occurred while querying user.");
		}

		SocketGuildUser user = guild.Users.First(guildUser => guildUser.Id.ToString().Equals(dbUser.DiscordID));

		if (assignmentResult.OldRole.HasValue)
		{
			Log.WriteVerbose("SetAssignmentRolesAsync", $"Old role found. Removing role from user ({dbUser.DiscordID}).");
			await user.RemoveRoleAsync(ulong.Parse(assignmentResult.OldRole.Value.RoleDiscordID));
		}

		Log.WriteVerbose("SetAssignmentRolesAsync", $"Adding role to user ({dbUser.DiscordID}).");
		await user.AddRoleAsync(ulong.Parse(assignmentResult.NewRole.RoleDiscordID));
	}
}
