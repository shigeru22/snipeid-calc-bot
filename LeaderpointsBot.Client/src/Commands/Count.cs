using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Api;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Api.OsuStats;
using LeaderpointsBot.Client.Actions;
using LeaderpointsBot.Client.Embeds;
using LeaderpointsBot.Client.Exceptions;
using LeaderpointsBot.Client.Exceptions.Actions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class CountModule
{
	public static async Task<Structures.Commands.CountModule.UserLeaderboardsCountMessages[]> UserLeaderboardsCountBathbotAsync(Embed topsCount, DiscordSocketClient client, SocketGuild guild)
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
		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;

		try
		{
			updateMessages = await CounterActions.UpdateUserDataAsync(guild, embedOsuId, points);
		}
		catch (SkipUpdateException)
		{
			await Log.WriteVerbose("UserLeaderboardsCountBathbotAsync", "No updateMessages set.");
		}

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.EMBED,
				Contents = Counter.CreateCountEmbed(embedUsername, embedTopCounts)
			}
		};

		if(updateMessages.HasValue)
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.TEXT,
				Contents = updateMessages.Value.PointsMessage
			});

			if(!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
				{
					MessageType = Common.ResponseMessageType.TEXT,
					Contents = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}

	public static async Task<Structures.Commands.CountModule.UserLeaderboardsCountMessages[]> CountLeaderboardPointsByDiscordUserAsync(string userDiscordId, string clientDiscordId, SocketGuild? guild = null)
	{
		// TODO: [2023-01-21] extract reused procedures as methods

		ServersQuerySchema.ServersTableData? dbServer = null;
		if(guild != null)
		{
			try
			{
				dbServer = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
			catch (Exception e)
			{
				await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching server.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}
		}
		
		int osuId;
		string osuUsername;

		try
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"Fetching user data from database (Discord ID { userDiscordId }).");

			UsersQuerySchema.UsersTableData dbUser = await DatabaseFactory.Instance.UsersInstance.GetUserByDiscordID(userDiscordId);
			osuId = dbUser.OsuID;
		}
		catch (DataNotFoundException)
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"User not found in database (Discord ID { userDiscordId }). Sending link message.");
			throw new SendMessageException($"Not yet linked to osu! user. Link using <@{ clientDiscordId }>` link [osu! user ID]`{ (dbServer != null && dbServer.Value.CommandsChannelID != null ? $" at <#{ dbServer.Value.CommandsChannelID }>" : "") }.", true);
		}
		catch (Exception e)
		{
			await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching user.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		try
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"Fetching osu! user data from osu!api (osu! ID { osuId }).");

			OsuDataTypes.OsuApiUserResponseData osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
			osuUsername = osuUser.Username;
		}
		catch (Exception e)
		{
			await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu! user.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		List<int[]> topCounts = new();
		int points;

		if(!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };
			int ranksLength = ranks.Length;

			List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new();
			foreach(int rank in ranks)
			{
				osuStatsRequests.Add(ApiFactory.Instance.OsuStatsInstance.GetTopCounts(osuUsername, rank));
			}

			OsuStatsDataTypes.OsuStatsResponseData[] osuStatsResponses;

			try
			{
				osuStatsResponses = await Task.WhenAll(osuStatsRequests);
			}
			catch (Exception e)
			{
				await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu!stats data.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			foreach(OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
			{
				topCounts.Add(new int[] { response.MaxRank, response.Count });
			}

			topCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			points = Counter.CalculateTopPoints(topCounts);
		}
		else
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse;

			try
			{
				osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);
			}
			catch (Exception e)
			{
				await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching respektive osu!stats data.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			topCounts.Add(new int[] { 1, (osuStatsResponse.Top1.HasValue ? osuStatsResponse.Top1.Value : 0) });
			topCounts.Add(new int[] { 8, (osuStatsResponse.Top8.HasValue ? osuStatsResponse.Top8.Value : 0) });
			topCounts.Add(new int[] { 25, (osuStatsResponse.Top25.HasValue ? osuStatsResponse.Top25.Value : 0) });
			topCounts.Add(new int[] { 50, (osuStatsResponse.Top50.HasValue ? osuStatsResponse.Top50.Value : 0) });

			points = Counter.CalculateTopPoints(topCounts);
		}

		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;
		if(guild != null && dbServer != null)
		{
			updateMessages = await CounterActions.UpdateUserDataAsync(guild, osuId, points);
		}

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.EMBED,
				Contents = Counter.CreateCountEmbed(osuUsername, topCounts)
			}
		};

		if(updateMessages.HasValue)
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.TEXT,
				Contents = updateMessages.Value.PointsMessage
			});

			if(!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
				{
					MessageType = Common.ResponseMessageType.TEXT,
					Contents = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}

	public static async Task<Structures.Commands.CountModule.UserLeaderboardsCountMessages[]> CountLeaderboardPointsByOsuUsernameAsync(string osuUsername, SocketGuild? guild = null)
	{
		// TODO: [2023-01-21] extract reused procedures as methods

		string tempOsuUsername;
		int osuId;

		ServersQuerySchema.ServersTableData? dbServer = null;
		if(guild != null)
		{
			try
			{
				dbServer = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
			catch (Exception e)
			{
				await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching server.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}
		}

		try
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"Fetching osu! user data from osu!api (osu! username { osuUsername }).");

			OsuDataTypes.OsuApiUserResponseData osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuUsername(osuUsername);
			tempOsuUsername = osuUser.Username;
			osuId = osuUser.ID;
		}
		catch (Exception e)
		{
			await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu! user.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		List<int[]> topCounts = new();
		int points;

		if(!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };
			int ranksLength = ranks.Length;

			List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new();
			foreach(int rank in ranks)
			{
				osuStatsRequests.Add(ApiFactory.Instance.OsuStatsInstance.GetTopCounts(osuUsername, rank));
			}

			OsuStatsDataTypes.OsuStatsResponseData[] osuStatsResponses;

			try
			{
				osuStatsResponses = await Task.WhenAll(osuStatsRequests);
			}
			catch (Exception e)
			{
				await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu!stats data.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			foreach(OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
			{
				topCounts.Add(new int[] { response.MaxRank, response.Count });
			}

			topCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			points = Counter.CalculateTopPoints(topCounts);
		}
		else
		{
			await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse;

			try
			{
				osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);
			}
			catch (Exception e)
			{
				await Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching respektive osu!stats data.{ (Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{ e }" : "") }");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			topCounts.Add(new int[] { 1, (osuStatsResponse.Top1.HasValue ? osuStatsResponse.Top1.Value : 0) });
			topCounts.Add(new int[] { 8, (osuStatsResponse.Top8.HasValue ? osuStatsResponse.Top8.Value : 0) });
			topCounts.Add(new int[] { 25, (osuStatsResponse.Top25.HasValue ? osuStatsResponse.Top25.Value : 0) });
			topCounts.Add(new int[] { 50, (osuStatsResponse.Top50.HasValue ? osuStatsResponse.Top50.Value : 0) });

			points = Counter.CalculateTopPoints(topCounts);
		}

		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;
		if(guild != null && dbServer != null)
		{
			try
			{
				updateMessages = await CounterActions.UpdateUserDataAsync(guild, osuId, points);
			}
			catch (SkipUpdateException)
			{
				await Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "No updateMessages set.");
			}
		}

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.EMBED,
				Contents = Counter.CreateCountEmbed(osuUsername, topCounts)
			}
		};

		if(updateMessages.HasValue)
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.TEXT,
				Contents = updateMessages.Value.PointsMessage
			});

			if(!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
				{
					MessageType = Common.ResponseMessageType.TEXT,
					Contents = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}
}
