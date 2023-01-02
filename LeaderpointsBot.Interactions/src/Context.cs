using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public static class ContextCommandsFactory
{
	private static UserCommandBuilder[] userContextCommands = {
		new UserCommandBuilder().WithName("Calculate points")
			.WithDMPermission(false)
	};

	public static UserCommandBuilder[] UserContextCommands { get => userContextCommands; }

	public static async Task CreateUserContextCommands(DiscordSocketClient client)
	{
		await Log.WriteVerbose("CreateUserContextCommands", "Iterating user context commands array.");

		int userContextCommandsCount = userContextCommands.Length;
		for(int i = 0; i < userContextCommandsCount; i++)
		{
			await Log.WriteInfo("CreateUserContextCommands", $"Creating user context commands ({ i + 1 }/{ userContextCommandsCount })...");

			await Log.WriteVerbose("CreateUserContextCommands", $"Creating command (index { i }) on client.");
			await client.CreateGlobalApplicationCommandAsync(userContextCommands[i].Build());

			if(i < userContextCommandsCount - 1)
			{
				if(Settings.Instance.Client.Logging.LogSeverity < 4) // logging level below debug
				{
					await Log.DeletePreviousLine();
				}
			}
			else
			{
				await Log.WriteInfo("CreateUserContextCommands", "Context commands created.");
			}
		}
	}
}