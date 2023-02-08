// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Interactions;
using Discord.Rest;
using LeaderpointsBot.Client.Commands;
using LeaderpointsBot.Client.Structures;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Modules;

public static class Context
{
	public class CountContextModule : InteractionModuleBase<SocketInteractionContext>
	{
		// user context -> Calculate points
		[EnabledInDm(true)]
		[UserCommand("Calculate points")]
		public async Task CountPointsCommand(IUser user)
		{
			Log.WriteInfo($"Calculating points for {user.Username}#{user.Discriminator}.");

			await Context.Interaction.DeferAsync();

			ReturnMessages[] responses = await Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString());

			RestInteractionMessage? replyMsg = null;
			foreach (ReturnMessages response in responses)
			{
				if (response.MessageType == Common.ResponseMessageType.Embed)
				{
					if (replyMsg == null)
					{
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg =>
						{
							msg.Content = string.Empty;
							msg.Embed = response.GetEmbed();
						});
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(embed: response.GetEmbed());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Text)
				{
					if (replyMsg == null)
					{
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = response.GetString());
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
				else if (response.MessageType == Common.ResponseMessageType.Error)
				{
					if (replyMsg == null)
					{
						replyMsg = await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Content = $"**Error:** {response.GetString()}");
					}
					else
					{
						_ = await replyMsg.Channel.SendMessageAsync(response.GetString());
					}
				}
			}
		}
	}
}