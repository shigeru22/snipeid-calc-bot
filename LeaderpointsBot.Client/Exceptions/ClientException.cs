// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Exceptions;

public class ClientException : Exception
{
	public ClientException() : base(ErrorMessages.ClientError.Message) { }
	public ClientException(string message) : base(message) { }
}
