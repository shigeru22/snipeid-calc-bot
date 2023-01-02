namespace LeaderpointsBot.Database.Schemas;

public static class UsersQuerySchema
{
	public struct UsersTableData
	{
		public int UserID { get; set; }
		public string DiscordID { get; set; }
		public int OsuID { get; set; }
		public string Country { get; set; }
		public int Points { get; set; }
		public DateTime LastUpdate { get; set; }
	}

	public struct UsersLeaderboardData
	{
		public int UserID { get; set; }
		public string Username { get; set; }
		public int Points { get; set; }
	}
}
