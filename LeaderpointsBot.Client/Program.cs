using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public class Program
{
	public static async Task Main(string[] args)
	{
		Client client = new Client(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
