// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Client.Exceptions;

public class ClientException : Exception
{
	public ClientException() : base("Client error occurred.") { }
	public ClientException(string message) : base($"Client error occurred: {message}") { }
}
