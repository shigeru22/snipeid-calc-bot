// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils.Process;
using System.Net;

namespace LeaderpointsBot.Api.Exceptions;

public class ApiResponseException : ApiException
{
	public HttpStatusCode Code { get; init; }

	public ApiResponseException(int statusCode) : base(ErrorMessages.ApiResponseError.Message)
	{
		Code = (HttpStatusCode)statusCode;
	}

	public ApiResponseException(HttpStatusCode statusCode) : base(ErrorMessages.ApiResponseError.Message)
	{
		Code = statusCode;
	}

	public ApiResponseException(int statusCode, string message) : base(message)
	{
		Code = (HttpStatusCode)statusCode;
	}

	public ApiResponseException(HttpStatusCode statusCode, string message) : base(message)
	{
		Code = statusCode;
	}
}
