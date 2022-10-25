import { Client, TextChannel, Message } from "discord.js";
import { Pool } from "pg";
import { insertUserData, fetchOsuUser } from "./userdata";
import { addRole } from "./roles";
import { LogSeverity, log } from "../log";

/**
 * Verifies the user and inserts their data into the database.
 *
 * @param { Client } client - Discord bot client.
 * @param { TextChannel } channel - Discord channel to send message to.
 * @param { Pool } db - Database connection pool.
 * @param { string } osuToken - osu! API token.
 * @param { Message } message - Message that triggered the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function verifyUser(client: Client, channel: TextChannel, db: Pool, osuToken: string, message: Message): Promise<void> {
  const contents = message.content.split(/\s+/g); // split by one or more spaces

  if(typeof(contents[2]) !== "string") {
    await channel.send("You need to specify your osu! user ID: `@" + process.env.BOT_NAME + " link [osu! user ID]`");
    return;
  }

  const osuId = parseInt(contents[2], 10);

  if(isNaN(osuId)) {
    await channel.send("**Error:** ID must be in numbers.");
    return;
  }

  if(osuId <= 0) {
    await channel.send("**Error:** I see what you did there. That's funny.");
    return;
  }

  const osuUser = await fetchOsuUser(channel, osuToken, osuId);
  if(typeof(osuUser) === "boolean") { // infer boolean returns as not found value
    return;
  }

  if(!osuUser.isCountryCodeAllowed) {
    await channel.send("**Error:** Wrong country code from osu! profile. Please contact server moderators.");
    return;
  }

  const result = await insertUserData(channel, db, message.author.id, osuId, osuUser.username);
  if(!result) {
    return;
  }

  if(typeof(process.env.VERIFIED_ROLE_ID) !== "string" || process.env.VERIFIED_ROLE_ID === "") {
    log(LogSeverity.LOG, "onNewMessage", "VERIFIED_ROLE_ID not set. Role granting skipped.");
    return;
  }

  await addRole(client, channel, message.author.id, process.env.SERVER_ID as string, process.env.VERIFIED_ROLE_ID);
  return;
}

export { verifyUser };
