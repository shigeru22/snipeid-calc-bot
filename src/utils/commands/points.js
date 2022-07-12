const Discord = require("discord.js");
const { LogSeverity, log } = require("../log");
const { counter } = require("../messages/counter");

/**
 * Sends calculated points and embed to specified channel.
 *
 * @param { Discord.Channel } channel
 * @param { string } username
 * @param { number[] } topCounts
 *
 * @returns { Promise<Discord.Message> }
 */
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

  const ret = await channel.send({ embeds: [ draft ] });
  return ret;
}

module.exports = {
  countPoints
};
