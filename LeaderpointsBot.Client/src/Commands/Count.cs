using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Actions;
using LeaderpointsBot.Client.Embeds;
using LeaderpointsBot.Client.Exceptions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class CountModule
{
	public static async Task<Structures.Commands.CountModule.UserLeaderboardsCountMessages[]> UserLeaderboardsCountBathbotAsync(DiscordSocketClient client, SocketGuild guild, Embed topsCount)
	{
		if(!topsCount.Author.HasValue)
		{
			throw new ClientException("Invalid embed passed to method.");
		}

		string embedUsername = Parser.ParseUsernameFromBathbotEmbedTitle(topsCount.Title);
		int embedOsuId = Parser.ParseOsuIDFromBathbotEmbedLink(topsCount.Author.Value.Url);
		int[,] embedTopCounts; // assume non-respektive

		await Log.WriteInfo("UserLeaderboardsCountBathbotAsync", $"Calculating points for osu! username: { embedUsername }");

		try
		{
			await Log.WriteVerbose("UserLeaderboardsCountBathbotAsync", "Parsing top counts from embed description.");
			embedTopCounts = Parser.ParseTopPointsFromBathbotEmbedDescription(topsCount.Description);
		}
		catch (Exception e)
		{
			await Log.WriteError("UserLeaderboardsCountBathbotAsync", $"An unhandled client exception occurred.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
			throw new SendMessageException("Unhandled client error occurred.");
		}

		// Bathbot doesn't use respektive's API at the moment
		int points = Counter.CalculateTopPoints(embedTopCounts);
		Structures.Actions.Counter.UpdateUserDataMessages updateMessages = await CounterActions.UpdateUserDataAsync(guild, embedOsuId, points);

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.EMBED,
				Contents = Counter.CreateCountEmbed(embedUsername, embedTopCounts)
			},
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.TEXT,
				Contents = updateMessages.PointsMessage
			}
		};

		if(!string.IsNullOrWhiteSpace(updateMessages.RoleMessage))
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.TEXT,
				Contents = updateMessages.RoleMessage
			});
		}

		return responses.ToArray();
	}
}
