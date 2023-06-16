// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Api;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Api.OsuStats;
using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Client.Exceptions;
using LeaderpointsBot.Client.Exceptions.Actions;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Commands;

public static class Counter
{
	// Bathbot count (<osc) message
	public static async Task<ReturnMessage[]> CountBathbotLeaderboardPointsAsync(Embed countEmbed, SocketGuildChannel? guildChannel = null)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		if (guildChannel != null)
		{
			(bool isChannelAllowed, _) = await Actions.Channel.IsClientCommandsAllowedAsync(transaction, guildChannel);
			if (!isChannelAllowed)
			{
				throw new InterruptProcessException("Ignoring since channel is not guild commands channel.");
			}
		}

		Servers.ServersTableData? dbServer = null;
		if (guildChannel != null)
		{
			try
			{
				dbServer = await Servers.GetServerByDiscordID(transaction, guildChannel.Guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		Log.WriteVerbose("Calculating leaderboards count from first embed.");
		if (!countEmbed.Author.HasValue)
		{
			throw new ClientException("Invalid embed passed to method.");
		}

		string embedUsername = Parser.ParseUsernameFromBathbotEmbedTitle(countEmbed.Title);
		int embedOsuId = Parser.ParseOsuIDFromBathbotEmbedLink(countEmbed.Author.Value.Url);
		int[,] embedTopCounts; // assume non-respektive

		Log.WriteInfo($"Calculating points for osu! username: {embedUsername}");

		Log.WriteVerbose("Parsing top counts from embed description.");
		embedTopCounts = Parser.ParseTopPointsFromBathbotEmbedDescription(countEmbed.Description);

		// Bathbot doesn't use respektive's API at the moment
		int points = Embeds.Counter.CalculateTopPoints(embedTopCounts);
		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;

		if (guildChannel != null && dbServer != null)
		{
			try
			{
				updateMessages = await Actions.Counter.UpdateUserDataAsync(transaction, guildChannel.Guild, embedOsuId, points);
			}
			catch (SkipUpdateException)
			{
				Log.WriteVerbose("No updateMessages set.");
			}
		}

		await transaction.CommitAsync();

		List<ReturnMessage> responses = new List<ReturnMessage>()
		{
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateCountEmbed(embedUsername,
					embedTopCounts,
					false,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild.Id.ToString()))
			}
		};

		if (updateMessages.HasValue)
		{
			responses.Add(new ReturnMessage()
			{
				Message = updateMessages.Value.PointsMessage
			});

			if (!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new ReturnMessage()
				{
					Message = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}

	public static async Task<ReturnMessage[]> CountLeaderboardPointsByDiscordUserAsync(string userDiscordId, string clientDiscordId, SocketGuildChannel? guildChannel = null)
	{
		// TODO: [2023-01-21] extract reused procedures as methods

		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		if (guildChannel != null)
		{
			await Actions.Channel.CheckCommandChannelAsync(transaction, guildChannel, Actions.Channel.GuildChannelType.COMMANDS);
		}

		Servers.ServersTableData? dbServer = null;
		if (guildChannel != null)
		{
			try
			{
				dbServer = await Servers.GetServerByDiscordID(transaction, guildChannel.Guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		int osuId;
		string osuUsername;

		try
		{
			Log.WriteVerbose($"Fetching user data from database (Discord ID {userDiscordId}).");

			Users.UsersTableData dbUser = await Users.GetUserByDiscordID(transaction, userDiscordId);
			osuId = dbUser.OsuID;
		}
		catch (DataNotFoundException)
		{
			Log.WriteVerbose($"User not found in database (Discord ID {userDiscordId}). Sending link message.");
			throw new SendMessageException($"Not yet linked to osu! user. Link using <@{clientDiscordId}>` link [osu! user ID]`{(dbServer != null && dbServer.Value.CommandsChannelID != null ? $" at <#{dbServer.Value.CommandsChannelID}>" : string.Empty)}.", true);
		}

		Log.WriteVerbose($"Fetching osu! user data from osu!api (osu! ID {osuId}).");

		OsuDataTypes.OsuApiUserResponseData? tempUser = CacheManager.Instance.OsuApiCacheInstance.GetOsuUserCache(osuId);
		if (!tempUser.HasValue)
		{
			tempUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
			CacheManager.Instance.OsuApiCacheInstance.AddOsuUserCache(osuId, tempUser.Value);
		}

		osuUsername = tempUser.Value.Username;
		List<int[]> topCounts = new List<int[]>();
		int points;

		if (!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			Log.WriteVerbose("Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };

			bool isComplete = true;
			foreach (int rank in ranks)
			{
				OsuStatsDataTypes.OsuStatsResponseData? tempData = CacheManager.Instance.OsuStatsCacheInstance.GetOsuStatsCache(osuUsername, rank);

				if (!tempData.HasValue)
				{
					isComplete = false;
					break;
				}

				topCounts.Add(new int[] { tempData.Value.MaxRank, tempData.Value.Count });
			}

			if (!isComplete)
			{
				List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new List<Task<OsuStatsDataTypes.OsuStatsResponseData>>();

				foreach (int rank in ranks)
				{
					osuStatsRequests.Add(ApiFactory.Instance.OsuStatsInstance.GetTopCounts(osuUsername, rank));
				}

				OsuStatsDataTypes.OsuStatsResponseData[] osuStatsResponses = await Task.WhenAll(osuStatsRequests);

				foreach (OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
				{
					CacheManager.Instance.OsuStatsCacheInstance.AddOsuStatsCache(osuUsername, response.MaxRank, response);
				}

				topCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			}

			points = Embeds.Counter.CalculateTopPoints(topCounts);
		}
		else
		{
			Log.WriteVerbose("Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);

			topCounts.Add(new int[] { 1, osuStatsResponse.Top1 ?? 0 });
			topCounts.Add(new int[] { 8, osuStatsResponse.Top8 ?? 0 });
			topCounts.Add(new int[] { 25, osuStatsResponse.Top25 ?? 0 });
			topCounts.Add(new int[] { 50, osuStatsResponse.Top50 ?? 0 });

			points = Embeds.Counter.CalculateTopPoints(topCounts);
		}

		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;
		if (guildChannel != null && dbServer != null)
		{
			updateMessages = await Actions.Counter.UpdateUserDataAsync(transaction, guildChannel.Guild, osuId, points);
		}

		await transaction.CommitAsync();

		List<ReturnMessage> responses = new List<ReturnMessage>()
		{
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateTopsEmbed(osuUsername,
					topCounts,
					osuId,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild))
			},
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateCountEmbed(osuUsername,
					topCounts,
					false,
					Settings.Instance.OsuApi.UseRespektiveStats,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild))
			}
		};

		if (updateMessages.HasValue)
		{
			responses.Add(new ReturnMessage()
			{
				Message = updateMessages.Value.PointsMessage
			});

			if (!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new ReturnMessage()
				{
					Message = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}

	public static async Task<ReturnMessage[]> CountLeaderboardPointsByOsuUsernameAsync(string osuUsername, SocketGuildChannel? guildChannel = null)
	{
		// TODO: [2023-01-21] extract reused procedures as methods

		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		if (guildChannel != null)
		{
			await Actions.Channel.CheckCommandChannelAsync(transaction, guildChannel, Actions.Channel.GuildChannelType.COMMANDS);
		}

		string tempOsuUsername;
		int osuId;

		Servers.ServersTableData? dbServer = null;
		if (guildChannel != null)
		{
			try
			{
				dbServer = await Servers.GetServerByDiscordID(transaction, guildChannel.Guild.Id.ToString());
			}
			catch (DataNotFoundException)
			{
				throw new SendMessageException("Server not found in our end!", true);
			}
		}

		Log.WriteVerbose($"Fetching osu! user data from osu!api (osu! username {osuUsername}).");

		// TODO: find cache by username
		OsuDataTypes.OsuApiUserResponseData osuUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuUsername(osuUsername);
		tempOsuUsername = osuUser.Username;
		osuId = osuUser.ID;

		List<int[]> topCounts = new List<int[]>();
		int points;

		if (!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			Log.WriteVerbose("Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };

			bool isComplete = true;
			foreach (int rank in ranks)
			{
				OsuStatsDataTypes.OsuStatsResponseData? tempData = CacheManager.Instance.OsuStatsCacheInstance.GetOsuStatsCache(osuUsername, rank);

				if (!tempData.HasValue)
				{
					isComplete = false;
					break;
				}

				topCounts.Add(new int[] { tempData.Value.MaxRank, tempData.Value.Count });
			}

			if (!isComplete)
			{
				List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new List<Task<OsuStatsDataTypes.OsuStatsResponseData>>();

				foreach (int rank in ranks)
				{
					osuStatsRequests.Add(ApiFactory.Instance.OsuStatsInstance.GetTopCounts(osuUsername, rank));
				}

				OsuStatsDataTypes.OsuStatsResponseData[] osuStatsResponses = await Task.WhenAll(osuStatsRequests);

				foreach (OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
				{
					CacheManager.Instance.OsuStatsCacheInstance.AddOsuStatsCache(osuUsername, response.MaxRank, response);
				}

				topCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			}

			points = Embeds.Counter.CalculateTopPoints(topCounts);
		}
		else
		{
			Log.WriteVerbose("Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);

			topCounts.Add(new int[] { 1, osuStatsResponse.Top1 ?? 0 });
			topCounts.Add(new int[] { 8, osuStatsResponse.Top8 ?? 0 });
			topCounts.Add(new int[] { 25, osuStatsResponse.Top25 ?? 0 });
			topCounts.Add(new int[] { 50, osuStatsResponse.Top50 ?? 0 });

			points = Embeds.Counter.CalculateTopPoints(topCounts);
		}

		Structures.Actions.Counter.UpdateUserDataMessages? updateMessages = null;
		if (guildChannel != null && dbServer != null)
		{
			try
			{
				updateMessages = await Actions.Counter.UpdateUserDataAsync(transaction, guildChannel.Guild, osuId, points);
			}
			catch (SkipUpdateException)
			{
				Log.WriteVerbose("No updateMessages set.");
			}
		}

		await transaction.CommitAsync();

		List<ReturnMessage> responses = new List<ReturnMessage>()
		{
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateTopsEmbed(osuUsername,
					topCounts,
					osuId,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild))
			},
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateCountEmbed(osuUsername,
					topCounts,
					false,
					Settings.Instance.OsuApi.UseRespektiveStats,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild))
			}
		};

		if (updateMessages.HasValue)
		{
			responses.Add(new ReturnMessage()
			{
				Message = updateMessages.Value.PointsMessage
			});

			if (!string.IsNullOrWhiteSpace(updateMessages.Value.RoleMessage))
			{
				responses.Add(new ReturnMessage()
				{
					Message = updateMessages.Value.RoleMessage
				});
			}
		}

		return responses.ToArray();
	}

	public static async Task<ReturnMessage[]> WhatIfUserCount(string userDiscordId, string arguments, SocketGuildChannel? guildChannel = null)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		if (guildChannel != null)
		{
			await Actions.Channel.CheckCommandChannelAsync(transaction, guildChannel, Actions.Channel.GuildChannelType.COMMANDS);
		}

		int osuId;
		string osuUsername;

		try
		{
			Log.WriteVerbose($"Fetching user data from database (Discord ID {userDiscordId}).");

			Users.UsersTableData dbUser = await Users.GetUserByDiscordID(transaction, userDiscordId);
			osuId = dbUser.OsuID;
		}
		catch (DataNotFoundException)
		{
			Log.WriteVerbose($"User not found in database (Discord ID {userDiscordId}). Sending link message.");
			throw new SendMessageException($"Not yet linked to osu! user. Link using `link` command first.", true);
		}

		int[,] whatIfs;
		try
		{
			whatIfs = Parser.ParseWhatIfArguments(arguments);
		}
		catch (InvalidDataException e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.UtilError.Message));
			throw new SendMessageException("Unhandled client error occurred.");
		}

		Log.WriteVerbose($"Fetching osu! user data from osu!api (osu! ID {osuId}).");

		OsuDataTypes.OsuApiUserResponseData? tempUser = CacheManager.Instance.OsuApiCacheInstance.GetOsuUserCache(osuId);

		if (!tempUser.HasValue)
		{
			tempUser = await ApiFactory.Instance.OsuApiInstance.GetUserByOsuID(osuId);
			CacheManager.Instance.OsuApiCacheInstance.AddOsuUserCache(osuId, tempUser.Value);
		}

		osuUsername = tempUser.Value.Username;

		List<int[]> originalTopCounts = new List<int[]>();
		List<int[]> whatIfTopCounts = new List<int[]>();
		int originalPoints;
		int whatIfPoints;

		if (!Settings.Instance.OsuApi.UseRespektiveStats)
		{
			Log.WriteVerbose("Fetching osu!stats data (top 1, 8, 15, 25, and 50).");

			int[] ranks = new int[] { 1, 8, 15, 25, 50 };
			int ranksLength = ranks.Length;

			bool isComplete = true;
			foreach (int rank in ranks)
			{
				OsuStatsDataTypes.OsuStatsResponseData? tempData = CacheManager.Instance.OsuStatsCacheInstance.GetOsuStatsCache(osuUsername, rank);

				if (!tempData.HasValue)
				{
					isComplete = false;
					break;
				}

				originalTopCounts.Add(new int[] { tempData.Value.MaxRank, tempData.Value.Count });
			}

			if (!isComplete)
			{
				List<Task<OsuStatsDataTypes.OsuStatsResponseData>> osuStatsRequests = new List<Task<OsuStatsDataTypes.OsuStatsResponseData>>();

				foreach (int rank in ranks)
				{
					osuStatsRequests.Add(ApiFactory.Instance.OsuStatsInstance.GetTopCounts(osuUsername, rank));
				}

				OsuStatsDataTypes.OsuStatsResponseData[] osuStatsResponses = await Task.WhenAll(osuStatsRequests);

				foreach (OsuStatsDataTypes.OsuStatsResponseData response in osuStatsResponses)
				{
					CacheManager.Instance.OsuStatsCacheInstance.AddOsuStatsCache(osuUsername, response.MaxRank, response);
				}

				originalTopCounts = osuStatsResponses.Select(response => new int[] { response.MaxRank, response.Count }).ToList();
			}

			originalPoints = Embeds.Counter.CalculateTopPoints(originalTopCounts);

			whatIfTopCounts = new List<int[]>();
			originalTopCounts.ForEach(top => whatIfTopCounts.Add(new int[] { top[0], top[1] }));

			int whatIfArgsLength = whatIfs.GetLength(0);
			for (int i = 0; i < whatIfArgsLength; i++)
			{
				int targetIndex = whatIfTopCounts.Select((topRank, index) => (topRank, index))
					.First(temp => temp.topRank[0] == whatIfs[i, 0])
					.index;
				whatIfTopCounts[targetIndex][1] = whatIfs[i, 1];
			}
			whatIfPoints = Embeds.Counter.CalculateTopPoints(whatIfTopCounts);
		}
		else
		{
			Log.WriteVerbose("Fetching respektive osu!stats data.");

			OsuStatsDataTypes.OsuStatsRespektiveResponseData osuStatsResponse = await ApiFactory.Instance.OsuStatsInstance.GetRespektiveTopCounts(osuId);

			originalTopCounts.Add(new int[] { 1, osuStatsResponse.Top1 ?? 0 });
			originalTopCounts.Add(new int[] { 8, osuStatsResponse.Top8 ?? 0 });
			originalTopCounts.Add(new int[] { 25, osuStatsResponse.Top25 ?? 0 });
			originalTopCounts.Add(new int[] { 50, osuStatsResponse.Top50 ?? 0 });

			originalPoints = Embeds.Counter.CalculateTopPoints(originalTopCounts, true);

			whatIfTopCounts = new List<int[]>();
			originalTopCounts.ForEach(top => whatIfTopCounts.Add(new int[] { top[0], top[1] }));

			int whatIfArgsLength = whatIfs.GetLength(0);
			for (int i = 0; i < whatIfArgsLength; i++)
			{
				int targetIndex = whatIfTopCounts.Select((topRank, index) => (topRank, index))
					.First(temp => temp.topRank[0] == whatIfs[i, 0])
					.index;
				whatIfTopCounts[targetIndex][1] = whatIfs[i, 1];
			}
			whatIfPoints = Embeds.Counter.CalculateTopPoints(whatIfTopCounts, true);
		}

		string retMessage;
		{
			int delta;
			if (whatIfPoints > originalPoints)
			{
				delta = whatIfPoints - originalPoints;
				retMessage = $"You would **gain {delta}** point{(delta != 1 ? "s" : string.Empty)} from original top count.";
			}
			else if (originalPoints > whatIfPoints)
			{
				delta = originalPoints - whatIfPoints;
				retMessage = $"You would **lose {delta}** point{(delta != 1 ? "s" : string.Empty)} from original top count.";
			}
			else
			{
				retMessage = "You would gain nothing from your original top count!";
			}
		}

		await transaction.CommitAsync();

		List<ReturnMessage> responses = new List<ReturnMessage>()
		{
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateCountEmbed(osuUsername,
					originalTopCounts,
					false,
					Settings.Instance.OsuApi.UseRespektiveStats,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild.Id.ToString()))
			},
			new ReturnMessage()
			{
				Embed = Embeds.Counter.CreateCountEmbed(osuUsername,
					whatIfTopCounts,
					true,
					Settings.Instance.OsuApi.UseRespektiveStats,
					guildChannel != null && Actions.Channel.IsSnipeIDGuild(guildChannel.Guild.Id.ToString()))
			},
			new ReturnMessage()
			{
				Message = retMessage
			}
		};

		return responses.ToArray();
	}
}
