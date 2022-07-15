const { LogSeverity, log } = require("../log");
const { getAllAssignments, getLastAssignmentUpdate } = require("../db/assignments");
const { AssignmentSort, DatabaseErrors } = require("../common");
const { createLeaderboardEmbed } = require("../messages/leaderboard");

/**
 * Sends top 50 leaderboard from the database to specified channel.
 *
 * @param { import("discord.js").TextChannel } channel - Discord channel to send message to.
 * @param { import("pg").Pool } db - Database connection pool.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function sendPointLeaderboard(channel, db) {
  log(LogSeverity.LOG, "sendPointLeaderboard", "Retrieving leaderboard data.");

  const rankings = await getAllAssignments(db, AssignmentSort.POINTS, true);
  if(typeof(rankings) === "number") {
    // @ts-ignore
    switch(rankings) { // rankings is number, or DatabaseErrors constant
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

    if(typeof(lastUpdateQuery) === "number") {
      switch(lastUpdateQuery) {
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

    lastUpdated = new Date(lastUpdateQuery);
  }

  const draft = createLeaderboardEmbed(rankings, lastUpdated);

  await channel.send({ embeds: [ draft ] });
}

module.exports = {
  sendPointLeaderboard
};
