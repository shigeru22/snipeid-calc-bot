namespace LeaderpointsBot.Client.Structures.Actions;

public static class UserData
{
	public struct AssignmentResultRoleData
	{
		public string RoleDiscordID { get; init; }
		public string RoleName { get; init; }
	}

	public struct AssignmentResult
	{
		public AssignmentResultRoleData? OldRole { get; init; }
		public AssignmentResultRoleData NewRole { get; init; }
		public string UserDiscordID { get; init; }
		public int DeltaPoints { get; init; }
		public DateTime? LastUpdate { get; init; }
	}
}
