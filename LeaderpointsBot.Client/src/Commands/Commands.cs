// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Embeds;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class CommandsFactory
{
	public static string GetPingMessage(DiscordSocketClient client)
	{
		Log.WriteVerbose("GetPingMessage", "Returning latency from client as ping message.");
		return $"Pong! ({client.Latency} ms)";
	}

	public static Embed GetBotHelpMessage(DiscordSocketClient client, bool useInteraction = false)
	{
		string commandPrefix = !useInteraction ? $"<@{client.CurrentUser.Id}> " : "**/**";

		return new EmbedBuilder().WithTitle("Help: Commands")
			.WithDescription(HelpMessages.GenerateCommandsHelpMessage(commandPrefix))
			.WithUrl("https://leaderpoints.kyutorius.com/help")
			.WithColor(Colors.Normal)
			.Build();
	}

	public static Embed GetConfigHelpMessage(DiscordSocketClient client, bool useInteraction = false)
	{
		string commandPrefix = !useInteraction ? $"<@{client.CurrentUser.Id}> " : "**/**";

		return new EmbedBuilder().WithTitle("Help: Server Configuration")
			.WithDescription(HelpMessages.GenerateConfigurationHelpMessage(commandPrefix, useInteraction))
			.WithUrl("https://leaderpoints.kyutorius.com/help")
			.WithColor(Colors.Normal)
			.Build();
	}

	private static class HelpMessages
	{
		public static readonly string CommandsPointsTitle = "**Points calculation**";
		public static readonly string CommandsPointsDescription = "Points could be calculated from Bathbot's `<osc` command, which this bot will follow up with calculated points and summary.\nThis bot is able to calculate points directly using `count` command.";
		public static readonly string CommandsLinkSyntax = "`link [osu! user ID]`";
		public static readonly string CommandsLinkDescription = "Links your Discord user to an osu! user.";
		public static readonly string CommandsCountSyntax = "`count [osu! username (optional)]`";
		public static readonly string CommandsCountDescription = "Calculates points based on leaderboard count.\nIf osu! username is omitted, will calculate points for your linked osu! user.";
		public static readonly string CommandsWhatifSyntax = "`whatif [what-if parameters]`";
		public static readonly string CommandsWhatifDescription = "Calculates what-if points based on leaderboard count. See help page for details.\nTL;DR, specify top counts in comma-delimited form. For example, `1=20,8=250`.\nFor now, this command requires osu! user linking.\n**P.S.** Specifying osu! username WIP!";
		public static readonly string CommandsLeaderboardSyntax = "`leaderboard`";
		public static readonly string CommandsLeaderboardSyntaxShort = "`lb`";
		public static readonly string CommandsLeaderboardDescription = "Returns server points leaderboard.";
		public static readonly string CommandsHelpSyntax = "`help`";
		public static readonly string CommandsHelpDescription = "Returns this help message.";
		public static readonly string CommandsConfigurationSyntax = "`config`";
		public static readonly string CommandsConfigurationDescription = $"Server configuration commands. Only available for server administrators.\nSee `config help` command for details.";

		public static readonly string ConfigurationNotesTitle = "**Notes**";
		public static readonly string ConfigurationNotesDescription = "Server configuration commands are only available for server administrators.\nEach setter commands have their own options, which could be entered to enable and omitted to disable.";
		public static readonly string ConfigurationShowSyntax = "`config show`";
		public static readonly string ConfigurationShowDescription = "Returns current server configuration.";
		public static readonly string ConfigurationHelpSyntax = "`config help`";
		public static readonly string ConfigurationHelpDescription = "Returns this help message.";
		public static readonly string ConfigurationCountrySyntax = "`config set country [country code]`";
		public static readonly string ConfigurationCountryDescription = "Sets country restriction for this server.\nCountry code must be in 2-letter country code (Alpha-2) form. See https://www.iban.com/country-codes for list. For example, `ID`.";
		public static readonly string ConfigurationRoleSyntax = "`config set verifiedrole [role]`";
		public static readonly string ConfigurationRoleDescription = "Sets verified user role, which could be granted after linking or calculating points of linked users.";
		public static readonly string ConfigurationRoleInteractionDetails = "Role could be specified by selecting from the command option list.";
		public static readonly string ConfigurationRoleMessageDetails = "Role could be entered by either mentioning the role or entering its ID directly.";
		public static readonly string ConfigurationCommandsSyntax = "`config set commandschannel [channel]`";
		public static readonly string ConfigurationCommandsDescription = "Sets server commands channel restriction.";
		public static readonly string ConfigurationCommandsInteractionDetails = "Channel could be specified by selecting from the command option list.";
		public static readonly string ConfigurationCommandsMessageDetails = "Channel could be entered by either mentioning the channel or entering its ID directly.";
		public static readonly string ConfigurationLeaderboardsSyntax = "`config set leaderboardschannel [channel]`";
		public static readonly string ConfigurationLeaderboardsDescription = "Sets server leaderboard commands channel restriction.";
		public static readonly string ConfigurationLeaderboardsInteractionDetails = "Channel could be specified by selecting from the command option list.";
		public static readonly string ConfigurationLeaderboardsMessageDetails = "Channel could be entered by either mentioning the channel or entering its ID directly.";

		public static string GenerateCommandsHelpMessage(string commandPrefix)
		{
			return $"{CommandsPointsTitle}\n" +
				$"{CommandsPointsDescription}\n\n" +
				$"{commandPrefix}`{CommandsLinkSyntax}`\n" +
				$"{CommandsLinkDescription}\n\n" +
				$"{commandPrefix}{CommandsCountSyntax}\n" +
				$"{CommandsCountDescription}\n\n" +
				$"{commandPrefix}{CommandsWhatifSyntax}\n" +
				$"{CommandsWhatifDescription}\n\n" +
				$"{commandPrefix}{CommandsLeaderboardSyntax}\n" +
				$"{commandPrefix}{CommandsLeaderboardSyntaxShort}\n" +
				$"{CommandsLeaderboardDescription}\n\n" +
				$"{commandPrefix}{CommandsHelpSyntax}\n" +
				$"{CommandsHelpDescription}\n\n" +
				$"{commandPrefix}{CommandsConfigurationSyntax}\n" +
				$"{CommandsConfigurationDescription}\n\n";
		}

		public static string GenerateConfigurationHelpMessage(string commandPrefix, bool useInteraction)
		{
			return $"{ConfigurationNotesTitle}\n" +
				$"{ConfigurationNotesDescription}\n\n" +
				$"{commandPrefix}{ConfigurationShowSyntax}\n" +
				$"{ConfigurationShowDescription}\n\n" +
				$"{commandPrefix}{ConfigurationHelpSyntax}\n" +
				$"{ConfigurationHelpDescription}\n\n" +
				$"{commandPrefix}{ConfigurationCountrySyntax}\n" +
				$"{ConfigurationCountryDescription}\n\n" +
				$"{commandPrefix}{ConfigurationRoleSyntax}\n" +
				$"{ConfigurationRoleDescription}\n" +
				$"{(useInteraction ? ConfigurationRoleInteractionDetails : ConfigurationRoleMessageDetails)}\n\n" +
				$"{commandPrefix}{ConfigurationCommandsSyntax}\n" +
				$"{ConfigurationCommandsDescription}\n" +
				$"{(useInteraction ? ConfigurationCommandsInteractionDetails : ConfigurationCommandsMessageDetails)}\n\n" +
				$"{commandPrefix}{ConfigurationLeaderboardsSyntax}\n" +
				$"{ConfigurationLeaderboardsDescription}\n" +
				$"{(useInteraction ? ConfigurationLeaderboardsInteractionDetails : ConfigurationLeaderboardsMessageDetails)}\n\n";
		}
	}
}
