using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api;

public class ApiFactory
{
	private static readonly ApiFactory instance = new();

	public static ApiFactory Instance { get => instance; }

	private readonly OsuApi apiOsu;

	public OsuApi OsuApiInstance
	{
		get => apiOsu;
	}

	private ApiFactory()
	{
		Log.WriteVerbose("ApiFactory", "ApiFactory instance created. Initializing wrapper instances.");

		apiOsu = new OsuApi();

		Log.WriteVerbose("ApiFactory", "API client wrapper instances created.");
	}
}
