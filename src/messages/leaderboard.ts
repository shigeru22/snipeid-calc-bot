import { EmbedBuilder } from "discord.js";
import { LogSeverity, log } from "../utils/log";
import { TimeOperation, TimeUtils } from "../utils/time";
import { IDBServerLeaderboardData } from "../types/db/users";

/**
 * Creates leaderboard embed message.
 *
 * @param { IDBLeaderboardData[] } data Leaderboard data.
 * @param { Date } lastUpdated Last update time.
 *
 * @returns { EmbedBuilder } Leaderboard embed message.
 */
function createLeaderboardEmbed(data: IDBServerLeaderboardData[], lastUpdated: Date): EmbedBuilder {
  let timeOperation = TimeOperation.INCREMENT;
  let hourOffset = 0;
  let minuteOffset = 0;

  const offset = TimeUtils.getTimeOffsetFromString(process.env.TZ_OFFSET as string);
  if(typeof(offset) === "undefined") {
    log(LogSeverity.WARN, "createLeaderboardEmbed", "Unable to get time offset from string. Using no offset.");
  }
  else {
    timeOperation = offset.operation;
    hourOffset = offset.hours;
    minuteOffset = offset.minutes;
  }

  const offsetLastUpdated = new Date(
    lastUpdated.getTime() + (
      (timeOperation === TimeOperation.INCREMENT ? 1 : -1) * (
        (hourOffset * 3600000) + ((minuteOffset + lastUpdated.getTimezoneOffset()) * 60000) // also offset based on timezone offset minutes
      )
    )
  );

  const len = data.length;
  let rankingsDesc = "";

  if(len > 0) {
    data.forEach((data, index) => {
      rankingsDesc += `${ index + 1 }. ${ data.userName }: ${ data.points }${ index < (len - 1) ? "\n" : "" }`;
    });
  }
  else {
    rankingsDesc = "Ranking list is empty. Go for the first!";
  }

  const draft = new EmbedBuilder().setTitle("Top 50 players based on points count")
    .setDescription(rankingsDesc)
    .setFooter({
      text: `Last updated: ${ offsetLastUpdated.getDate() }/${ offsetLastUpdated.getMonth() + 1 }/${ offsetLastUpdated.getFullYear() }, ${ offsetLastUpdated.getHours().toString().padStart(2, "0") }:${ offsetLastUpdated.getMinutes().toString().padStart(2, "0") }`
    })
    .setColor("#ff0000");

  return draft;
}

export { createLeaderboardEmbed };
