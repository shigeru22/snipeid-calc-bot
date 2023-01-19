namespace LeaderpointsBot.Client.Exceptions;

public class ClientException : Exception
{
	public ClientException() : base("Client error occurred.") { }
	public ClientException(string message) : base($"Client error occurred: { message }") { }
}
