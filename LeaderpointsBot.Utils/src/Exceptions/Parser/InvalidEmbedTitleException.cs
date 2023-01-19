namespace LeaderpointsBot.Utils.Exceptions.Parser;

public class InvalidEmbedTitleException : UtilsException
{
	public InvalidEmbedTitleException() : base("Specified embed title is invalid.") { }
	public InvalidEmbedTitleException(string message) : base($"Specified embed title is invalid: { message }") { }
}
