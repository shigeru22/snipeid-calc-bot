using System.Net;
using Discord.WebSocket;
using LeaderpointsBot.Api;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Client.Exceptions.Actions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures.Actions;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class CounterActions
{
	public static async Task<Counter.UpdateUserDataMessages?> UpdateUserDataAsync(SocketGuild guild, int osuId, int points)
	{
		OsuDataTypes.OsuApiUserResponseData osuUser;
		Structures.Actions.UserData.AssignmentResult assignmentResult;

		try
		{
			await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			await Log.WriteError("UpdateUserDataAsync", $"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found.", true);
		}
		catch (Exception e)
		{
			await Log.WriteError("UpdateUserDataAsync", $"An unhandled error occurred while querying server.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : "")}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		await Log.WriteVerbose("UpdateUserDataAsync", $"Updating user data for osu! ID {osuId}.");

		try
		{
			osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
		}
		catch (ApiResponseException e)
		{
			if(e.Code != HttpStatusCode.NotFound)
			{
				throw new SendMessageException("osu! user not found.", true);
			}

			throw new SendMessageException("osu!api error occurred.", true);
		}
		catch (Exception)
		{
			await Log.WriteError("UpdateUserDataAsync", $"An unhandled error occurred while retrieving osu! user.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? " See above errors for details." : "")}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		{
			if(osuUser.IsBot)
			{
				throw new SendMessageException("Suddenly, you turned into a skynet...", true);
			}

			if(osuUser.IsDeleted)
			{
				throw new SendMessageException("Did you do something to your osu! account?", true);
			}
		}

		try
		{
			assignmentResult = await UserData.InsertOrUpdateAssignment(guild.Id.ToString(), osuId, osuUser.Username, osuUser.CountryCode, points);
		}
		catch (SkipUpdateException)
		{
			await Log.WriteVerbose("UpdateUserDataAsync", "Data update skipped. Returning update messages as null.");
			return null;
		}
		catch (Exception)
		{
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		try
		{
			await Roles.SetAssignmentRolesAsync(guild, osuId, assignmentResult);
		}
		catch (Exception)
		{
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		await Log.WriteDebug("UpdateUserDataAsync", $"assignmentResult.LastUpdate = {assignmentResult.LastUpdate}");

		return new Counter.UpdateUserDataMessages()
		{
			PointsMessage = assignmentResult.LastUpdate.HasValue switch
			{
				true => $"<@{ assignmentResult.UserDiscordID }> has {(points >= 0 ? "gained" : "lost") } { assignmentResult.DeltaPoints } point{ (points != 1 ? "s" : "") } since { Date.DeltaTimeToString(assignmentResult.LastUpdate.Value - DateTime.Now) } ago.",
				_ => $"<@{ assignmentResult.UserDiscordID }> achieved {points} points. Go for those leaderboards!"
			},
			RoleMessage = assignmentResult.OldRole.HasValue switch
			{
				true => assignmentResult.NewRole.RoleDiscordID.Equals(assignmentResult.OldRole?.RoleDiscordID) switch
				{
					false => $"You have been { (assignmentResult.DeltaPoints > 0 ? "promoted" : "demoted") } to **{ assignmentResult.NewRole.RoleName }** role. { (assignmentResult.DeltaPoints > 0 ? "Nice job!" : "Fight back for those leaderboards!") }",
					_ => null
				},
				_ => null
			}
		};
	}
}