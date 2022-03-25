const Discord = require("discord.js");
const { Pool } = require("pg");
const { LogSeverity, log } = require("../log");
const { getAllAssignments, getLastAssignmentUpdate } = require("../db/assignments");
const { AssignmentSort } = require("../common");
const { createLeaderboardEmbed } = require("../messages/leaderboard");

async function sendPointLeaderboard(channel, pool) {
  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "sendPointLeaderboard", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(!(pool instanceof Pool)) {
    log(LogSeverity.ERROR, "sendPointLeaderboard", "pool must be a Pool object instance.");
    process.exit(1);
  }

  log(LogSeverity.LOG, "sendPointLeaderboard", "Retrieving leaderboard data.");

  const rankings = await getAllAssignments(pool, AssignmentSort.POINTS, true);
  const lastUpdated = new Date(await getLastAssignmentUpdate(pool));
  const draft = createLeaderboardEmbed(rankings, lastUpdated);

  await channel.send({ embeds: [ draft ] });
}

module.exports = {
  sendPointLeaderboard
};
