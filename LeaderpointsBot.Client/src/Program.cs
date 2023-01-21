using LeaderpointsBot.Api;
using LeaderpointsBot.Database;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public static class Program
{
	public static async Task Main(string[] args)
	{
		await Log.WriteInfo("Main", "Program started.");

		await Log.WriteVerbose("Main", "Setting up ApiFactory.");

		ApiFactory.Instance.OsuApiInstance.Token.ClientID = Settings.Instance.OsuApi.ClientID;
		ApiFactory.Instance.OsuApiInstance.Token.ClientSecret = Settings.Instance.OsuApi.ClientSecret;

		await Log.WriteVerbose("Main", "Setting up DatabaseFactory.");

		DatabaseFactory.Instance.SetConfig(new DatabaseConfig()
		{
			HostName = Settings.Instance.Database.HostName,
			Port = Settings.Instance.Database.Port,
			Username = Settings.Instance.Database.Username,
			Password = Settings.Instance.Database.Password,
			DatabaseName = Settings.Instance.Database.DatabaseName,
			CAFilePath = Settings.Instance.Database.CAFilePath,
		});

		await Log.WriteVerbose("Main", "Starting up client.");

		Client client = new(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
