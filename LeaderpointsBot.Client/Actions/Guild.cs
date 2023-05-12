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
	public static async Task InsertGuildToDatabase(SocketGuild guild)
	{
		Log.WriteVerbose("Checking for existing server in database.");

		try
		{
			ServersQuerySchema.ServersTableData tempServer = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
			if (tempServer.DiscordID.Equals(guild.Id.ToString()))
			{
				Log.WriteInfo($"Server already exists in database ({guild.Id}).");
				return;
			}
		}
		catch (DataNotFoundException)
		{
			// exception thrown is server not found, continue instead
			Log.WriteVerbose("DataNotFound exception thrown. Ignoring since this is intended.");
		}

		Log.WriteInfo($"Inserting new server data to database ({guild.Id}).");

		await DatabaseFactory.Instance.ServersInstance.InsertServer(guild.Id.ToString());

		Log.WriteInfo($"Inserting empty role to server data ({guild.Id}).");

		ServersQuerySchema.ServersTableData guildData;
		try
		{
			guildData = await DatabaseFactory.Instance.ServersInstance.GetServerByDiscordID(guild.Id.ToString());
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
