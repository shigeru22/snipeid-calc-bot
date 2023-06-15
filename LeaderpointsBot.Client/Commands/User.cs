// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Api;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Commands;

public static class User
{
	public static async Task ReapplyUserRoles(SocketGuildUser user)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Checking user in database (user ID {user.Id}).");

		Users.UsersTableData userData;
		try
		{
			userData = await Users.GetUserByDiscordID(transaction, user.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteVerbose($"User with ID {user.Id} not found in database. Cancelling roles reapplication.");
			return;
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		Log.WriteVerbose($"Checking user roles in server (user ID {user.Id}, server ID {user.Guild.Id}).");

		Assignments.AssignmentsTableData guildUserRole;
		try
		{
			guildUserRole = await Assignments.GetAssignmentByUserDiscordID(transaction, user.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteVerbose($"User assignment data with ID {user.Id} not found in database. Cancelling roles reapplication.");
			return;
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		Log.WriteVerbose($"Checking server verified role settings.");

		Servers.ServersTableData guildData;
		try
		{
			guildData = await Servers.GetServerByDiscordID(transaction, user.Guild.Id.ToString());
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		if (guildData.VerifiedRoleID != null)
		{
			Log.WriteVerbose($"Granting user verified role (user ID {user.Id}, server ID {user.Guild.Id}).");
			await Actions.Roles.SetVerifiedRoleAsync(transaction, user.Guild, user);
		}

		Roles.RolesTableData roleData;
		try
		{
			roleData = await Roles.GetRoleByRoleID(transaction, guildUserRole.RoleID);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		if (!string.IsNullOrWhiteSpace(roleData.DiscordID))
		{
			await Actions.Roles.SetAssignmentRolesAsync(transaction, user, new Structures.Actions.UserData.AssignmentResult()
			{
				NewRole = new Structures.Actions.UserData.AssignmentResultRoleData()
				{
					RoleDiscordID = roleData.DiscordID,
					RoleName = roleData.RoleName
				}
			});
		}

		await transaction.CommitAsync();
	}

	public static async Task<ReturnMessage> LinkUser(SocketUser user, int osuId, SocketGuild? guild = null)
	{
		// TODO: [2023-01-26] use OAuth?

		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose($"Checking user in database (user ID {user.Id}).");

		try
		{
			_ = await Users.GetUserByDiscordID(transaction, user.Id.ToString());

			Log.WriteInfo($"User with ID {user.Id} already linked (in database). Sending error message.");
			throw new SendMessageException("You've already linked your osu! account.", true);
		}
		catch (DataNotFoundException)
		{
			// continue
			Log.WriteVerbose($"User with ID {user.Id} not found in database.");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		Log.WriteVerbose($"Checking user in database (osu! ID {osuId}).");

		try
		{
			_ = await Users.GetUserByOsuID(transaction, osuId);

			Log.WriteInfo($"osu! ID {osuId} already linked by someone (in database). Sending error message.");
			throw new SendMessageException("osu! account already linked.", true);
		}
		catch (DataNotFoundException)
		{
			// continue
			Log.WriteVerbose($"User with osu! ID {osuId} not found in database.");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		Log.WriteVerbose($"Fetching osu! user from osu!api (osu! ID {osuId}).");

		OsuDataTypes.OsuApiUserResponseData osuUser;

		try
		{
			// always fetch latest data (don't use cache)
			// store response instead after fetch

			osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
			CacheManager.Instance.OsuApiCacheInstance.AddOsuUserCache(osuId, osuUser);
		}
		catch (ApiResponseException)
		{
			Log.WriteError("osu!api error occurred. Sending error message.");
			throw new SendMessageException("osu!api error occurred. Check status?", true);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		Log.WriteVerbose($"Inserting Discord user to database (user ID {user.Id}).");

		await Users.InsertUser(transaction, user.Id.ToString(), osuId, osuUser.Username, osuUser.CountryCode);

		if (guild != null)
		{
			Log.WriteVerbose("Message sent from server. Granting server verified role (if set).");
			await Actions.Roles.SetVerifiedRoleAsync(transaction, guild, user);
		}

		await transaction.CommitAsync();

		Log.WriteVerbose("Returning linked message as embed.");

		return new ReturnMessage()
		{
			Embed = Embeds.User.CreateLinkedEmbed(user.Id.ToString(), osuUser.Username, osuId)
		};
	}
}
