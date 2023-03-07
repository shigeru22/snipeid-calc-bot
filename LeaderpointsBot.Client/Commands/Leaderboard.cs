// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Commands;

public static class Leaderboard
{
	public static async Task<ReturnMessage> GetServerLeaderboard(string guildDiscordId)
	{
		UsersQuerySchema.UsersLeaderboardData[] serverLeaderboardData;
		DateTime lastUpdate;

		try
		{
			Log.WriteVerbose($"Fetching leaderboard data from database (guild ID {guildDiscordId}).");

			serverLeaderboardData = await DatabaseFactory.Instance.UsersInstance.GetServerPointsLeaderboard(guildDiscordId);
			lastUpdate = await DatabaseFactory.Instance.UsersInstance.GetServerLastPointUpdate(guildDiscordId);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		if (serverLeaderboardData.Length <= 0)
		{
			throw new SendMessageException("Leaderboard is empty. Go for the first!");
		}

		Log.WriteVerbose("Returning leaderboard data as embed.");

		return new ReturnMessage()
		{
			Embed = Embeds.Leaderboard.CreateLeaderboardEmbed(serverLeaderboardData,
				lastUpdate,
				useLegacyColor: Actions.Channel.IsSnipeIDGuild(guildDiscordId))
		};
	}
}
