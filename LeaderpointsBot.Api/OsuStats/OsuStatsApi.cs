// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;
using System.Text;
using System.Text.Json;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Api.OsuStats;

public class OsuStatsApi
{
	public const string OSU_STATS_ENDPOINT = "https://osustats.ppy.sh/api";
	public const string OSU_STATS_RESPEKTIVE_ENDPOINT = "https://osustats.respektive.pw";

	public OsuStatsApi()
	{
		Log.WriteVerbose("osu!stats API wrapper class instantiated.");
	}

	public async Task<OsuStatsDataTypes.OsuStatsResponseData> GetTopCounts(string osuUsername, int maxRank)
	{
		Log.WriteVerbose($"Requesting osu!stats data for {osuUsername} (maxRank = {maxRank}).");

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

			Log.WriteVerbose("Requesting osu!stats getScores endpoint.");
			response = await client.PostAsync($"{OSU_STATS_ENDPOINT}/getScores", data);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.OsuStatsDataRequestError.Message));
			throw new ApiInstanceException(ErrorMessages.OsuStatsDataRequestError.Message);
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Log.WriteWarning($"osu!stats API returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		JsonElement[]? content = JsonSerializer.Deserialize<JsonElement[]>(await response.Content.ReadAsStringAsync());

		if (content == null)
		{
			Log.WriteError(ErrorMessages.OsuStatsDataNullError.Message);
			throw new ApiInstanceException(ErrorMessages.OsuStatsDataNullError.Message);
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

		Log.WriteVerbose("osu!stats user count retrieved successfully. Returning data as OsuStatsResponseData object.");
		return new OsuStatsDataTypes.OsuStatsResponseData()
		{
			Username = osuUsername,
			MaxRank = maxRank,
			Count = content[1].GetInt32() // integer boxed as object
		};
	}

	public async Task<OsuStatsDataTypes.OsuStatsRespektiveResponseData> GetRespektiveTopCounts(int osuId)
	{
		Log.WriteVerbose($"Requesting osu!stats (respektive) data for osu! ID {osuId}.");

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			Log.WriteVerbose("Requesting osu!stats (respektive) counts endpoint.");
			response = await client.GetAsync($"{OSU_STATS_RESPEKTIVE_ENDPOINT}/counts/{osuId}");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.OsuStatsDataRequestError.Message));
			throw new ApiInstanceException(ErrorMessages.OsuStatsDataRequestError.Message);
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Log.WriteWarning($"osu!stats (respektive) API returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuStatsDataTypes.OsuStatsRespektiveResponseData ret = JsonSerializer.Deserialize<OsuStatsDataTypes.OsuStatsRespektiveResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		Log.WriteVerbose("osu!stats (respektive) user counts retrieved successfully. Returning data as OsuStatsRespektiveResponseData object.");
		return ret;
	}
}
