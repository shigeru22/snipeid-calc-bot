// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Structures.Actions;

public static class Counter
{
	public readonly struct UpdateUserDataMessages
	{
		public string PointsMessage { get; init; }
		public string? RoleMessage { get; init; }
	}
}
