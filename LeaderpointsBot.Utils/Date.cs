// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils;

public static class Date
{
	private static readonly DateTime zeroTime = new DateTime(1, 1, 1);

	public static string GetCurrentDate(bool useUtc = true)
	{
		DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
		return $"{currentTime.Year}/{currentTime.Month.ToString().PadLeft(2, '0')}/{currentTime.Day.ToString().PadLeft(2, '0')}";
	}

	public static string GetCurrentTime(bool useUtc = true)
	{
		DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
		return $"{currentTime.Hour.ToString().PadLeft(2, '0')}:{currentTime.Minute.ToString().PadLeft(2, '0')}/{currentTime.Second.ToString().PadLeft(2, '0')}";
	}

	public static string GetCurrentDateTime(bool useUtc = true)
	{
		DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
		return $"{currentTime.Year}/{currentTime.Month.ToString().PadLeft(2, '0')}/{currentTime.Day.ToString().PadLeft(2, '0')} {currentTime.Hour.ToString().PadLeft(2, '0')}:{currentTime.Minute.ToString().PadLeft(2, '0')}:{currentTime.Second.ToString().PadLeft(2, '0')}";
	}

	public static string DateTimeToString(DateTime time, bool useUtc = true)
	{
		DateTime tempTime = useUtc ? time.ToUniversalTime() : time;

		// yyyy/mm/dd, hh:mm
		return $"{tempTime.Month}/{tempTime.Day}/{tempTime.Year}, {tempTime.Hour.ToString().PadLeft(2, '0')}:{tempTime.Minute.ToString().PadLeft(2, '0')} ({(useUtc ? "UTC" : "local time")})";
	}

	public static string DeltaTimeToString(TimeSpan deltaTime)
	{
		// days
		int absTimeValue = Math.Abs(deltaTime.Days);
		if (absTimeValue >= 1)
		{
			return $"{absTimeValue} day{(absTimeValue != 1 ? "s" : string.Empty)}";
		}

		// hours
		absTimeValue = Math.Abs(deltaTime.Hours);
		if (absTimeValue >= 1)
		{
			return $"{absTimeValue - 1} hour{(absTimeValue - 1 != 1 ? "s" : string.Empty)}";
		}

		// minutes
		absTimeValue = Math.Abs(deltaTime.Seconds);
		if (absTimeValue >= 1)
		{
			return $"{absTimeValue - 1} minute{(absTimeValue - 1 != 1 ? "s" : string.Empty)}";
		}

		return "<1 minute";
	}
}
