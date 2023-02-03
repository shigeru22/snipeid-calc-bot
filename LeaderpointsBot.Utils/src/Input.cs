// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Text;

namespace LeaderpointsBot.Utils;

public static class Input
{
	public static string ReadHiddenLine()
	{
		StringBuilder input = new StringBuilder();

		bool endInput = false;
		while (!endInput)
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			switch (key.Key)
			{
				case ConsoleKey.Backspace:
					if (input.Length > 0)
					{
						_ = input.Remove(input.Length - 1, 1);
					}
					break;
				case ConsoleKey.Enter:
					Console.WriteLine();
					endInput = true;
					break;
				default:
					_ = input.Append(key.KeyChar);
					break;
			}
		}

		return input.ToString();
	}
}
