namespace LeaderpointsBot.Database.Exceptions;

public class DatabaseException : Exception
{
	public DatabaseException() : base("Database error occurred.") { }
	public DatabaseException(string message) : base($"Database error occurred: { message }") { }
}
