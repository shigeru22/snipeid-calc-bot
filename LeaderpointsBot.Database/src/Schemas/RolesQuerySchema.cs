namespace LeaderpointsBot.Database.Schemas;

public static class RolesQuerySchema
{
	public struct RolesTableData
	{
		public int RoleID { get; set; }
		public string DiscordID { get; set; }
		public string RoleName { get; set; }
		public int MinPoints { get; set; }
	}

	public struct RoleAssignmentData
	{
		public string? OldRoleID { get; set; }
		public string? OldRoleName { get; set; }
		public string NewRoleID { get; set; }
		public string NewRoleName { get; set; }
	}
}
