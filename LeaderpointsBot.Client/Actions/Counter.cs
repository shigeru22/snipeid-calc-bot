// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;
using Discord.WebSocket;
using LeaderpointsBot.Api;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Client.Exceptions.Actions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Actions;

public static class Counter
{
	public static async Task<Structures.Actions.Counter.UpdateUserDataMessages?> UpdateUserDataAsync(DatabaseTransaction transaction, SocketGuild guild, int osuId, int points)
	{
		OsuDataTypes.OsuApiUserResponseData osuUser;
		Structures.Actions.UserData.AssignmentResult assignmentResult;

		try
		{
			_ = await Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found.", true);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		Log.WriteVerbose($"Updating user data for osu! ID {osuId}.");

		try
		{
			OsuDataTypes.OsuApiUserResponseData? tempUser = CacheManager.Instance.OsuApiCacheInstance.GetOsuUserCache(osuId);

			if (!tempUser.HasValue)
			{
				tempUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
				CacheManager.Instance.OsuApiCacheInstance.AddOsuUserCache(osuId, tempUser.Value);
			}

			osuUser = tempUser.Value;
		}
		catch (ApiResponseException e)
		{
			if (e.Code != HttpStatusCode.NotFound)
			{
				throw new SendMessageException("osu! user not found.", true);
			}

			throw new SendMessageException("osu!api error occurred.", true);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		{
			if (osuUser.IsBot)
			{
				throw new SendMessageException("Suddenly, you turned into a skynet...", true);
			}

			if (osuUser.IsDeleted)
			{
				throw new SendMessageException("Did you do something to your osu! account?", true);
			}
		}

		try
		{
			assignmentResult = await UserData.InsertOrUpdateAssignment(transaction, guild.Id.ToString(), osuId, osuUser.Username, osuUser.CountryCode, points);
		}
		catch (SkipUpdateException)
		{
			Log.WriteVerbose("Data update skipped. Returning update messages as null.");
			return null;
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		try
		{
			await Roles.SetAssignmentRolesAsync(transaction, guild, osuId, assignmentResult);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		Log.WriteVerbose("Returning user update result message data.");

		return new Structures.Actions.Counter.UpdateUserDataMessages()
		{
			PointsMessage = assignmentResult.LastUpdate.HasValue switch
			{
				true => $"<@{assignmentResult.UserDiscordID}> has {(assignmentResult.DeltaPoints >= 0 ? "gained" : "lost")} **{assignmentResult.DeltaPoints}** point{(Math.Abs(assignmentResult.DeltaPoints) != 1 ? "s" : string.Empty)} since {Date.DeltaTimeToString(assignmentResult.LastUpdate.Value - DateTime.Now)} ago.",
				_ => $"<@{assignmentResult.UserDiscordID}> achieved {points} points. Go for those leaderboards!"
			},
			RoleMessage = assignmentResult.OldRole.HasValue switch
			{
				true => assignmentResult.NewRole.RoleDiscordID.Equals(assignmentResult.OldRole?.RoleDiscordID) switch
				{
					false => $"You have been {(assignmentResult.DeltaPoints > 0 ? "promoted" : "demoted")} to **{assignmentResult.NewRole.RoleName}** role. {(assignmentResult.DeltaPoints > 0 ? "Nice job!" : "Fight back for those leaderboards!")}",
					_ => null
				},
				_ => null
			}
		};
	}
}
