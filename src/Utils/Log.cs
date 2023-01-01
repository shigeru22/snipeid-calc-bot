using Discord;

namespace LeaderpointsBot.Utils;

public static class Log
{
	private static string[] logSeverity = {
		"CRITICAL",
		"ERROR",
		"WARNING",
		"INFO",
		"VERBOSE",
		"DEBUG"
	};

	public static Task Write(LogMessage msg)
	{
		if(Settings.Instance.Client.LogSeverity >= (int)msg.Severity)
		{
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[((int)msg.Severity)][0] } :: { msg.Source } :: { msg.Message }");
		}

		return Task.CompletedTask;
	}

	public static Task Write(LogSeverity severity, string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= (int)severity)
		{
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[((int)severity)][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteCritical(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 0) {
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[0][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteError(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 1) {
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[1][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteWarning(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 2) {
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[2][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteInfo(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 3) {
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[3][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteVerbose(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 4) {
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[4][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteDebug(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 5) {
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[5][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}
}