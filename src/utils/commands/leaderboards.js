const Discord = require("discord.js");
const { Pool } = require("pg");
const { LogSeverity, log } = require("../log");
const { getAllAssignments, getLastAssignmentUpdate } = require("../db/assignments");
const { AssignmentSort } = require("../common");
const { createLeaderboardEmbed } = require("../messages/leaderboard");

/**
 * Sends top 50 leaderboard to specified channel.
 *
 * @param { Discord.Channel } channel
 * @param { Pool } db
 *
 * @returns { Promise<void> }
 */
async function sendPointLeaderboard(channel, db) {
  log(LogSeverity.LOG, "sendPointLeaderboard", "Retrieving leaderboard data.");

  const rankings = await getAllAssignments(db, AssignmentSort.POINTS, true);
  const lastUpdated = new Date(await getLastAssignmentUpdate(db));
  const draft = createLeaderboardEmbed(rankings, lastUpdated);

  await channel.send({ embeds: [ draft ] });
}

module.exports = {
  sendPointLeaderboard
};
