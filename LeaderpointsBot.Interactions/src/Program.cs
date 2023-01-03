using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public static class Program
{
	public static async Task Main(string[] args)
	{
		Client client = new(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
