// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Utils.Exceptions.Parser;

public class InvalidEmbedDescriptionException : UtilsException
{
	public InvalidEmbedDescriptionException() : base("Specified embed description is invalid.") { }
	public InvalidEmbedDescriptionException(string message) : base($"Specified embed description is invalid: {message}") { }
}
