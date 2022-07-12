const Discord = require("discord.js");
const { LogSeverity, log } = require("../log");

/**
 * Adds specified reaction to certain number element inside the calculated points result.
 *
 * @param { Discord.Client } client
 * @param { Discord.Message } message
 * @param { number[] } topCounts
 * @param { number } points
 *
 * @returns { Promise<void> }
 */
async function addWysiReaction(client, message, topCounts, points) {
  // TODO: validate topCounts array

  if(typeof(points) !== "number") {
    log(LogSeverity.ERROR, "addWysiReaction", "points must be number.");
    process.exit(1);
  }

  let wysi = false;
  topCounts.forEach(count => {
    if(count.toString().includes("727")) {
      wysi = true;
    }
  });

  // only run if wysi is not yet true
  if(!wysi) {
    wysi = points.toString().includes("727");
  }

  if(wysi) {
    log(LogSeverity.LOG, "addWysiReaction", "727 element detected. Adding reaction to message.");

    const emoji = client.emojis.cache.get(process.env.OSUHOW_EMOJI_ID);
    await message.react(emoji);
  }
}

module.exports = {
  addWysiReaction
};
