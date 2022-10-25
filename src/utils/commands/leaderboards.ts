import { TextChannel } from "discord.js";
import { Pool } from "pg";
import { LogSeverity, log } from "../log";
import { getAllAssignments, getLastAssignmentUpdate } from "../db/assignments";
import { AssignmentSort, DatabaseErrors } from "../common";
import { createLeaderboardEmbed } from "../messages/leaderboard";

/**
 * Sends top 50 leaderboard from the database to specified channel.
 *
 * @param { TextChannel } channel - Discord channel to send message to.
 * @param { Pool } db - Database connection pool.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function sendPointLeaderboard(channel: TextChannel, db: Pool): Promise<void> {
  log(LogSeverity.LOG, "sendPointLeaderboard", "Retrieving leaderboard data.");

  const rankings = await getAllAssignments(db, AssignmentSort.POINTS, true);
  if(rankings.status !== DatabaseErrors.OK || rankings.assignments === undefined) {
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
    const lastUpdateQuery = await getLastAssignmentUpdate(db);

    if(lastUpdateQuery.status !== DatabaseErrors.OK || lastUpdateQuery.date === undefined) {
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

    lastUpdated = new Date(lastUpdateQuery.date);
  }

  const draft = createLeaderboardEmbed(rankings.assignments, lastUpdated);

  await channel.send({ embeds: [ draft ] });
}

export { sendPointLeaderboard };
