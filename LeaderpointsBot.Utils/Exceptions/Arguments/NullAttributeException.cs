// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Arguments;

public class NullAttributeException : UtilsException
{
	public NullAttributeException(string attributeName) : base($"{attributeName} attribute not found in method.") { }
}
