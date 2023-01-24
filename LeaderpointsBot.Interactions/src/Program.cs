// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public static class Program
{
	public static async Task Main(string[] args)
	{
		Client client = new Client(Settings.Instance.Client.BotToken);
		await client.Run();
	}
}
