// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Arguments;

[AttributeUsage(AttributeTargets.Method)]
public class ArgumentAttribute : Attribute
{
	public string? ShortFlag { get; init; }
	public string? LongFlag { get; init; }

	public ArgumentAttribute(string? shortFlag)
	{
		ShortFlag = shortFlag;
		LongFlag = null;
	}

	public ArgumentAttribute(string? shortFlag, string? longFlag)
	{
		ShortFlag = shortFlag;
		LongFlag = longFlag;
	}
}
