// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Exceptions.Commands;

public class SendMessageException : ClientException
{
	private readonly bool isError;
	private readonly string draft;

	public SendMessageException() : base("Client interrupted, but no message needed to be sent.")
	{
		isError = false;
		draft = string.Empty;
	}

	public SendMessageException(string message, bool isError = false) : base("Client interrupted and needs to send message to sender.")
	{
		this.isError = isError;
		draft = message;
	}

	public bool IsError { get => isError; }
	public string Draft { get => draft; }
}
