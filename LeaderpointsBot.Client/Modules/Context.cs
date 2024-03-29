// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.Interactions;
using LeaderpointsBot.Client.Actions;
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

			ReturnMessage[] responses = await Commands.Counter.CountLeaderboardPointsByDiscordUserAsync(Context.User.Id.ToString(), Context.Client.CurrentUser.Id.ToString());

			Log.WriteVerbose("Points calculated successfully. Sending responses.");
			await Reply.SendToInteractionContextAsync(Context, responses);
		}
	}
}