// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Exceptions.Commands;

public class SendMessageException : ClientException
{
	public bool IsError { get; init; }
	public string Draft { get; init; }

	public SendMessageException() : base(ErrorMessages.ClientCommandInterruptError.Message)
	{
		IsError = false;
		Draft = string.Empty;
	}

	public SendMessageException(string message, bool isError = false) : base(ErrorMessages.ClientCommandSendMessageError.Message)
	{
		IsError = isError;
		Draft = message;
	}
}
