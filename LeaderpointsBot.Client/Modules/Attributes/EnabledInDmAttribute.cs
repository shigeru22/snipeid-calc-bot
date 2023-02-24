// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.Commands;
using Discord.WebSocket;

namespace LeaderpointsBot.Client.Modules.Attributes;

public class EnabledInDmAttribute : PreconditionAttribute
{
	private readonly bool isEnabled;

	public EnabledInDmAttribute(bool enable = true) => isEnabled = enable;

	public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
	{
		if (!isEnabled && context.Channel is SocketDMChannel)
		{
			return Task.FromResult(PreconditionResult.FromError("This command is only available on servers."));
		}

		return Task.FromResult(PreconditionResult.FromSuccess());
	}
}
