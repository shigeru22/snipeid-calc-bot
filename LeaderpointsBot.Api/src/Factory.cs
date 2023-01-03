using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api;

public class ApiFactory
{
	private static readonly ApiFactory instance = new();

	public static ApiFactory Instance { get => instance; }

	private OsuApi apiOsu;

	public OsuApi OsuApiInstance
	{
		get => apiOsu;
	}

	private ApiFactory()
	{
		Log.WriteVerbose("ApiFactory", "ApiFactory instance created. Initializing wrapper instances.");

		apiOsu = new();

		Log.WriteVerbose("ApiFactory", "API client wrapper instances created.");
	}
}
