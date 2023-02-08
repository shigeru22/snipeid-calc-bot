// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;

namespace LeaderpointsBot.Client.Structures;

public readonly struct ReturnMessages
{
	public Common.ResponseMessageType MessageType { get; init; }
	public object Contents { get; init; }

	public string GetString() => (string)Contents;
	public Embed GetEmbed() => (Embed)Contents;
}
