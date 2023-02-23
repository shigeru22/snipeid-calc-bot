// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Environments;

public class EnvironmentVariableValueException : UtilsException
{
	public EnvironmentVariableValueException(string envKey) : base($"Invalid {envKey} value.") { }
	public EnvironmentVariableValueException(string envKey, string description) : base($"Invalid {envKey} value, {description}") { }
}
