// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Net;

namespace LeaderpointsBot.Api.Exceptions;

public class ApiResponseException : ApiException
{
	private readonly HttpStatusCode code;

	public ApiResponseException(int statusCode) : base($"API client returned status code {statusCode}.")
	{
		code = (HttpStatusCode)statusCode;
	}

	public ApiResponseException(HttpStatusCode statusCode) : base($"API client returned status code {(int)statusCode}.")
	{
		code = statusCode;
	}

	public ApiResponseException(int statusCode, string message) : base($"API client returned status code {statusCode}: {message}")
	{
		code = (HttpStatusCode)statusCode;
	}

	public ApiResponseException(HttpStatusCode statusCode, string message) : base($"API client returned status code {(int)statusCode}: {message}")
	{
		code = statusCode;
	}

	public HttpStatusCode Code { get => code; }
}
