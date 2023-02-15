// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Arguments;

[AttributeUsage(AttributeTargets.Method)]
public class DescriptionAttribute : Attribute
{
	public string Description { get; init; }
	public bool IsFlag { get; init; }

	public DescriptionAttribute(string description, bool isFlag = false)
	{
		if (string.IsNullOrWhiteSpace(description))
		{
			throw new InvalidOperationException("Argument description should not be empty.");
		}

		Description = description;
		IsFlag = isFlag;
	}
}
