const Discord = require("discord.js");
const { LogSeverity, log } = require("../time");
const { TimeOperation, getTimeOffsetFromString } = require("../time");

/**
 * Creates leaderboard embed message.
 *
 * @param { { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date }[] } data
 * @param { Date } lastUpdated
 *
 * @returns { Discord.MessageEmbed }
 */
function createLeaderboardEmbed(data, lastUpdated) {
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
        (hourOffset * 3600000) + ((minuteOffset + lastUpdated.getTimezoneOffset()) * 60000) // also offset based on timezone offset minutes
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
        offsetLastUpdated.getDate() + "/" +
        (offsetLastUpdated.getMonth() + 1) + "/" +
        offsetLastUpdated.getFullYear() + ", " +
        offsetLastUpdated.getHours().toString().padStart(2, "0") + ":" +
        offsetLastUpdated.getMinutes().toString().padStart(2, "0")
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
