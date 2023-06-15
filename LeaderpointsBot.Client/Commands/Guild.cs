// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Commands;

public static class Guild
{
	public static async Task InsertGuildData(SocketGuild guild)
	{
		DatabaseTransaction transaction = DatabaseFactory.Instance.InitializeTransaction();

		Log.WriteVerbose("Checking for existing server in database.");
		try
		{
			Servers.ServersTableData tempServer = await Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
			if (tempServer.DiscordID.Equals(guild.Id.ToString()))
			{
				Log.WriteInfo($"Server already exists in database ({guild.Id}).");
				return;
			}
		}
		catch (DataNotFoundException)
		{
			// exception thrown is server not found, continue instead
			Log.WriteDebug("DataNotFound exception thrown. Ignoring since this is intended.");
		}

		await Actions.Guild.InsertGuildToDatabase(transaction, guild);

		await transaction.CommitAsync();

		Log.WriteInfo($"Server added to database ({guild.Id}).");
	}
}
