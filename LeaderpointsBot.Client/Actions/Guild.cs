// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using Discord.WebSocket;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Exceptions;
using LeaderpointsBot.Database.Schemas;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public class Guild
{
	public static async Task InsertGuildToDatabase(DatabaseTransaction transaction, SocketGuild guild)
	{
		Log.WriteInfo($"Inserting new server data to database ({guild.Id}).");

		await Database.Tables.Servers.InsertServer(transaction, guild.Id.ToString());

		Log.WriteInfo($"Inserting empty role to server data ({guild.Id}).");

		ServersQuerySchema.ServersTableData guildData;
		try
		{
			guildData = await Database.Tables.Servers.GetServerByDiscordID(transaction, guild.Id.ToString());
		}
		catch (DataNotFoundException)
		{
			// should not be here since already inserted before
			Log.WriteError($"Inserted server data not found ({guild.Id}).");
			return;
		}

		await DatabaseFactory.Instance.RolesInstance.InsertRole("0", "(No role)", 0, guildData.ServerID);
	}
}
