using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public static class Program
{
	public static async Task Main(string[] args)
	{
		Client client = new(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
