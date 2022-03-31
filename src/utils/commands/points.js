const Discord = require("discord.js");
const { LogSeverity, log } = require("../log");
const { counter } = require("../messages/counter");

async function countPoints(channel, username, topCounts) {
  if(!(channel instanceof Discord.Channel)) {
    log(LogSeverity.ERROR, "countPoints", "channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(typeof(username) !== "string") {
    log(LogSeverity.ERROR, "countPoints", "username must be string.");
    process.exit(1);
  }

  // TODO: validate topCounts array

  log(LogSeverity.LOG, "countPoints", "Calculating points for username: " + username);
  const draft = counter(
    topCounts[0],
    topCounts[1],
    topCounts[2],
    topCounts[3],
    topCounts[4],
    username
  );

  return (await channel.send({ embeds: [ draft ] }));
}

module.exports = {
  countPoints
};
