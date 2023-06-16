// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class Leaderboard
{
	public static async Task<ReturnMessage> GetServerLeaderboard(SocketGuildChannel guildChannel)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		await Actions.Channel.CheckCommandChannelAsync(transaction, guildChannel, Actions.Channel.GuildChannelType.VERIFY);

		Log.WriteVerbose($"Fetching leaderboard data from database (guild ID {guildChannel.Guild.Id}).");

		Users.UsersLeaderboardData[] serverLeaderboardData = await Users.GetServerPointsLeaderboard(transaction, guildChannel.Guild.Id.ToString());
		DateTime lastUpdate = await Users.GetServerLastPointUpdate(transaction, guildChannel.Guild.Id.ToString());

		if (serverLeaderboardData.Length <= 0)
		{
			throw new SendMessageException("Leaderboard is empty. Go for the first!");
		}

		await transaction.CommitAsync();

		Log.WriteVerbose("Returning leaderboard data as embed.");

		return new ReturnMessage()
		{
			Embed = Embeds.Leaderboard.CreateLeaderboardEmbed(serverLeaderboardData,
				lastUpdate,
				useLegacyColor: Actions.Channel.IsSnipeIDGuild(guildChannel.Guild.Id.ToString()))
		};
	}
}
