// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Api.Exceptions;

public class ApiException : Exception
{
	public ApiException() : base("API error occurred.") { }

	public ApiException(string message) : base($"API error occurred: {message}") { }
}
