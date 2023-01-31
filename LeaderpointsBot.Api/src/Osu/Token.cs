// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Api.Osu;

public class OsuToken
{
	public const string OSU_TOKEN_ENDPOINT = "https://osu.ppy.sh/oauth/token";

	private int clientId = 0;
	private string clientSecret = string.Empty;
	private string? token = null;
	private DateTime expirationTime = DateTime.UtcNow; // use UTC for accurate results

	public OsuToken()
	{
		Log.WriteVerbose("osu!api token wrapper class instantiated.");
	}

	public OsuToken(int clientId, string clientSecret)
	{
		Log.WriteVerbose("osu!api token wrapper class instantiated.");

		ClientID = clientId;
		ClientSecret = clientSecret;
	}

	public int ClientID
	{
		get => clientId;
		set
		{
			clientId = value;
			Log.WriteVerbose("osu!api client ID value changed.");
		}
	}

	public string ClientSecret
	{
		get => clientSecret;
		set
		{
			clientSecret = value;
			Log.WriteVerbose("osu!api client secret value changed.");
		}
	}

	public async Task<string> GetTokenAsync()
	{
		Log.WriteVerbose("Retrieving osu!api client token.");

		if (token != null && DateTime.UtcNow < expirationTime)
		{
			Log.WriteVerbose("Non-expired token found. Returning token.");
			return token;
		}

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			OsuDataTypes.OsuApiTokenRequestData requestData = new OsuDataTypes.OsuApiTokenRequestData()
			{
				ClientID = clientId,
				ClientSecret = clientSecret,
				GrantType = "client_credentials",
				Scope = "public"
			};

			StringContent data = new StringContent(JsonSerializer.Serialize(requestData.ToRawData()), Encoding.UTF8, "application/json");

			Log.WriteVerbose("Requesting osu!api token endpoint.");
			response = await client.PostAsync(OSU_TOKEN_ENDPOINT, data);
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.OsuApiTokenRequestError.Message));
			throw new ApiInstanceException(ErrorMessages.OsuApiTokenRequestError.Message);
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Log.WriteWarning($"osu!api returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuDataTypes.OsuApiTokenResponseData content = JsonSerializer.Deserialize<OsuDataTypes.OsuApiTokenResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		token = content.AccessToken;
		expirationTime = DateTime.UtcNow.AddSeconds(content.ExpiresIn);

		Log.WriteVerbose("osu!api token requested successfully. Returning token.");
		return token;
	}

	public async Task RevokeTokenAsync()
	{
		Log.WriteVerbose("Revoking current osu!api client token.");

		if (token == null || DateTime.UtcNow >= expirationTime)
		{
			Log.WriteError(ErrorMessages.OsuApiTokenRevokedError.Message);
			throw new InvalidDataException("osu!api token either already revoked or expired.");
		}

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

			Log.WriteVerbose("Requesting osu!api revoke token endpoint.");
			response = await client.DeleteAsync($"{OsuApi.OSU_API_ENDPOINT}/oauth/tokens/current");
		}
		catch (Exception e)
		{
			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.OsuApiTokenRevokationError.Message));
			throw new ApiInstanceException(ErrorMessages.OsuApiTokenRevokationError.Message);
		}

		// if not 204
		if (response.StatusCode != HttpStatusCode.NoContent)
		{
			Log.WriteWarning($"osu!api returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		token = null;
		expirationTime = DateTime.UtcNow;

		Log.WriteVerbose("osu!api token revoked successfully.");
	}
}
