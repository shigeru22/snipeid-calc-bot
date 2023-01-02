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
			switch(msg.Severity)
			{
				case LogSeverity.Critical:
					Log.WriteCritical(msg.Source, msg.Message);
					break;
				case LogSeverity.Error:
					Log.WriteError(msg.Source, msg.Message);
					break;
				case LogSeverity.Warning:
					Log.WriteWarning(msg.Source, msg.Message);
					break;
				case LogSeverity.Info:
					Log.WriteInfo(msg.Source, msg.Message);
					break;
				case LogSeverity.Verbose:
					Log.WriteVerbose(msg.Source, msg.Message);
					break;
				case LogSeverity.Debug:
					Log.WriteDebug(msg.Source, msg.Message);
					break;
			}
		}

		return Task.CompletedTask;
	}

	public static Task Write(LogSeverity severity, string source, string message)
	{
		string output = $"{ Date.GetCurrentDateTime() } :: { logSeverity[((int)severity)][0] } :: { source } :: { message }";
		int severitySetting = Settings.Instance.Client.LogSeverity;

		if(severitySetting >= (int)severity)
		{
			if(severity >= LogSeverity.Warning) // warning and above
			{
				Console.WriteLine(output);
			}
			else
			{
				Console.Error.WriteLine(output);
			}
		}

		return Task.CompletedTask;
	}

	public static Task WriteCritical(string source, string message)
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;

		if(Settings.Instance.Client.LogSeverity >= 0)
		{
			Console.Error.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[0][0] } :: { source } :: { message }");
		}

		Console.ForegroundColor = currentColor;

		return Task.CompletedTask;
	}

	public static Task WriteError(string source, string message)
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;

		if(Settings.Instance.Client.LogSeverity >= 1)
		{
			Console.Error.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[1][0] } :: { source } :: { message }");
		}

		Console.ForegroundColor = currentColor;

		return Task.CompletedTask;
	}

	public static Task WriteWarning(string source, string message)
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;

		if(Settings.Instance.Client.LogSeverity >= 2)
		{
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[2][0] } :: { source } :: { message }");
		}

		Console.ForegroundColor = currentColor;

		return Task.CompletedTask;
	}

	public static Task WriteInfo(string source, string message)
	{
		if(Settings.Instance.Client.LogSeverity >= 3)
		{
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[3][0] } :: { source } :: { message }");
		}

		return Task.CompletedTask;
	}

	public static Task WriteVerbose(string source, string message)
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.DarkGray;

		if(Settings.Instance.Client.LogSeverity >= 4)
		{
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[4][0] } :: { source } :: { message }");
		}

		Console.ForegroundColor = currentColor;

		return Task.CompletedTask;
	}

	public static Task WriteDebug(string source, string message)
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.DarkGray;

		if(Settings.Instance.Client.LogSeverity >= 5)
		{
			Console.WriteLine($"{ Date.GetCurrentDateTime() } :: { logSeverity[5][0] } :: { source } :: { message }");
		}

		Console.ForegroundColor = currentColor;

		return Task.CompletedTask;
	}

	public static Task DeletePreviousLine(bool keepCurrentLine = false)
	{
		int currentCursorLine = Console.CursorTop;
		Console.SetCursorPosition(0, currentCursorLine - 1);
		Console.Write(new string(' ', Console.WindowWidth));

		if(!keepCurrentLine)
		{
			Console.SetCursorPosition(0, currentCursorLine - 1);
		}

		return Task.CompletedTask;
	}
}