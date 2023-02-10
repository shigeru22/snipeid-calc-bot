// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord;
using Discord.WebSocket;
using LeaderpointsBot.Client.Exceptions.Commands;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class Configuration
{
	public static async Task<Embed> GetGuildConfigurationAsync(SocketGuild guild)
	{
		Log.WriteVerbose($"Fetching server in database (guild ID {guild.Id}).");

		ServersQuerySchema.ServersTableData guildData;

		try
		{
			guildData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			Log.WriteError($"Server with Discord ID {guild.Id} not found in database.");
			throw new SendMessageException("Server not found in our end!", true);
		}

		Structures.Embeds.Configuration.ServerConfigurations configData = new Structures.Embeds.Configuration.ServerConfigurations()
		{
			GuildName = guild.Name,
			GuildIconURL = guild.IconUrl,
			CountryCode = guildData.Country,
			VerifiedRoleName = string.IsNullOrWhiteSpace(guildData.VerifiedRoleID) ? null : guild.GetRole(ulong.Parse(guildData.VerifiedRoleID)).Name,
			CommandsChannelName = string.IsNullOrWhiteSpace(guildData.CommandsChannelID) ? null : guild.GetTextChannel(ulong.Parse(guildData.CommandsChannelID)).Name,
			LeaderboardsChannelName = string.IsNullOrWhiteSpace(guildData.LeaderboardsChannelID) ? null : guild.GetTextChannel(ulong.Parse(guildData.LeaderboardsChannelID)).Name
		};

		return Embeds.Configuration.CreateServerConfigurationEmbed(configData);
	}
}
