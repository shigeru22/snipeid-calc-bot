const Discord = require("discord.js");
const { LogSeverity, log } = require("../log");

async function addWysiReaction(client, message, topCounts, points) {
  if(!(client instanceof Discord.Client)) {
    log(LogSeverity.ERROR, "addWysiReaction", "client must be a Discord.Client object instance.");
    process.exit(1);
  }

  if(!(message instanceof Discord.Message)) {
    log(LogSeverity.ERROR, "addWysiReaction", "message must be string.");
    process.exit(1);
  }

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
