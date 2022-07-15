const { LogSeverity, log } = require("../log");

/**
 * Adds specified reaction to certain number element inside the calculated points result.
 *
 * @param { import("discord.js").Client } client - Discord bot client.
 * @param { import("discord.js").Message } message - Discord message to add reaction to.
 * @param { number[] } topCounts - Array of top counts.
 * @param { number } points - Calculated points.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function addWysiReaction(client, message, topCounts, points) {
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
