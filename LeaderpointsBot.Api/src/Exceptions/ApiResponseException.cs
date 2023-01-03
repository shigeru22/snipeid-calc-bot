using System.Net;

namespace LeaderpointsBot.Api.Exceptions;

public class ApiResponseException : ApiException
{
	public HttpStatusCode Code { get; private set; }

	public ApiResponseException(int statusCode): base($"API client returned status code { statusCode }.")
	{
		Code = (HttpStatusCode)statusCode;
	}

	public ApiResponseException(HttpStatusCode statusCode): base($"API client returned status code { (int)statusCode }.")
	{
		Code = statusCode;
	}

	public ApiResponseException(int statusCode, string message): base($"API client returned status code { statusCode }: { message }")
	{
		Code = (HttpStatusCode)statusCode;
	}

	public ApiResponseException(HttpStatusCode statusCode, string message): base($"API client returned status code { (int)statusCode }: { message }")
	{
		Code = statusCode;
	}
}
