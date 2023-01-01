namespace LeaderpointsBot;

using LeaderpointsBot.Utils;

public class Program
{
	public static async Task Main(string[] args)
	{
		Client client = new Client(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
