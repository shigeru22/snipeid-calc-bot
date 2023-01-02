namespace LeaderpointsBot.Database.Exceptions;

public class DatabaseInstanceException : DatabaseException
{
	public DatabaseInstanceException(): base("Database instance error occurred.") { }
	public DatabaseInstanceException(string message): base($"Database instance error occurred: { message }") { }
}
