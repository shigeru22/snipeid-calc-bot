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

	public static Embed GetBotHelpMessage(DiscordSocketClient client)
	{
		ulong clientId = client.CurrentUser.Id;

		return new EmbedBuilder().WithTitle("Help: Commands")
			.WithDescription($@"
				**Points calculation**
				Points could be calculated from Bathbot's `<osc` command, which this bot will follow up with calculated points and summary.
				This bot is able to calculate points directly using `count` command.

				<@{ clientId }> `link [osu! user ID]`
				Links your Discord user to an osu! user.

				<@{ clientId }> `count [osu! username (optional)]`
				Calculates points based on leaderboard count.
				If osu! username is omitted, will calculate points for your linked osu! user.

				<@{ clientId }> `whatif [what-if parameters]`
				Calculates what-if points based on leaderboard count. See help page for details.
				TL;DR, specify top counts in comma-delimited form. For example, `1=20,8=250`.
				For now, this command requires osu! user linking.
				**P.S.** Specifying osu! username WIP!

				<@{ clientId }> `leaderboard`
				<@{ clientId }> `lb`
				Returns server points leaderboard.

				<@{ clientId }> `help`
				Returns this help message.

				<@{ clientId }> `config`
				Server configuration commands. Only available for server administrators.
				See <@{ clientId }> `config help` or `/config help` for details.
			")
			.WithUrl("https://leaderpoints.kyutorius.com/help")
			.WithColor(238, 229, 229)
			.Build();
	}
}
