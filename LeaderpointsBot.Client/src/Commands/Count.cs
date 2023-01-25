// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

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
	public static async Task<Structures.Commands.CountModule.UserLeaderboardsCountMessages[]> UserLeaderboardsCountBathbotAsync(Embed topsCount, SocketGuild guild)
	{
		if (!topsCount.Author.HasValue)
		{
			throw new ClientException("Invalid embed passed to method.");
		}

		string embedUsername = Parser.ParseUsernameFromBathbotEmbedTitle(topsCount.Title);
		int embedOsuId = Parser.ParseOsuIDFromBathbotEmbedLink(topsCount.Author.Value.Url);
		int[,] embedTopCounts; // assume non-respektive

		Log.WriteInfo("UserLeaderboardsCountBathbotAsync", $"Calculating points for osu! username: {embedUsername}");

		try
		{
			Log.WriteVerbose("UserLeaderboardsCountBathbotAsync", "Parsing top counts from embed description.");
			embedTopCounts = Parser.ParseTopPointsFromBathbotEmbedDescription(topsCount.Description);
		}
		catch (Exception e)
		{
			Log.WriteError("UserLeaderboardsCountBathbotAsync", $"An unhandled client exception occurred.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
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
			Log.WriteVerbose("UserLeaderboardsCountBathbotAsync", "No updateMessages set.");
		}

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new List<Structures.Commands.CountModule.UserLeaderboardsCountMessages>()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.Embed,
				Contents = Counter.CreateCountEmbed(embedUsername, embedTopCounts)
			}
		};

		if (updateMessages.HasValue)
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.Text,
				Contents = updateMessages.Value.PointsMessage
			});

			if (!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
				{
					MessageType = Common.ResponseMessageType.Text,
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
		if (guild != null)
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
				Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching server.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}
		}

		int osuId;
		string osuUsername;

		try
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"Fetching user data from database (Discord ID {userDiscordId}).");

			UsersQuerySchema.UsersTableData dbUser = await DatabaseFactory.Instance.UsersInstance.GetUserByDiscordID(userDiscordId);
			osuId = dbUser.OsuID;
		}
		catch (DataNotFoundException)
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"User not found in database (Discord ID {userDiscordId}). Sending link message.");
			throw new SendMessageException($"Not yet linked to osu! user. Link using <@{clientDiscordId}>` link [osu! user ID]`{(dbServer != null && dbServer.Value.CommandsChannelID != null ? $" at <#{dbServer.Value.CommandsChannelID}>" : string.Empty)}.", true);
		}
		catch (Exception e)
		{
			Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching user.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		try
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"Fetching osu! user data from osu!api (osu! ID {osuId}).");

			OsuDataTypes.OsuApiUserResponseData osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
			osuUsername = osuUser.Username;
		}
		catch (Exception e)
		{
			Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu! user.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		List<int[]> topCounts = new List<int[]>();
		int points;

		if (!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };
			int ranksLength = ranks.Length;

			List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new List<Task<OsuStatsDataTypes.OsuStatsResponseData>>();
			foreach (int rank in ranks)
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
				Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu!stats data.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			foreach (OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
			{
				topCounts.Add(new int[] { response.MaxRank, response.Count });
			}

			topCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			points = Counter.CalculateTopPoints(topCounts);
		}
		else
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse;

			try
			{
				osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);
			}
			catch (Exception e)
			{
				Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching respektive osu!stats data.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			topCounts.Add(new int[] { 1, osuStatsResponse.Top1 ?? 0 });
			topCounts.Add(new int[] { 8, osuStatsResponse.Top8 ?? 0 });
			topCounts.Add(new int[] { 25, osuStatsResponse.Top25 ?? 0 });
			topCounts.Add(new int[] { 50, osuStatsResponse.Top50 ?? 0 });

			points = Counter.CalculateTopPoints(topCounts);
		}

		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;
		if (guild != null && dbServer != null)
		{
			updateMessages = await CounterActions.UpdateUserDataAsync(guild, osuId, points);
		}

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new List<Structures.Commands.CountModule.UserLeaderboardsCountMessages>()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.Embed,
				Contents = Counter.CreateCountEmbed(osuUsername, topCounts)
			}
		};

		if (updateMessages.HasValue)
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.Text,
				Contents = updateMessages.Value.PointsMessage
			});

			if (!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
				{
					MessageType = Common.ResponseMessageType.Text,
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
		if (guild != null)
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
				Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching server.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}
		}

		try
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", $"Fetching osu! user data from osu!api (osu! username {osuUsername}).");

			OsuDataTypes.OsuApiUserResponseData osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuUsername(osuUsername);
			tempOsuUsername = osuUser.Username;
			osuId = osuUser.ID;
		}
		catch (Exception e)
		{
			Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu! user.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
			throw new SendMessageException("Unhandled client error occurred.", true);
		}

		List<int[]> topCounts = new List<int[]>();
		int points;

		if (!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };
			int ranksLength = ranks.Length;

			List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new List<Task<OsuStatsDataTypes.OsuStatsResponseData>>();
			foreach (int rank in ranks)
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
				Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching osu!stats data.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			foreach (OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
			{
				topCounts.Add(new int[] { response.MaxRank, response.Count });
			}

			topCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			points = Counter.CalculateTopPoints(topCounts);
		}
		else
		{
			Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse;

			try
			{
				osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);
			}
			catch (Exception e)
			{
				Log.WriteError("CountLeaderboardPointsByDiscordUserAsync", $"An unhandled exception occurred while fetching respektive osu!stats data.{(Settings.Instance.Client.Logging.LogSeverity >= 4 ? $" Exception details below.\n{e}" : string.Empty)}");
				throw new SendMessageException("Unhandled client error occurred.", true);
			}

			topCounts.Add(new int[] { 1, osuStatsResponse.Top1 ?? 0 });
			topCounts.Add(new int[] { 8, osuStatsResponse.Top8 ?? 0 });
			topCounts.Add(new int[] { 25, osuStatsResponse.Top25 ?? 0 });
			topCounts.Add(new int[] { 50, osuStatsResponse.Top50 ?? 0 });

			points = Counter.CalculateTopPoints(topCounts);
		}

		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;
		if (guild != null && dbServer != null)
		{
			try
			{
				updateMessages = await CounterActions.UpdateUserDataAsync(guild, osuId, points);
			}
			catch (SkipUpdateException)
			{
				Log.WriteVerbose("CountLeaderboardPointsByDiscordUserAsync", "No updateMessages set.");
			}
		}

		List<Structures.Commands.CountModule.UserLeaderboardsCountMessages> responses = new List<Structures.Commands.CountModule.UserLeaderboardsCountMessages>()
		{
			new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.Embed,
				Contents = Counter.CreateCountEmbed(osuUsername, topCounts)
			}
		};

		if (updateMessages.HasValue)
		{
			responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
			{
				MessageType = Common.ResponseMessageType.Text,
				Contents = updateMessages.Value.PointsMessage
			});

			if (!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new Structures.Commands.CountModule.UserLeaderboardsCountMessages()
				{
					MessageType = Common.ResponseMessageType.Text,
					Contents = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}
}
