namespace LeaderpointsBot.Api.Exceptions;

public class ApiInstanceException : ApiException
{
	public ApiInstanceException(): base("API client instance error occurred.") { }
	public ApiInstanceException(string message): base($"API client instance error occurred: { message }") { }
}
