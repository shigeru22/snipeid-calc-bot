namespace LeaderpointsBot.Client.Exceptions.Commands;

public class SendMessageException : ClientException
{
	public bool IsError { get; }
	public string Draft { get; }

	public SendMessageException() : base("Client interrupted, but no message needed to be sent.")
	{
		IsError = false;
		Draft = "";
	}

	public SendMessageException(string message, bool isError = false) : base("Client interrupted and needs to send message to sender.")
	{
		IsError = isError;
		Draft = message;
	}
}
