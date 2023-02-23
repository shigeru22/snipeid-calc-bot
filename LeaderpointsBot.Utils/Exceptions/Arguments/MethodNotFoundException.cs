// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Arguments;

public class MethodNotFoundException : UtilsException
{
	public MethodNotFoundException(string attributeName) : base($"Method with {attributeName} attribute not found.") { }
}
