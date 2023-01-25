// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class Roles
{
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
