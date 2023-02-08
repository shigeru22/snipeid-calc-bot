// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Exceptions.Embeds;

public class InvalidEmbedDescriptionException : ClientException
{
	public InvalidEmbedDescriptionException() : base(ErrorMessages.ClientEmbedDescriptionError.Message) { }
}
