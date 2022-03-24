const Discord = require("discord.js");

function createLeaderboardEmbed(data, lastUpdated) {
  const draft = new Discord.MessageEmbed();
  const len = data.length;
  let rankingsDesc = "";

  if(len > 0) {
    draft.setTitle("Top 50 players based on points count");
    for(let i = 0; i < len; i++) {
      rankingsDesc += (i + 1).toString() + ". " + data[i].username + ": " + data[i].points;
      if(i < len - 1) {
        rankingsDesc += "\n";
      }
    }
    draft.setDescription(rankingsDesc);
    draft.setFooter({
      text: "Last updated: " +
        lastUpdated.getDate() + "/" +
        (lastUpdated.getMonth() + 1) + "/" +
        lastUpdated.getFullYear() + ", " +
        lastUpdated.getHours() + ":" +
        lastUpdated.getMinutes()
    });
  }
  else {
    draft.setDescription("Ranking list is empty. Go for the first!");
  }
  draft.setColor("#ff0000");

  return draft;
}

module.exports = {
  createLeaderboardEmbed
};
