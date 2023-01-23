// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Exceptions.Actions;

public class SkipUpdateException : ClientException
{
	public SkipUpdateException() : base("Process asked to skip data update.") { }
}
