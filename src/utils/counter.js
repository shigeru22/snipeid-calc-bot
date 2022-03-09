const Discord = require("discord.js");

function counter(top_1, top_8, top_15, top_25, top_50, username) {
  const draft = new Discord.MessageEmbed();
  draft.setTitle("");
  draft.setDescription("Test");
  draft.setColor("#ff0000");

  return draft;
}

module.exports = counter;