namespace LeaderpointsBot.Utils.Exceptions;

public class UtilsException : Exception
{
	public UtilsException(): base() { }
	public UtilsException(string message): base(message) { }
}