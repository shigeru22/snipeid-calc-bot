// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using System.Text.Json;

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

		methods = null; // free memory since no longer used

		if (Settings.Instance.shouldPromptPassword)
		{
			Console.Write($"Enter password for {Settings.Instance.database.Username}: ");
			string temp = Input.ReadHiddenLine();
			Settings.Instance.database.Password = temp;
		}
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
