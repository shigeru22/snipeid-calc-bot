const { LogSeverity, log } = require("../log");
const { counter } = require("../messages/counter");

/**
 * Sends calculated points and embed to specified channel.
 *
 * @param { import("discord.js").TextChannel } channel - Discord channel to send message to.
 * @param { string } username - osu! username.
 * @param { number[] } topCounts - Array of top counts.
 *
 * @returns { Promise<import("discord.js").Message> } Promise object with `Discord.Message` sent message object.
 */
async function countPoints(channel, username, topCounts) {
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
