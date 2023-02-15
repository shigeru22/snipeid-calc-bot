// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using System.Text;

namespace LeaderpointsBot.Utils.Arguments;

public static class ArgumentHandler
{
	private static MethodInfo[]? methods = null;

	public static void HandleArguments(string[] args) // TODO: create tests
	{
		methods = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(identifier => identifier.GetTypes())
			.Where(type => type.IsClass && type.Name.Equals(nameof(Settings)))
			.SelectMany(classItem => classItem.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			.Where(method => method.GetCustomAttribute(typeof(ArgumentAttribute)) != null)
			.ToArray();

		if (methods == null || methods.Length <= 0)
		{
			// nothing to process
			methods = null;
			return;
		}

		int argsLength = args.Length;
		for (int i = 0; i < argsLength; i++)
		{
			if (args[i].StartsWith("--"))
			{
				HandleArgument(ref i, true, args[i][2..], GetArgumentElement(args, i + 1));
			}
			else if (args[i].StartsWith("-"))
			{
				HandleArgument(ref i, false, args[i][1..], GetArgumentElement(args, i + 1));
			}
			else
			{
				throw new ArgumentException($"Invalid program argument: {args[i]}");
			}
		}

		Settings.Instance.PostArgumentHandling();

		methods = null; // free memory since no longer used
	}

	internal static void PrintHelpMessage()
	{
		if (methods == null || methods.Length <= 0)
		{
			// TODO: create better exception
			throw new NullReferenceException("Methods with Argument attribute not found.");
		}

		int minShortWidth = 2;
		int minLongWidth = 2;

		List<string[]> outputRows = new List<string[]>();
		foreach (MethodInfo method in methods)
		{
			ArgumentAttribute? argAttr = (ArgumentAttribute?)method.GetCustomAttribute(typeof(ArgumentAttribute));
			DescriptionAttribute? descAttr = (DescriptionAttribute?)method.GetCustomAttribute(typeof(DescriptionAttribute));

			if (argAttr == null)
			{
				throw new ArgumentException("Invalid method processed.");
			}

			if (descAttr == null)
			{
				throw new ArgumentException("Methods with argument attribute should implement Description attribute.");
			}

			if (argAttr.ShortFlag != null && minShortWidth < (argAttr.ShortFlag.Length + 1))
			{
				minShortWidth = argAttr.ShortFlag.Length + 1;
			}

			if (argAttr.LongFlag != null && minLongWidth < (argAttr.LongFlag.Length + 3))
			{
				minLongWidth = argAttr.LongFlag.Length + 3;
			}

			if (method.GetParameters().Length == 1 && argAttr.LongFlag != null && minLongWidth < (argAttr.LongFlag.Length + 8))
			{
				minLongWidth = argAttr.LongFlag.Length + 10;
			}

			outputRows.Add(new string[] {
				$"-{argAttr.ShortFlag ?? string.Empty}",
				$"--{argAttr.LongFlag ?? string.Empty} {(method.GetParameters().Length == 1 ? "[value]" : string.Empty)}",
				descAttr.Description
			});
		}

		StringBuilder helpMessage = new StringBuilder();

		_ = helpMessage.Append("Usage:\n");
		_ = helpMessage.Append("  LeaderpointsBot.Client [options]\n\n");
		_ = helpMessage.Append("Options:\n");
		_ = helpMessage.Append("Note that each options could be specified for overriding settings without the need for configuration file.\n");
		_ = helpMessage.Append("However, if any option is not specified, an error message will be shown and exit.\n");

		foreach (string[] outputColumns in outputRows)
		{
			_ = helpMessage.Append($"  ");
			_ = helpMessage.Append($"{outputColumns[0].PadRight(minShortWidth, ' ')}  ");
			_ = helpMessage.Append($"{outputColumns[1].PadRight(minLongWidth, ' ')}  ");
			_ = helpMessage.Append($"{outputColumns[2]}\n");
		}

		_ = helpMessage.Append('\n');

		Console.WriteLine(helpMessage);

		methods = null;
		Environment.Exit(0);
	}

	private static void HandleArgument(ref int currentIndex, bool isLongArgument, string key, string? value)
	{
		if (methods == null)
		{
			// nothing to process
			return;
		}

		foreach (MethodInfo method in methods)
		{
			ArgumentAttribute? attr = (ArgumentAttribute?)method.GetCustomAttribute(typeof(ArgumentAttribute), false);
			bool isCurrentMethod = isLongArgument ? (attr?.LongFlag != null && attr.LongFlag.Equals(key)) : (attr?.ShortFlag != null && attr.ShortFlag.Equals(key));

			if (method.DeclaringType != null && isCurrentMethod)
			{
				if (method.GetParameters().Length == 1)
				{
					if (value == null)
					{
						throw new ArgumentException("This argument method requires a parameter value.");
					}

					ParameterInfo[] parameters = method.GetParameters()
						.Where(param => param.GetCustomAttribute(typeof(ArgumentParameterAttribute)) != null)
						.ToArray();

					if (parameters.Length <= 0)
					{
						throw new ArgumentException("Parameter type not specified using ArgumentParameter attribute.");
					}

					object tempValue = Convert.ChangeType(value, parameters[0].ParameterType);
					_ = method.Invoke(Settings.Instance, new object[] { tempValue });

					currentIndex++;
				}
				else
				{
					_ = method.Invoke(Settings.Instance, null);
				}

				break;
			}
		}
	}

	private static string? GetArgumentElement(string[] arr, int index)
	{
		try
		{
			if (string.IsNullOrEmpty(arr[index]) || arr[index].StartsWith('-'))
			{
				return null;
			}

			return arr[index];
		}
		catch (IndexOutOfRangeException)
		{
			return null;
		}
	}

	private static T? ParseArgumentValue<T>(string argumentValue)
	{
		return (T)Convert.ChangeType(argumentValue, typeof(T));
	}
}
