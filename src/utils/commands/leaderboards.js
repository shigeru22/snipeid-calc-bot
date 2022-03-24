const Discord = require("discord.js");
const { Pool } = require("pg");
const { getAllAssignments, getLastAssignmentUpdate } = require("../db/assignments");
const { AssignmentSort } = require("../common");
const { createLeaderboardEmbed } = require("../messages/leaderboard");

async function sendPointLeaderboard(channel, pool) {
  if(!(channel instanceof Discord.Channel)) {
    console.log("[ERROR] sendPointLeaderboard :: channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(!(pool instanceof Pool)) {
    console.log("[ERROR] sendPointLeaderboard :: pool must be a Pool object instance.");
    process.exit(1);
  }

  const rankings = await getAllAssignments(pool, AssignmentSort.POINTS, true);
  const lastUpdated = new Date(await getLastAssignmentUpdate(pool));
  const draft = createLeaderboardEmbed(rankings, lastUpdated);

  await channel.send({ embeds: [ draft ] });
}

module.exports = {
  sendPointLeaderboard
};
