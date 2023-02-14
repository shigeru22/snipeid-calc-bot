// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;

namespace LeaderpointsBot.Client.Structures;

public readonly struct ReturnMessage
{
	public bool IsError { get; init; }

	public string? Message { get; init; }
	public Embed? Embed { get; init; }
}
