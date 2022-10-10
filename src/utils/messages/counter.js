const { MessageEmbed } = require("discord.js");

/**
 * Calculates points for each top rank count.
 *
 * @param { number } top1 - Top 1 count.
 * @param { number } top8 - Top 8 count.
 * @param { number } top15 - Top 15 count.
 * @param { number } top25 - Top 25 count.
 * @param { number } top50 - Top 50 count.
 *
 * @returns { number } Calculated points.
 */
function calculatePoints(top1, top8, top15, top25, top50) {
  return top1 * 5 + (top8 - top1) * 3 + (top15 - top8) * 2 + (top25 - top15) + (top50 - top25);
}

/**
 * Calculates points for each top rank count from respektive's API.
 *
 * @param { number } top1 - Top 1 count.
 * @param { number } top8 - Top 8 count.
 * @param { number } top25 - Top 25 count.
 * @param { number } top50 - Top 50 count.
 *
 * @returns { number } Calculated points.
 */
function calculateRespektivePoints(top1, top8, top25, top50) {
  return top1 * 5 + (top8 - top1) * 3 + (top25 - top8) * 2 + (top50 - top25);
}

/**
 * Creates counter draft embed message.
 *
 * @param { number } top1 - Top 1 count.
 * @param { number } top8 - Top 8 count.
 * @param { number } top15 - Top 15 count.
 * @param { number } top25 - Top 25 count.
 * @param { number } top50 - Top 50 count.
 * @param { string } username - osu! username.
 *
 * @returns { MessageEmbed }
 */
function counter(top1, top8, top15, top25, top50, username) {
  const draft = new MessageEmbed();

  const points = calculatePoints(top1, top8, top15, top25, top50);

  draft.setTitle("Points for " + username + ":");
  draft.setDescription("```" + ((top1 * 5).toString().padEnd(6)) + "= " + (top1 + " x 5").toString().padStart(19) + "\n" + (((top8 - top1) * 3).toString().padEnd(6)) + "= " + ("(" + top8 + " - " + top1 + ") x 3").toString().padStart(19) + "\n" + (((top15 - top8) * 2).toString().padEnd(6)) + "= " + ("(" + top15 + " - " + top8 + ") x 2").toString().padStart(19) + "\n" + ((top25 - top15).toString().padEnd(6)) + "= " + ("(" + top25 + " - " + top15 + ") x 1").toString().padStart(19) + "\n" + ((top50 - top25).toString().padEnd(6)) + "= " + ("(" + top50 + " - " + top25 + ") x 1").toString().padStart(19) + "\n```" + "\n= **" + points + "** points.");
  draft.setColor("#ff0000");

  return draft;
}

/**
 * Creates counter draft embed message.
 *
 * @param { number } top1 - Top 1 count.
 * @param { number } top8 - Top 8 count.
 * @param { number } top25 - Top 25 count.
 * @param { number } top50 - Top 50 count.
 * @param { string } username - osu! username.
 *
 * @returns { MessageEmbed }
 */
function counterRespektive(top1, top8, top25, top50, username) {
  const draft = new MessageEmbed();

  const points = calculateRespektivePoints(top1, top8, top25, top50);

  draft.setTitle("Points for " + username + ":");
  draft.setDescription("```" + "Top 1 : " + ((top1 * 5).toString().padEnd(6)) + "= " + (top1 + " x 5").toString().padStart(19) + "\n" + "Top 8 : " + (((top8 - top1) * 3).toString().padEnd(6)) + "= " + ("(" + top8 + " - " + top1 + ") x 3").toString().padStart(19) + "\n" + "Top 25: " + ((top25 - top8).toString().padEnd(6)) + "= " + ("(" + top25 + " - " + top8 + ") x 1").toString().padStart(19) + "\n" + "Top 50: " + ((top50 - top25).toString().padEnd(6)) + "= " + ("(" + top50 + " - " + top25 + ") x 1").toString().padStart(19) + "\n```" + "\n= **" + points + "** points.");
  draft.setColor("#ff0000");
  draft.setFooter({ text: "Note: you might lost points due to respektive's API not returning top 15 count." });

  return draft;
}

module.exports = {
  calculatePoints,
  calculateRespektivePoints,
  counter,
  counterRespektive
};
