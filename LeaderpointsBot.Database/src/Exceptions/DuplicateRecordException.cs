namespace LeaderpointsBot.Database.Exceptions;

public class DuplicateRecordException : DatabaseException
{
	public DuplicateRecordException(): base("Duplicated record found with the specified query.") { }
	public DuplicateRecordException(string table): base($"Duplicated record found at { table } table.") { }
	public DuplicateRecordException(string table, string column): base($"Duplicated record found in { column } column at { table } table.") { }
	public DuplicateRecordException(string table, string column, string message): base($"Duplicated record found in { column } column at { table } table: { message }") { }
}
