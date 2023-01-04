using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Api.OsuStats;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api;

public class ApiFactory
{
	private static readonly ApiFactory instance = new();

	public static ApiFactory Instance { get => instance; }

	private readonly OsuApi apiOsu;
	private readonly OsuStatsApi apiOsuStats;

	public OsuApi OsuApiInstance
	{
		get => apiOsu;
	}

	public OsuStatsApi OsuStatsInstance
	{
		get => apiOsuStats;
	}

	private ApiFactory()
	{
		Log.WriteVerbose("ApiFactory", "ApiFactory instance created. Initializing wrapper instances.");

		apiOsu = new OsuApi();
		apiOsuStats = new OsuStatsApi();

		Log.WriteVerbose("ApiFactory", "API client wrapper instances created.");
	}
}
