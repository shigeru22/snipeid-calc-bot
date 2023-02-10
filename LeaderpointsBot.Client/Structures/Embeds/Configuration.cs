// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Structures.Embeds;

public static class Configuration
{
	public readonly struct ServerConfigurations
	{
		public string GuildName { get; init; }
		public string? GuildIconURL { get; init; }
		public string? CountryCode { get; init; }
		public string? VerifiedRoleName { get; init; }
		public string? CommandsChannelName { get; init; }
		public string? LeaderboardsChannelName { get; init; }
	}
}
