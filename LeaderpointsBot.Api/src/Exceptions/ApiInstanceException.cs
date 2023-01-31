// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Api.Exceptions;

public class ApiInstanceException : ApiException
{
	public ApiInstanceException() : base(ErrorMessages.ApiInstanceError.Message) { }
	public ApiInstanceException(string message) : base(message) { }
}
