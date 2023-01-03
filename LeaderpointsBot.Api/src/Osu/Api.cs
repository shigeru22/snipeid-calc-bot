using System.Net;
using System.Text.Json;
using LeaderpointsBot.Api.Exceptions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api.Osu;

public class OsuApi
{
	public const string OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";

	public OsuToken Token { get; }

	public OsuApi()
	{
		Log.WriteVerbose("OsuApi", "osu!api wrapper class instantiated.");

		Token = new OsuToken();
	}

	public OsuApi(OsuToken token)
	{
		Log.WriteVerbose("OsuApi", "osu!api wrapper class instantiated.");

		Token = token;
	}

	public OsuApi(int clientId, string clientSecret)
	{
		Log.WriteVerbose("OsuApi", "osu!api wrapper class instantiated.");

		Token = new OsuToken(clientId, clientSecret);
	}

	public async Task<OsuDataTypes.OsuApiUserResponseData> GetUserByOsuID(int osuId)
	{
		await Log.WriteVerbose("GetUserByOsuID", $"Requesting osu! user with ID { osuId }.");

		HttpResponseMessage response;

		try
		{
			using HttpClient client = new();

			client.DefaultRequestHeaders.Accept.Add(new("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer { await Token.GetTokenAsync() }");

			await Log.WriteVerbose("GetUserByOsuID", "Requesting osu!api user endpoint.");
			response = await client.GetAsync($"{ OsuApi.OSU_API_ENDPOINT }/users/{ osuId }");
		}
		catch (Exception e)
		{
			await Log.WriteError("GetUserByOsuID", $"An unhandled error occurred while revoking token. Exception details below.\n{ e }");
			throw new ApiInstanceException("Unhandled exception.");
		}

		if(response.StatusCode != HttpStatusCode.OK) // not 200
		{
			await Log.WriteWarning("GetUserByOsuID", $"osu!api returned status code { (int)response.StatusCode }");
			throw new ApiResponseException(response.StatusCode);
		}

		OsuDataTypes.OsuApiUserResponseData ret = JsonSerializer.Deserialize<OsuDataTypes.OsuApiUserResponseRawData>(await response.Content.ReadAsStringAsync()).ToStandardData();

		await Log.WriteVerbose("GetUserByOsuID", "osu! user retrieved successfully. Returning data as OsuApiUserResponseData object.");
		return ret;
	}
}
