// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Utils;

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
		Log.WriteVerbose("OsuToken", "osu!api token wrapper class instantiated.");
	}

	public OsuToken(int clientId, string clientSecret)
	{
		Log.WriteVerbose("OsuToken", "osu!api token wrapper class instantiated.");

		ClientID = clientId;
		ClientSecret = clientSecret;
	}

	public int ClientID
	{
		get => clientId;
		set
		{
			clientId = value;
			Log.WriteVerbose("ClientID", "osu!api client ID value changed.");
		}
	}

	public string ClientSecret
	{
		get => clientSecret;
		set
		{
			clientSecret = value;
			Log.WriteVerbose("ClientSecret", "osu!api client secret value changed.");
		}
	}

	public async Task<string> GetTokenAsync()
	{
		Log.WriteVerbose("GetTokenAsync", "Retrieving osu!api client token.");

		if (token != null && DateTime.UtcNow < expirationTime)
		{
			Log.WriteVerbose("GetTokenAsync", "Non-expired token found. Returning token.");
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

			Log.WriteVerbose("GetTokenAsync", "Requesting osu!api token endpoint.");
			response = await client.PostAsync(OSU_TOKEN_ENDPOINT, data);
		}
		catch (Exception e)
		{
			Log.WriteError("GetTokenAsync", $"An unhandled error occurred while requesting token. Exception details below.\n{e}");
			throw new ApiInstanceException("Unhandled exception.");
		}

		// if not 200
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Log.WriteWarning("GetTokenAsync", $"osu!api returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuDataTypes.OsuApiTokenResponseData content = JsonSerializer.Deserialize<OsuDataTypes.OsuApiTokenResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		token = content.AccessToken;
		expirationTime = DateTime.UtcNow.AddSeconds(content.ExpiresIn);

		Log.WriteVerbose("GetTokenAsync", "osu!api token requested successfully. Returning token.");
		return token;
	}

	public async Task RevokeTokenAsync()
	{
		Log.WriteVerbose("RevokeTokenAsync", "Revoking current osu!api client token.");

		if (token == null || DateTime.UtcNow >= expirationTime)
		{
			Log.WriteError("RevokeTokenAsync", "Token either already revoked or expired.");
			throw new InvalidDataException("osu!api token either already revoked or expired.");
		}

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

			Log.WriteVerbose("RevokeTokenAsync", "Requesting osu!api revoke token endpoint.");
			response = await client.DeleteAsync($"{OsuApi.OSU_API_ENDPOINT}/oauth/tokens/current");
		}
		catch (Exception e)
		{
			Log.WriteError("RevokeTokenAsync", $"An unhandled error occurred while revoking token. Exception details below.\n{e}");
			throw new ApiInstanceException("Unhandled exception.");
		}

		// if not 204
		if (response.StatusCode != HttpStatusCode.NoContent)
		{
			Log.WriteWarning("RevokeTokenAsync", $"osu!api returned status code {(int)response.StatusCode}");
			throw new ApiResponseException(response.StatusCode);
		}

		token = null;
		expirationTime = DateTime.UtcNow;

		Log.WriteVerbose("RevokeTokenAsync", "osu!api token revoked successfully.");
	}
}
