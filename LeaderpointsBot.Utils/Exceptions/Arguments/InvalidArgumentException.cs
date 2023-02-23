// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Arguments;

public class InvalidArgumentException : UtilsException
{
	public string ArgumentName { get; init; }

	public InvalidArgumentException(string argName) : base($"Invalid program argument: {argName}")
	{
		ArgumentName = argName;
	}
}
