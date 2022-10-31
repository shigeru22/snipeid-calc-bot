import { TextChannel } from "discord.js";
import { Pool } from "pg";
import { LogSeverity, log } from "../utils/log";
import { getAllAssignments, getLastAssignmentUpdate } from "../db/assignments";
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
  log(LogSeverity.LOG, "sendPointLeaderboard", "Retrieving leaderboard data.");

  const rankings = await getAllAssignments(db, channel.guildId, AssignmentSort.POINTS, true);
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
