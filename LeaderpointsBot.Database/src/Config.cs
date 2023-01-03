namespace LeaderpointsBot.Database;

public struct DatabaseConfig
{
	public string HostName { get; }
	public int Port { get; }
	public string Username { get; }
	public string Password { get; }
	public string DatabaseName { get; }
	public string? CAFilePath { get; }

	public DatabaseConfig(string hostName, int port, string username, string password, string databaseName, string? caFilePath)
    {
    	HostName = hostName;
    	Port = port;
    	Username = username;
    	Password = password;
    	DatabaseName = databaseName;
    	CAFilePath = caFilePath;
    }
}
