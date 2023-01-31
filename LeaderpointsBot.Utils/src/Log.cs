// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Runtime.CompilerServices;
using Discord;

namespace LeaderpointsBot.Utils;

public static class Log
{
	public static readonly string[] LogSeverity =
	{
		"CRITICAL",
		"ERROR",
		"WARNING",
		"INFO",
		"VERBOSE",
		"DEBUG"
	};

	public static void Write(LogMessage msg)
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= (int)msg.Severity)
		{
			switch (msg.Severity)
			{
				case Discord.LogSeverity.Critical:
					WriteCritical(msg.Message, msg.Source);
					break;
				case Discord.LogSeverity.Error:
					WriteError(msg.Message);
					break;
				case Discord.LogSeverity.Warning:
					WriteWarning(msg.Message);
					break;
				case Discord.LogSeverity.Info:
					WriteInfo(msg.Message);
					break;
				case Discord.LogSeverity.Verbose:
					WriteVerbose(msg.Message);
					break;
				case Discord.LogSeverity.Debug:
					WriteDebug(msg.Message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(msg));
			}
		}
	}

	public static void Write(LogSeverity severity, string source, string message)
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= (int)severity)
		{
			switch (severity)
			{
				case Discord.LogSeverity.Critical:
					WriteCritical(message, source);
					break;
				case Discord.LogSeverity.Error:
					WriteError(message);
					break;
				case Discord.LogSeverity.Warning:
					WriteWarning(message);
					break;
				case Discord.LogSeverity.Info:
					WriteInfo(message);
					break;
				case Discord.LogSeverity.Verbose:
					WriteVerbose(message);
					break;
				case Discord.LogSeverity.Debug:
					WriteDebug(message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
			}
		}
	}

	public static Task WriteAsync(LogMessage msg)
	{
		Write(msg);
		return Task.CompletedTask;
	}

	public static Task WriteAsync(LogSeverity severity, string source, string message)
	{
		Write(severity, source, message);
		return Task.CompletedTask;
	}

	public static void WriteCritical(string source, string message)
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;

		if (Settings.Instance.Client.Logging.LogSeverity >= 0)
		{
			Console.Error.WriteLine($"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[0][0]} :: {source} :: {message}");
		}

		Console.ForegroundColor = currentColor;
	}

	public static void WriteError(string message, [CallerMemberName] string source = "")
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;

		if (Settings.Instance.Client.Logging.LogSeverity >= 1)
		{
			Console.Error.WriteLine($"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[1][0]} :: {source} :: {message}");
		}

		Console.ForegroundColor = currentColor;
	}

	public static void WriteWarning(string message, [CallerMemberName] string source = "")
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;

		if (Settings.Instance.Client.Logging.LogSeverity >= 2)
		{
			Console.WriteLine($"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[2][0]} :: {source} :: {message}");
		}

		Console.ForegroundColor = currentColor;
	}

	public static void WriteInfo(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= 3)
		{
			Console.WriteLine($"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[3][0]} :: {source} :: {message}");
		}
	}

	public static void WriteVerbose(string message, [CallerMemberName] string source = "")
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.DarkGray;

		if (Settings.Instance.Client.Logging.IsVerboseOrDebug())
		{
			Console.WriteLine($"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[4][0]} :: {source} :: {message}");
		}

		Console.ForegroundColor = currentColor;
	}

	public static void WriteDebug(string message, [CallerMemberName] string source = "")
	{
		ConsoleColor currentColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.DarkGray;

		if (Settings.Instance.Client.Logging.LogSeverity >= 5)
		{
			Console.WriteLine($"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[5][0]} :: {source} :: {message}");
		}

		Console.ForegroundColor = currentColor;
	}

	public static void DeletePreviousLine(bool keepCurrentLine = false)
	{
		int currentCursorLine = Console.CursorTop;
		Console.SetCursorPosition(0, currentCursorLine - 1);
		Console.Write(new string(' ', Console.WindowWidth));

		if (!keepCurrentLine)
		{
			Console.SetCursorPosition(0, currentCursorLine - 1);
		}
	}

	public static string GenerateExceptionMessage(Exception e, string errorMessage)
	{
		return $"{errorMessage}{(Settings.Instance.Client.Logging.IsVerboseOrDebug() ? $". Exception details below.\n{e}" : string.Empty)}";
	}
}
