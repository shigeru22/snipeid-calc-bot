using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class CommandsFactory
{
	public static string GetPingMessage(DiscordSocketClient client)
	{
		Log.WriteVerbose("GetPingMessage", "Returning latency from client as ping message.");
		return $"Pong! ({ client.Latency } ms)";
	}

	public static Embed GetBotHelpMessage(DiscordSocketClient client, bool useInteraction = false)
	{
		string commandPrefix = !useInteraction ? $"<@{ client.CurrentUser.Id }> " : "**/**";

		return new EmbedBuilder().WithTitle("Help: Commands")
			.WithDescription($@"
				**Points calculation**
				Points could be calculated from Bathbot's `<osc` command, which this bot will follow up with calculated points and summary.
				This bot is able to calculate points directly using `count` command.

				{ commandPrefix }`link [osu! user ID]`
				Links your Discord user to an osu! user.

				{ commandPrefix }`count [osu! username (optional)]`
				Calculates points based on leaderboard count.
				If osu! username is omitted, will calculate points for your linked osu! user.

				{ commandPrefix }`whatif [what-if parameters]`
				Calculates what-if points based on leaderboard count. See help page for details.
				TL;DR, specify top counts in comma-delimited form. For example, `1=20,8=250`.
				For now, this command requires osu! user linking.
				**P.S.** Specifying osu! username WIP!

				{ commandPrefix }`leaderboard`
				{ commandPrefix }`lb`
				Returns server points leaderboard.

				{ commandPrefix }`help`
				Returns this help message.

				{ commandPrefix }`config`
				Server configuration commands. Only available for server administrators.
				See <@{ client.CurrentUser.Id }> `config help` or **/**`config help` for details.
			")
			.WithUrl("https://leaderpoints.kyutorius.com/help")
			.WithColor(238, 229, 229)
			.Build();
	}

	public static Embed GetConfigHelpMessage(DiscordSocketClient client, bool useInteraction = false)
	{
		string commandPrefix = !useInteraction ? $"<@{ client.CurrentUser.Id }> " : "**/**";
		
		return new EmbedBuilder().WithTitle("Help: Server Configuration")
			.WithDescription($@"
				**Note**
				Server configuration commands are only available for server administrators.
				Each setter commands have their own options, which could be entered to enable and omitted to disable.

				{ commandPrefix }`config show`
				Returns current server configuration.

				{ commandPrefix }`config help`
				Returns this help message.

				{ commandPrefix }`config set country [country code]`
				Sets country restriction for this server.
				Country code must be in 2-letter country code (Alpha-2) form. See https://www.iban.com/country-codes for list. For example, `ID`.

				{ commandPrefix }`config set verifiedrole [role]`
				Sets verified user role, which could be granted after linking or calculating points of linked users.
				{ (useInteraction
					? "Role could be specified by selecting from the command option list."
					: "Role could be entered by either mentioning the role or entering its ID directly.") }

				{ commandPrefix }`config set commandschannel [channel]`
				Sets server commands channel restriction.
				{ (useInteraction
					? "Channel could be specified by selecting from the command option list."
					: "Channel could be entered by either mentioning the channel or entering its ID directly.") }

				{ commandPrefix }`config set leaderboardschannel [channel]`
				Sets server leaderboard commands channel restriction.
				{ (useInteraction
					? "Channel could be specified by selecting from the command option list."
					: "Channel could be entered by either mentioning the channel or entering its ID directly.") }
			")
			.WithUrl("https://leaderpoints.kyutorius.com/help")
			.WithColor(238, 229, 229)
			.Build();
	}
}
