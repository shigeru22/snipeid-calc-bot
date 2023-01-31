// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Exceptions.Actions;

public class SkipUpdateException : ClientException
{
	public SkipUpdateException() : base(ErrorMessages.ClientActionSkipUpdateError.Message) { }
}
