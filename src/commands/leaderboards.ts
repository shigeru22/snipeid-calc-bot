import { TextChannel } from "discord.js";
import { Pool } from "pg";
import { Log } from "../utils/log";
import { DBUsers, DBServers } from "../db";
import { DatabaseErrors, DatabaseSuccess } from "../utils/common";
import { createLeaderboardEmbed } from "../messages/leaderboard";

class Leaderboards {
  /**
   * Sends top 50 leaderboard from the database to specified channel.
   *
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { Pool } db Database connection pool.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async sendPointLeaderboard(channel: TextChannel, db: Pool): Promise<void> {
    const serverData = await DBServers.getServerByDiscordId(db, channel.guild.id);

    if(serverData.status !== DatabaseSuccess.OK) {
      Log.warn("sendPointLeaderboard", "Someone asked for leaderboard count, but server not in database.");
      return;
    }

    {
      const isCommand = await DBServers.isLeaderboardChannel(db, channel.guild.id, channel.id);
      switch(isCommand) {
        case false:
          Log.warn("sendPointLeaderboard", `${ channel.guild.id }: Not in commands channel.`);
          await channel.send(`**Error:** Enter this command at <#${ serverData.data.leaderboardsChannelId }> channel.`); // fallthrough
        case null:
          return;
      }
    }

    Log.info("sendPointLeaderboard", "Retrieving leaderboard data.");

    Log.debug("sendPointLeaderboard", `country: ${ serverData.data.country }`);

    const rankings = serverData.data.country === null ? await DBUsers.getPointsLeaderboard(db, channel.guild.id) : await DBUsers.getPointsLeaderboardByCountry(db, channel.guild.id, serverData.data.country.toUpperCase());

    if(rankings.status !== DatabaseSuccess.OK) {
      switch(rankings.status) { // rankings is number, or DatabaseErrors constant
        case DatabaseErrors.NO_RECORD:
          await channel.send("**Error:** No records found. Be the first!");
          break;
        case DatabaseErrors.CONNECTION_ERROR:
          await channel.send("**Error:** Database connection failed. Please contact bot administrator.");
          break;
        case DatabaseErrors.CLIENT_ERROR:
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          break;
      }

      return;
    }

    let lastUpdated = new Date();
    {
      const lastUpdateQuery = await DBUsers.getLastPointUpdate(db, channel.guildId);

      if(lastUpdateQuery.status !== DatabaseSuccess.OK) {
        switch(lastUpdateQuery.status) {
          case DatabaseErrors.NO_RECORD:
            await channel.send("**Error:** No records found. Be the first!");
            break;
          case DatabaseErrors.CONNECTION_ERROR:
            await channel.send("**Error:** Database connection failed. Please contact bot administrator.");
            break;
          case DatabaseErrors.CLIENT_ERROR:
            await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
            break;
        }

        return;
      }

      lastUpdated = new Date(lastUpdateQuery.data);
    }

    const draft = createLeaderboardEmbed(rankings.data, lastUpdated);
    await channel.send({ embeds: [ draft ] });

    Log.info("sendPointLeaderboard", `Leaderboard sent for server ID ${ channel.guildId } (${ channel.guild.name }).`);
  }
}

export default Leaderboards;
