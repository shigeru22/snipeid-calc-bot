// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;
using System.Text;
using System.Text.Json;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api.OsuStats;

public class OsuStatsApi
{
	public const string OsuStatsEndpoint = "https://osustats.ppy.sh/api";
	public const string OsuStatsRespektiveEndpoint = "https://osustats.respektive.pw";

	public OsuStatsApi()
	{
		Log.WriteVerbose("OsuStatsApi", "osu!stats API wrapper class instantiated.");
	}

	public async Task<OsuStatsDataTypes.OsuStatsResponseData> GetTopCounts(string osuUsername, int maxRank)
	{
		await Log.WriteVerbose("GetTopCounts", $"Requesting osu!stats data for {osuUsername} (maxRank = {maxRank}).");

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			OsuStatsDataTypes.OsuStatsRequestData requestData = new OsuStatsDataTypes.OsuStatsRequestData()
			{
				AccMin = 0.0,
				AccMax = 100.0,
				RankMin = 1,
				RankMax = maxRank,
				SortBy = 2,
				SortOrder = 1,
				Page = 1,
				Username = osuUsername
			};

			StringContent data = new StringContent(JsonSerializer.Serialize(requestData.ToRawData()), Encoding.UTF8, "application/json");

			await Log.WriteVerbose("GetTopCounts", "Requesting osu!stats getScores endpoint.");
			response = await client.PostAsync($"{OsuStatsEndpoint}/getScores", data);
		}
		catch (Exception e)
		{
			await Log.WriteError("GetTopCounts", $"An unhandled error occurred while requesting osu!stats data. Exception details below.\n{e}");
			throw new ApiInstanceException("Unhandled exception.");
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			await Log.WriteWarning("GetTopCounts", $"osu!stats API returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		JsonElement[]? content = JsonSerializer.Deserialize<JsonElement[]>(await response.Content.ReadAsStringAsync());

		if (content == null)
		{
			await Log.WriteError("GetTopCounts", "Content parsed as null.");
			throw new ApiInstanceException("Content parsed as null.");
		}

		/*
		 * osu!stats API response format (JSON, 200) is the following:
		 * [
		 *   RankDetails[],
		 *   number, // this is the rank count
		 *   boolean,
		 *   boolean
		 * ]
		 */

		await Log.WriteVerbose("GetTopCounts", "osu!stats user count retrieved successfully. Returning data as OsuStatsResponseData object.");
		return new OsuStatsDataTypes.OsuStatsResponseData()
		{
			Username = osuUsername,
			MaxRank = maxRank,
			Count = content[1].GetInt32() // integer boxed as object
		};
	}

	public async Task<OsuStatsDataTypes.OsuStatsRespektiveResponseData> GetRespektiveTopCounts(int osuId)
	{
		await Log.WriteVerbose("GetRespektiveTopCounts", $"Requesting osu!stats (respektive) data for osu! ID {osuId}.");

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			await Log.WriteVerbose("GetRespektiveTopCounts", "Requesting osu!stats (respektive) counts endpoint.");
			response = await client.GetAsync($"{OsuStatsRespektiveEndpoint}/counts/{osuId}");
		}
		catch (Exception e)
		{
			await Log.WriteError("GetRespektiveTopCounts", $"An unhandled error occurred while requesting osu!stats data. Exception details below.\n{e}");
			throw new ApiInstanceException("Unhandled exception.");
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			await Log.WriteWarning("GetRespektiveTopCounts", $"osu!stats (respektive) API returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuStatsDataTypes.OsuStatsRespektiveResponseData ret = JsonSerializer.Deserialize<OsuStatsDataTypes.OsuStatsRespektiveResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		await Log.WriteVerbose("GetRespektiveTopCounts", "osu!stats (respektive) user counts retrieved successfully. Returning data as OsuStatsRespektiveResponseData object.");
		return ret;
	}
}
