const Discord = require("discord.js");

async function addWysiReaction(client, message, topCounts, points) {
  if(!(client instanceof Discord.Client)) {
    console.log("[ERROR] addWysiReaction :: client must be a Discord.Client object instance.");
    process.exit(1);
  }

  if(!(message instanceof Discord.Message)) {
    console.log("[ERROR] addWysiReaction :: message must be string.");
    process.exit(1);
  }

  // TODO: validate topCounts array

  if(typeof(points) !== "number") {
    console.log("[ERROR] addWysiReaction :: points must be number.");
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
    const emoji = client.emojis.cache.get(process.env.OSUHOW_EMOJI_ID);
    await message.react(emoji);
  }
}

module.exports = {
  addWysiReaction
};
