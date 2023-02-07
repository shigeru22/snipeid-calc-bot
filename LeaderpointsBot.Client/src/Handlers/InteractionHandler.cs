// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Utils;
using LeaderpointsBot.Utils.Process;

namespace LeaderpointsBot.Client.Handlers;

public class InteractionHandler
{
	private readonly DiscordSocketClient client;
	private readonly InteractionService interactionService;

	public InteractionHandler(DiscordSocketClient client, InteractionService interactionService)
	{
		Log.WriteVerbose("InteractionsFactory instance created.");

		this.client = client;
		this.interactionService = interactionService;

		Log.WriteVerbose("Instance client set.");
	}

	public async Task InitializeServiceAsync()
	{
		Log.WriteVerbose("Registering entry assembly as interaction service module.");
		_ = await interactionService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
	}

	public async Task OnInvokeInteraction(SocketInteraction cmd)
	{
		static async Task SendResponse(SocketInteraction cmd, string message)
		{
			if (cmd.HasResponded)
			{
				_ = await cmd.ModifyOriginalResponseAsync(res => res.Content = message);
			}
			else
			{
				await cmd.RespondAsync(message);
			}
		}

		// Log.WriteDebug("OnInvokeSlashInteraction", $"Slash interaction from { cmd.User.Username }#{ cmd.User.Discriminator }: { cmd.Data.Name } ({ cmd.Data.Id })");

		SocketInteractionContext context = new SocketInteractionContext(client, cmd);

		IResult result = await interactionService.ExecuteCommandAsync(context, null);

		if (result.Error != InteractionCommandError.Exception)
		{
			// command processing complete
			return;
		}

		if (result is ExecuteResult execResult)
		{
			Exception e = execResult.Exception;

			if (e is SendMessageException ex)
			{
				await SendResponse(cmd, $"{(ex.IsError ? "**Error:** " : string.Empty)}{ex.Draft})");
				return;
			}

			Log.WriteError(Log.GenerateExceptionMessage(e, ErrorMessages.ClientError.Message));
			await SendResponse(cmd, "**Error:** Unhandled client error occurred.");
		}
	}
}
