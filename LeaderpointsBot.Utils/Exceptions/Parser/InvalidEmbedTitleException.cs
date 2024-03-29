// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Parser;

public class InvalidEmbedTitleException : UtilsException
{
	public InvalidEmbedTitleException() : base("Specified embed title is invalid.") { }
	public InvalidEmbedTitleException(string message) : base($"Specified embed title is invalid: {message}") { }
}
