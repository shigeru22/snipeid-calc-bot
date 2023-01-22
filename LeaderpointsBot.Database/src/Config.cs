namespace LeaderpointsBot.Database;

public struct DatabaseConfig
{
	public string HostName { get; set; }
	public int Port { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string DatabaseName { get; set; }
	public string? CAFilePath { get; set; }

	public DatabaseConfig(string hostName, int port, string username, string password, string databaseName, string? caFilePath)
    {
    	HostName = hostName;
    	Port = port;
    	Username = username;
    	Password = password;
    	DatabaseName = databaseName;
    	CAFilePath = caFilePath;
    }

	public string ToConnectionString()
	{
		string connectionString = $"Host={ HostName };Port={ Port };Username={ Username };Password={ Password };Database={ DatabaseName }";
		if(!string.IsNullOrEmpty(CAFilePath))
		{
			connectionString += $";SSL Certificate={ Path.GetFullPath(CAFilePath) }";
		}

		return connectionString;
	}
}
