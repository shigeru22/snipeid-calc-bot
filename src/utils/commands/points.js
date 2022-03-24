const Discord = require("discord.js");
const { calculatePoints, counter } = require("../messages/counter");

async function countPoints(channel, username, topCounts) {
  if(!(channel instanceof Discord.Channel)) {
    console.log("[ERROR] countPoints :: channel must be a Discord.Channel object instance.");
    process.exit(1);
  }

  if(typeof(username) !== "string") {
    console.log("[ERROR] countPoints :: username must be string.");
    process.exit(1);
  }

  // TODO: validate topCounts array

  console.log("[LOG] countPoints :: Calculating points for username: " + username);
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
