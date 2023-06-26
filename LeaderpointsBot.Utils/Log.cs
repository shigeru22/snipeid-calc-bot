// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Diagnostics;
using System.Reflection;
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
					WriteError(msg.Message, msg.Source);
					break;
				case Discord.LogSeverity.Warning:
					WriteWarning(msg.Message, msg.Source);
					break;
				case Discord.LogSeverity.Info:
					WriteInfo(msg.Message, msg.Source);
					break;
				case Discord.LogSeverity.Verbose:
					WriteVerbose(msg.Message, msg.Source);
					break;
				case Discord.LogSeverity.Debug:
					WriteDebug(msg.Message, msg.Source);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(msg));
			}
		}
	}

	public static void Write(LogSeverity severity, string message, [CallerMemberName] string source = "")
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

	public static Task WriteAsync(LogSeverity severity, string message, [CallerMemberName] string source = "")
	{
		Write(severity, source, message);
		return Task.CompletedTask;
	}

	public static void WriteCritical(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= 0)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;

			Console.Error.WriteLine(GenerateLogMessage(Discord.LogSeverity.Critical, message, source));

			Console.ForegroundColor = currentColor;
		}
	}

	public static void WriteError(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= 1)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;

			Console.Error.WriteLine(GenerateLogMessage(Discord.LogSeverity.Error, message, source));

			Console.ForegroundColor = currentColor;
		}
	}

	public static void WriteWarning(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= 2)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.WriteLine(GenerateLogMessage(Discord.LogSeverity.Warning, message, source));

			Console.ForegroundColor = currentColor;
		}
	}

	public static void WriteInfo(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= 3)
		{
			Console.WriteLine(GenerateLogMessage(Discord.LogSeverity.Info, message, source));
		}
	}

	public static void WriteVerbose(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.IsVerboseOrDebug())
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGray;

			Console.WriteLine(GenerateLogMessage(Discord.LogSeverity.Verbose, message, source));

			Console.ForegroundColor = currentColor;
		}
	}

	public static void WriteDebug(string message, [CallerMemberName] string source = "")
	{
		if (Settings.Instance.Client.Logging.LogSeverity >= 5)
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGray;

			Console.WriteLine(GenerateLogMessage(Discord.LogSeverity.Debug, message, source));

			Console.ForegroundColor = currentColor;
		}
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

	public static string GenerateLogMessage(LogSeverity severity, string message, string source)
	{
		string tempSource = source.Equals(".ctor") || source.Equals(".cctor") ? GetParentTypeName() : source;
		return $"{Date.GetCurrentDateTime(Settings.Instance.Client.Logging.UseUTC)} :: {LogSeverity[(int)severity][0]} :: {tempSource} :: {message}";
	}

	public static string GenerateExceptionMessage(Exception e, string errorMessage)
	{
		return $"{errorMessage}{(Settings.Instance.Client.Logging.IsVerboseOrDebug() ? $". Exception details below.\n{e}" : string.Empty)}";
	}

	private static string GetParentTypeName()
	{
		StackFrame stackFrame = new StackFrame(3);
		MethodBase? methodBase = stackFrame.GetMethod();

		if (methodBase != null && methodBase.DeclaringType != null)
		{
			return methodBase.DeclaringType.Name;
		}

		return "(unknown)";
	}
}
