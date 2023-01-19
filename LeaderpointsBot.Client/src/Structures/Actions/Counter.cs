namespace LeaderpointsBot.Client.Structures.Actions;

public static class Counter
{
	public struct UpdateUserDataMessages
	{
		public string PointsMessage { get; init; }
		public string? RoleMessage { get; init; }
	}
}
