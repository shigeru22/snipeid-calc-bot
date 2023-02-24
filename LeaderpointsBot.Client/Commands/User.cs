// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Api;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Commands;

public static class User
{
	public static async Task<ReturnMessage> LinkUser(SocketUser user, int osuId, SocketGuild? guild = null)
	{
		// TODO: [2023-01-26] use OAuth?

		Log.WriteVerbose($"Checking user in database (user ID {user.Id}).");

		try
		{
			_ = await DatabaseFactory.Instance.UsersInstance.GetUserByDiscordID(user.Id.ToString());

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
			_ = await DatabaseFactory.Instance.UsersInstance.GetUserByOsuID(osuId);

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
			osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
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

		await DatabaseFactory.Instance.UsersInstance.InsertUser(user.Id.ToString(), osuId, osuUser.Username, osuUser.CountryCode);

		if (guild != null)
		{
			Log.WriteVerbose("Message sent from server. Granting server verified role (if set).");
			await Actions.Roles.SetVerifiedRoleAsync(guild, user);
		}

		return new ReturnMessage()
		{
			Embed = Embeds.User.CreateLinkedEmbed(user.Id.ToString(), osuUser.Username, osuId)
		};
	}
}
