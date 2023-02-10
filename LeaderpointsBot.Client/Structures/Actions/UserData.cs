// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Structures.Actions;

public static class UserData
{
	public readonly struct AssignmentResultRoleData
	{
		public string RoleDiscordID { get; init; }
		public string RoleName { get; init; }
	}

	public readonly struct AssignmentResult
	{
		public AssignmentResultRoleData? OldRole { get; init; }
		public AssignmentResultRoleData NewRole { get; init; }
		public string UserDiscordID { get; init; }
		public int DeltaPoints { get; init; }
		public DateTime? LastUpdate { get; init; }
	}
}
