// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Api.Osu;

public class OsuApi
{
	public const string OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";

	private readonly OsuToken osuTokenInstance;

	public OsuToken Token => osuTokenInstance;

	public OsuApi()
	{
		Log.WriteVerbose("osu!api wrapper class instantiated.");

		osuTokenInstance = new OsuToken();
	}

	public OsuApi(OsuToken token)
	{
		Log.WriteVerbose("osu!api wrapper class instantiated.");

		osuTokenInstance = token;
	}

	public OsuApi(int clientId, string clientSecret)
	{
		Log.WriteVerbose("osu!api wrapper class instantiated.");

		osuTokenInstance = new OsuToken(clientId, clientSecret);
	}

	public async Task<OsuDataTypes.OsuApiUserResponseData> GetUserByOsuID(int osuId)
	{
		Log.WriteVerbose($"Requesting osu! user with ID {osuId}.");

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await Token.GetTokenAsync()}");

			Log.WriteVerbose("Requesting osu!api user endpoint.");
			response = await client.GetAsync($"{OsuApi.OSU_API_ENDPOINT}/users/{osuId}?type=id");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.OsuApiDataRequestError.Message));
			throw new ApiInstanceException(ErrorMessages.OsuApiDataRequestError.Message);
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Log.WriteWarning($"osu!api returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuDataTypes.OsuApiUserResponseData ret = JsonSerializer.Deserialize<OsuDataTypes.OsuApiUserResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		Log.WriteVerbose("osu! user retrieved successfully. Returning data as OsuApiUserResponseData object.");
		return ret;
	}

	public async Task<OsuDataTypes.OsuApiUserResponseData> GetUserByOsuUsername(string osuUsername)
	{
		Log.WriteVerbose($"Requesting osu! user with username {osuUsername}.");

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await Token.GetTokenAsync()}");

			Log.WriteVerbose("Requesting osu!api user endpoint.");
			response = await client.GetAsync($"{OsuApi.OSU_API_ENDPOINT}/users/{osuUsername}?key=username");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.OsuApiDataRequestError.Message));
			throw new ApiInstanceException(ErrorMessages.OsuApiDataRequestError.Message);
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Log.WriteWarning($"osu!api returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuDataTypes.OsuApiUserResponseData ret = JsonSerializer.Deserialize<OsuDataTypes.OsuApiUserResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		Log.WriteVerbose("osu! user retrieved successfully. Returning data as OsuApiUserResponseData object.");
		return ret;
	}
}
