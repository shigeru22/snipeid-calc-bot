// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Arguments;

public class ParameterAttributeException : UtilsException
{
	public ParameterAttributeException(string methodName)
		: base($"Parameter type not specified using ArgumentParameter attribute: {methodName}") { }
}
