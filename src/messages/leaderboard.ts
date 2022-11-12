import { EmbedBuilder } from "discord.js";
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
      text: `Last updated: ${ lastUpdated.getDate() }/${ lastUpdated.getMonth() + 1 }/${ lastUpdated.getFullYear() }, ${ lastUpdated.getHours().toString().padStart(2, "0") }:${ lastUpdated.getMinutes().toString().padStart(2, "0") } (UTC)`
    })
    .setColor("#ff0000");

  return draft;
}

export { createLeaderboardEmbed };
