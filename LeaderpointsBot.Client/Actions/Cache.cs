// Copyright (c) shigeru22, concept by Akshiro28.
// Licensed under the MIT license. See LICENSE in the repository root for details.

using LeaderpointsBot.Client.Caching;
using LeaderpointsBot.Database;
using LeaderpointsBot.Database.Tables;
using LeaderpointsBot.Utils;

namespace LeaderpointsBot.Client.Actions;

public static class Cache
{
	public static async Task PopulateGuildConfigurations(DatabaseTransaction transaction)
	{
		Log.WriteVerbose("Acquiring server data and storing to cache.");

		Servers.ServersTableData[] guildsData = await Servers.GetServers(transaction);

		if (guildsData.Length <= 0)
		{
			// nothing to process
			Log.WriteVerbose("No server data found in database. Skipping process.");
			return;
		}

		foreach (Servers.ServersTableData guildData in guildsData)
		{
			CacheManager.Instance.GuildCacheInstance.AddDatabaseCache(guildData.DiscordID, guildData);

			// test data integrity
			var tempCacheData = CacheManager.Instance.GuildCacheInstance.GetDatabaseCache(guildData.DiscordID);
			if (!tempCacheData.Equals(guildData))
			{
				throw new InvalidDataException("Inconsistent cache value detected.");
			}
		}

		Log.WriteVerbose("Server cache data populated from database.");
	}
}
