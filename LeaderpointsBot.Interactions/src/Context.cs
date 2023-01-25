// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Interactions;

public static class ContextCommandsFactory
{
	public static UserCommandBuilder[] UserContextCommands { get; } =
	{
		new UserCommandBuilder().WithName("Calculate points")
			.WithDMPermission(false)
	};

	public static async Task CreateUserContextCommands(DiscordSocketClient client)
	{
		Log.WriteVerbose("CreateUserContextCommands", "Iterating user context commands array.");

		int userContextCommandsCount = UserContextCommands.Length;
		for (int i = 0; i < userContextCommandsCount; i++)
		{
			Log.WriteInfo("CreateUserContextCommands", $"Creating user context commands ({i + 1}/{userContextCommandsCount})...");

			Log.WriteVerbose("CreateUserContextCommands", $"Creating command (index {i}) on client.");
			await client.CreateGlobalApplicationCommandAsync(UserContextCommands[i].Build());

			if (i < userContextCommandsCount - 1)
			{
				// logging level is not verbose or debug
				if (Settings.Instance.Client.Logging.LogSeverity < 4)
				{
					Log.DeletePreviousLine();
				}
			}
			else
			{
				Log.WriteInfo("CreateUserContextCommands", "Context commands created.");
			}
		}
	}
}
