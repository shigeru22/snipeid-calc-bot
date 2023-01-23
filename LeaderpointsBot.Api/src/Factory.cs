// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using LeaderpointsBot.Api.Osu;
using LeaderpointsBot.Api.OsuStats;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Api;

public class ApiFactory
{
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:StaticReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "Private static readonly instance names should be lowercased (styling not yet configurable).")]
	private static readonly ApiFactory instance = new ApiFactory();

	private readonly OsuApi apiOsu;
	private readonly OsuStatsApi apiOsuStats;

	private ApiFactory()
	{
		Log.WriteVerbose("ApiFactory", "ApiFactory instance created. Initializing wrapper instances.");

		apiOsu = new OsuApi();
		apiOsuStats = new OsuStatsApi();

		Log.WriteVerbose("ApiFactory", "API client wrapper instances created.");
	}

	public static ApiFactory Instance { get => instance; }

	public OsuApi OsuApiInstance { get => apiOsu; }

	public OsuStatsApi OsuStatsInstance { get => apiOsuStats; }
}
