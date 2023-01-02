namespace LeaderpointsBot.Database.Exceptions;

public class DataNotFoundException : DatabaseException
{
	public DataNotFoundException(): base("Data with specified query not found in database.") { }
	public DataNotFoundException(string message): base($"Data with specified query not found in database: { message }") { }
}
