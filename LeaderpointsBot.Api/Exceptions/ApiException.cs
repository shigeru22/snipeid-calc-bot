// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Api.Exceptions;

public class ApiException : Exception
{
	public ApiException() : base(ErrorMessages.ApiError.Message) { }
	public ApiException(string message) : base(message) { }
}
