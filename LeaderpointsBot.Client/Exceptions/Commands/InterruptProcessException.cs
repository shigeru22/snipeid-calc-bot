// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Exceptions.Commands;

public class InterruptProcessException : Exception
{
	public string Reason { get; init; }

	public InterruptProcessException(string reason) : base("Client interrupted the process.")
	{
		Reason = reason;
	}
}
