namespace LeaderpointsBot.Database.Schemas;

public static class AssignmentsQuerySchema
{
	public enum AssignmentType
	{
		INSERT = 1,
		UPDATE
	}

	public struct AssignmentsTableData
	{
		public int AssignmentID { get; set; }
		public string Username { get; set; }
		public string RoleName { get; set; }
	}

	public struct AssignmentsResultData
	{
		public AssignmentType Type { get; set; }
		public string DiscordID { get; set; } // user Discord ID
		public RolesQuerySchema.RoleAssignmentData Role { get; set; }
		public int Delta { get; set; }
		public DateTime? LastUpdate { get; set; }
	}
}