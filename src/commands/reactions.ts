import { Client, Message } from "discord.js";
import { LogSeverity, log } from "../utils/log";

/**
 * Adds specified reaction to certain number element inside the calculated points result.
 *
 * @param { Client } client Discord bot client.
 * @param { Message } message Discord message to add reaction to.
 * @param { number[] } topCounts Array of top counts.
 * @param { number } points Calculated points.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function addWysiReaction(client: Client, message: Message, topCounts: number[], points: number): Promise<void> {
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

    const emoji = client.emojis.cache.get(process.env.OSUHOW_EMOJI_ID as string);
    if(emoji === undefined) {
      return;
    }

    await message.react(emoji);
  }
}

export { addWysiReaction };
