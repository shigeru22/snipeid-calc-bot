import { TextChannel } from "discord.js";
import { Pool } from "pg";
import { DBUsers, DBServers } from "../db";
import { Log } from "../utils/log";
import { createLeaderboardEmbed } from "../messages/leaderboard";
import { ServerNotFoundError, NoRecordError } from "../errors/db";

/**
 * Leaderboard commands class.
 */
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
    let serverData;

    {
      try {
        serverData = await DBServers.getServerByDiscordId(db, channel.guild.id);
      }
      catch (e) {
        if(e instanceof ServerNotFoundError) {
          Log.error("sendPointLeaderboard", `Server with ID ${ channel.guild.id } not found in database.`);
          await channel.send("**Error:** Server not in database.");
        }
        else {
          await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        }

        return;
      }
    }

    {
      let isCommand;

      try {
        isCommand = await DBServers.isCommandChannel(db, channel.guild.id, channel.id);
      }
      catch (e) {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        return;
      }

      switch(isCommand) {
        case false:
          Log.warn("sendPointLeaderboard", `${ channel.guild.id }: Not in commands channel.`); // fallthrough
        case null:
          return;
      }
    }

    Log.info("sendPointLeaderboard", "Retrieving leaderboard data.");

    let rankings;

    try {
      rankings = serverData.country === null ? await DBUsers.getServerPointsLeaderboard(db, channel.guild.id) : await DBUsers.getServerPointsLeaderboardByCountry(db, channel.guild.id, serverData.country.toUpperCase());
    }
    catch (e) {
      if(e instanceof NoRecordError) {
        await channel.send("**Error:** No records found. Be the first!");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return;
    }

    let lastUpdated;
    {
      try {
        lastUpdated = await DBUsers.getServerLastPointUpdate(db, channel.guildId);
      }
      catch (e) {
        if(e instanceof NoRecordError) {
          await channel.send("**Error:** No records found. Be the first!");
        }
        else {
          await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        }

        return;
      }
    }

    const draft = createLeaderboardEmbed(rankings, lastUpdated);
    await channel.send({ embeds: [ draft ] });

    Log.info("sendPointLeaderboard", `Leaderboard sent for server ID ${ channel.guildId } (${ channel.guild.name }).`);
  }
}

export default Leaderboards;
