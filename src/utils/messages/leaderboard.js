const Discord = require("discord.js");
const { LogSeverity, log } = require("../time");
const { TimeOperation, getTimeOffsetFromString } = require("../time");

function createLeaderboardEmbed(data, lastUpdated) {
  if(!(lastUpdated instanceof Date)) {
    return false;
  }

  let timeOperation = TimeOperation.INCREMENT;
  let hourOffset = 0;
  let minuteOffset = 0;

  const offset = getTimeOffsetFromString(process.env.TZ_OFFSET);
  if(typeof(offset) === "undefined") {
    log(LogSeverity.WARN, "createLeaderboardEmbed", "Unable to get time offset from string. Using no offset.");
  }
  else {
    timeOperation = offset.operation;
    hourOffset = offset.hours;
    minuteOffset = offset.minutes;
  }

  const offsetLastUpdated = new Date(
    lastUpdated.getTime() +
    (
      (timeOperation === TimeOperation.INCREMENT ? 1 : -1) *
      (
        (hourOffset * 3600000) + (minuteOffset * 60000)
      )
    )
  );

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
        lastUpdated.getHours().toString().padStart(2, "0") + ":" +
        lastUpdated.getMinutes().toString().padStart(2, "0")
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
