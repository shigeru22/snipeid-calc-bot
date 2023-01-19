namespace LeaderpointsBot.Client.Exceptions.Embeds;

public class InvalidEmbedDescriptionException : ClientException
{
	public InvalidEmbedDescriptionException() : base("Specified title is invalid.") { }
}
