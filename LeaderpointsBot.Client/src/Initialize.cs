// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.Interactions;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client;

public static class Initialize
{
	public static async Task CreateInteractionsAsync(InteractionService interactionService)
	{
		DateTime startTime = DateTime.Now;

		Log.WriteInfo("Start initializing bot interaction commands.");

		try
		{
			var temp = interactionService.SlashCommands;
			_ = await interactionService.RegisterCommandsGloballyAsync();
		}
		catch (Exception e)
		{
			// TODO: determine application command creation errors

			Log.WriteCritical($"Unhandled error occurred while creating command. Exception details below.\n{e}", "OnReady");

			Log.WriteVerbose("Exiting with code 1.");
			Environment.Exit(1);
		}

		DateTime endTime = DateTime.Now;

		Log.WriteInfo($"Operation completed in {Math.Round((endTime - startTime).TotalSeconds, 3)} seconds.");
	}

	public static Task CreateDatabaseAsync()
	{
		throw new NotImplementedException();
	}
}
