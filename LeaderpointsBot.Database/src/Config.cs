namespace LeaderpointsBot.Database;

public struct DatabaseConfig
{
	public string HostName { get; set; }
	public int Port { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string DatabaseName { get; set; }
	public string? CAFilePath { get; set; }
}
