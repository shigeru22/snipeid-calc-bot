// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Arguments;

public class ArgumentParameterException : UtilsException
{
	public ArgumentParameterException(bool containsParameter)
		: base($"This parameter {(containsParameter ? "requires a" : "does not require")} parameter value.") { }
}
