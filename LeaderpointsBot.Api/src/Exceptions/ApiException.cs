namespace LeaderpointsBot.Api.Exceptions;

public class ApiException : Exception
{
	public ApiException(): base("API error occurred.") { }
	public ApiException(string message): base($"API error occurred: { message }") { }
}
