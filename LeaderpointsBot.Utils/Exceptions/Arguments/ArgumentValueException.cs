// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Arguments;

public class ArgumentValueException : UtilsException
{
	public ArgumentValueException(string argName) : base($"Invalid {argName} value.") { }
	public ArgumentValueException(string argName, string description) : base($"Invalid {argName} value, {description}") { }
}
