const Discord = require("discord.js");

/**
 * Calculates points for each top rank count.
 *
 * @param { number } top_1
 * @param { number } top_8
 * @param { number } top_15
 * @param { number } top_25
 * @param { number } top_50
 *
 * @returns { number }
 */
function calculatePoints(top_1, top_8, top_15, top_25, top_50) {
  return top_1 * 5 +
    (top_8 - top_1) * 3 +
    (top_15 - top_8) * 2 +
    (top_25 - top_15) +
    (top_50 - top_25);
}

/**
 * Creates counter draft embed message.
 *
 * @param { number } top_1
 * @param { number } top_8
 * @param { number } top_15
 * @param { number } top_25
 * @param { number } top_50
 *
 * @returns { Discord.MessageEmbed }
 */
function counter(top_1, top_8, top_15, top_25, top_50, username) {
  const draft = new Discord.MessageEmbed();

  const points = calculatePoints(top_1, top_8, top_15, top_25, top_50);

  draft.setTitle("Points for "+ username + ":");
  draft.setDescription("```"
  + ((top_1 * 5).toString().padEnd(6)) + "= " + (top_1 + " x 5").toString().padStart(19) + "\n"
  + (((top_8 - top_1) * 3).toString().padEnd(6)) + "= " + ("(" + top_8 + " - " + top_1 + ") x 3").toString().padStart(19) + "\n"
  + (((top_15 - top_8) * 2).toString().padEnd(6)) + "= " + ("(" + top_15 + " - " + top_8 + ") x 2").toString().padStart(19) + "\n"
  + ((top_25 - top_15).toString().padEnd(6)) + "= " + ("(" + top_25 + " - " + top_15 + ") x 1").toString().padStart(19) + "\n"
  + ((top_50 - top_25).toString().padEnd(6)) + "= " + ("(" + top_50 + " - " + top_25 + ") x 1").toString().padStart(19) + "\n```"
  + "\n= **" + points + "** points.");
  draft.setColor("#ff0000");

  return draft;
}

module.exports = {
  calculatePoints,
  counter
};
