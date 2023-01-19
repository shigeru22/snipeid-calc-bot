namespace LeaderpointsBot.Utils.Exceptions.Parser;

public class InvalidEmbedDescriptionException : UtilsException
{
	public InvalidEmbedDescriptionException() : base("Specified embed description is invalid.") { }
	public InvalidEmbedDescriptionException(string message) : base($"Specified embed description is invalid: { message }") { }
}
