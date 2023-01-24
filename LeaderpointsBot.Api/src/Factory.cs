// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Api.OsuStats;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api;

public class ApiFactory
{
	private static readonly ApiFactory instance = new ApiFactory();

	private readonly OsuApi apiOsu;
	private readonly OsuStatsApi apiOsuStats;

	public static ApiFactory Instance => instance;
	public OsuApi OsuApiInstance => apiOsu;
	public OsuStatsApi OsuStatsInstance => apiOsuStats;

	private ApiFactory()
	{
		Log.WriteVerbose("ApiFactory", "ApiFactory instance created. Initializing wrapper instances.");

		apiOsu = new OsuApi();
		apiOsuStats = new OsuStatsApi();

		Log.WriteVerbose("ApiFactory", "API client wrapper instances created.");
	}
}
