import { TextChannel } from "discord.js";
import { Pool } from "pg";
import { LogSeverity, log } from "../utils/log";
import { getAllAssignments, getAllAssignmentsByCountry, getLastAssignmentUpdate } from "../db/assignments";
import { getServerByDiscordId, isLeaderboardChannel } from "../db/servers";
import { AssignmentSort, DatabaseErrors, DatabaseSuccess } from "../utils/common";
import { createLeaderboardEmbed } from "../messages/leaderboard";

/**
 * Sends top 50 leaderboard from the database to specified channel.
 *
 * @param { TextChannel } channel Discord channel to send message to.
 * @param { Pool } db Database connection pool.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function sendPointLeaderboard(channel: TextChannel, db: Pool): Promise<void> {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "sendPointLeaderboard", "Someone asked for leaderboard count, but server not in database.");
    return;
  }

  {
    const isCommand = await isLeaderboardChannel(db, channel.guild.id, channel.id);
    switch(isCommand) {
      case false:
        log(LogSeverity.WARN, "sendPointLeaderboard", `${ channel.guild.id }: Not in commands channel.`);
        await channel.send(`**Error:** Enter this command at <#${ serverData.data.leaderboardsChannelId }> channel.`); // fallthrough
      case null:
        return;
    }
  }

  log(LogSeverity.LOG, "sendPointLeaderboard", "Retrieving leaderboard data.");

  const rankings = serverData.data.country === null ? await getAllAssignments(db, channel.guild.id, AssignmentSort.POINTS, true) : await getAllAssignmentsByCountry(db, channel.guild.id, serverData.data.country, AssignmentSort.POINTS, true);

  if(rankings.status !== DatabaseSuccess.OK) {
    switch(rankings.status) { // rankings is number, or DatabaseErrors constant
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
    const lastUpdateQuery = await getLastAssignmentUpdate(db, channel.guildId);

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

  log(LogSeverity.LOG, "sendPointLeaderboard", `Leaderboard sent for server ID ${ channel.guildId } (${ channel.name }).`);
}

export { sendPointLeaderboard };
