// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

namespace LeaderpointsBot.Api.Exceptions;

public class ApiInstanceException : ApiException
{
	public ApiInstanceException() : base("API client instance error occurred.") { }
	public ApiInstanceException(string message) : base($"API client instance error occurred: {message}") { }
}
