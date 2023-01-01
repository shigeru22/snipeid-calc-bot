namespace LeaderpointsBot.Utils;

public static class Date
{
	public static string GetCurrentDate(bool useUtc = true)
	{
		DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
		return $"{ currentTime.Year }/{ currentTime.Month.ToString().PadLeft(2, '0') }/{ currentTime.Day.ToString().PadLeft(2, '0') }";
	}

	public static string GetCurrentTime(bool useUtc = true)
	{
		DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
		return $"{ currentTime.Hour.ToString().PadLeft(2, '0') }:{ currentTime.Minute.ToString().PadLeft(2, '0') }/{ currentTime.Second.ToString().PadLeft(2, '0') }";
	}

	public static string GetCurrentDateTime(bool useUtc = true)
	{
		DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
		return $"{ currentTime.Year }/{ currentTime.Month.ToString().PadLeft(2, '0') }/{ currentTime.Day.ToString().PadLeft(2, '0') } { currentTime.Hour.ToString().PadLeft(2, '0') }:{ currentTime.Minute.ToString().PadLeft(2, '0') }:{ currentTime.Second.ToString().PadLeft(2, '0') }";
	}
}