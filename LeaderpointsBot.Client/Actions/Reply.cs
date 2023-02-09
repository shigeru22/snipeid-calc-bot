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
	public static async Task SendToCommandContextAsync(SocketCommandContext context, string replyMsg, bool isError = false)
	{
		string draft = $"{(isError ? "**Error:** " : string.Empty)}{replyMsg}";

		if (Settings.Instance.Client.UseReply)
		{
			_ = await context.Message.ReplyAsync(draft);
		}
		else
		{
			_ = await context.Channel.SendMessageAsync(draft);
		}
	}

	public static async Task SendToCommandContextAsync(SocketCommandContext context, Embed replyEmbed)
	{
		if (Settings.Instance.Client.UseReply)
		{
			_ = await context.Message.ReplyAsync(embed: replyEmbed);
		}
		else
		{
			_ = await context.Channel.SendMessageAsync(embed: replyEmbed);
		}
	}

	public static async Task SendToCommandContextAsync(SocketCommandContext context, ReturnMessages[] responses)
	{
		foreach (ReturnMessages response in responses)
		{
			switch (response.MessageType)
			{
				case Common.ResponseMessageType.Embed:
					await SendToCommandContextAsync(context, response.GetEmbed());
					break;
				case Common.ResponseMessageType.Text:
					await SendToCommandContextAsync(context, response.GetString());
					break;
				case Common.ResponseMessageType.Error:
					await SendToCommandContextAsync(context, response.GetString(), true);
					break;
				default:
					throw new NotImplementedException("MessageType not implemented in condition block.");
			}
		}
	}

	public static async Task SendToInteractionContextAsync(SocketInteractionContext context, string replyMsg, bool isError = false, bool modifyResponse = false)
	{
		string draft = $"{(isError ? "**Error:** " : string.Empty)}{replyMsg}";

		if (modifyResponse)
		{
			_ = await context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = draft);
		}
		else
		{
			_ = await context.Channel.SendMessageAsync(draft);
		}
	}

	public static async Task SendToInteractionContextAsync(SocketInteractionContext context, Embed replyEmbed, bool modifyResponse = false)
	{
		if (modifyResponse)
		{
			_ = await context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = replyEmbed);
		}
		else
		{
			_ = await context.Channel.SendMessageAsync(embed: replyEmbed);
		}
	}

	public static async Task SendToInteractionContextAsync(SocketInteractionContext context, ReturnMessages[] responses)
	{
		RestInteractionMessage interactionResponse = await context.Interaction.GetOriginalResponseAsync();
		bool modifyResponse = interactionResponse.Content.Equals(string.Empty) || interactionResponse.Embeds.Count <= 0;

		foreach (ReturnMessages response in responses)
		{
			switch (response.MessageType)
			{
				case Common.ResponseMessageType.Embed:
					await SendToInteractionContextAsync(context, response.GetEmbed(), modifyResponse);
					break;
				case Common.ResponseMessageType.Text:
					await SendToInteractionContextAsync(context, response.GetString(), false, modifyResponse);
					break;
				case Common.ResponseMessageType.Error:
					await SendToInteractionContextAsync(context, response.GetString(), true, modifyResponse);
					break;
				default:
					throw new NotImplementedException("MessageType not implemented in condition block.");
			}

			modifyResponse = false;
		}
	}
}
