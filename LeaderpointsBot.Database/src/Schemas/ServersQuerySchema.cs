namespace LeaderpointsBot.Database.Schemas;

public static class ServersQuerySchema
{
	public struct ServersTableData
	{
		public int ServerID { get; set; }
		public string DiscordID { get; set; }
		public string? Country { get; set; }
		public string? VerifyChannelID { get; set; }
		public string? VerifiedRoleID { get; set; }
		public string? CommandsChannelID { get; set; }
		public string? LeaderboardsChannelID { get; set; }
	}
}