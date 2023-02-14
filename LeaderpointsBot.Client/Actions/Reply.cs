// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class Reply
{
	public static async Task SendToCommandContextAsync(SocketCommandContext context, ReturnMessage response)
	{
		await SendToCommandContextAsync(context, response.Message, response.Embed, response.IsError);
	}

	public static async Task SendToCommandContextAsync(SocketCommandContext context, ReturnMessage[] responses)
	{
		foreach (ReturnMessage response in responses)
		{
			await SendToCommandContextAsync(context, response.Message, response.Embed, response.IsError);
		}
	}

	public static async Task SendToInteractionContextAsync(SocketInteractionContext context, ReturnMessage response, bool? modifyResponse = null)
	{
		RestInteractionMessage interactionResponse = await context.Interaction.GetOriginalResponseAsync();
		bool shouldModifyResponse = modifyResponse == null ? interactionResponse.Content.Equals(string.Empty) || interactionResponse.Embeds.Count <= 0 : modifyResponse.Value;

		await SendToInteractionContextAsync(context, response.Message, response.Embed, response.IsError, shouldModifyResponse);
	}

	public static async Task SendToInteractionContextAsync(SocketInteractionContext context, ReturnMessage[] responses, bool? modifyResponse = null)
	{
		RestInteractionMessage interactionResponse = await context.Interaction.GetOriginalResponseAsync();
		bool shouldModifyResponse = modifyResponse == null ? interactionResponse.Content.Equals(string.Empty) || interactionResponse.Embeds.Count <= 0 : modifyResponse.Value;

		foreach (ReturnMessage response in responses)
		{
			await SendToInteractionContextAsync(context, response.Message, response.Embed, response.IsError, shouldModifyResponse);
			modifyResponse = false;
		}
	}

	private static async Task SendToCommandContextAsync(SocketCommandContext context, string? replyMsg = null, Embed? replyEmbed = null, bool isError = false)
	{
		string? draft = string.IsNullOrEmpty(replyMsg) ? null : $"{(isError ? "**Error:** " : string.Empty)}{replyMsg}";

		if (Settings.Instance.Client.UseReply)
		{
			_ = await context.Message.ReplyAsync(text: draft, embed: replyEmbed);
		}
		else
		{
			_ = await context.Channel.SendMessageAsync(text: draft, embed: replyEmbed);
		}
	}

	private static async Task SendToInteractionContextAsync(SocketInteractionContext context, string? replyMsg = null, Embed? replyEmbed = null, bool isError = false, bool modifyResponse = false)
	{
		string? draft = string.IsNullOrEmpty(replyMsg) ? null : $"{(isError ? "**Error:** " : string.Empty)}{replyMsg}";

		if (modifyResponse)
		{
			_ = await context.Interaction.ModifyOriginalResponseAsync(msg =>
			{
				msg.Content = draft;
				msg.Embed = replyEmbed;
			});
		}
		else
		{
			_ = await context.Channel.SendMessageAsync(text: draft, embed: replyEmbed);
		}
	}
}
