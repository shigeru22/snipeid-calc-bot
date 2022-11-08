import { Client, TextChannel, Message } from "discord.js";
import { Pool } from "pg";
import { getServerByDiscordId } from "../db/servers";
import { insertUserData, fetchOsuUser } from "./userdata";
import { addRole } from "./roles";
import { LogSeverity, log } from "../utils/log";
import { DatabaseSuccess } from "../utils/common";

/**
 * Verifies the user and inserts their data into the database.
 *
 * @param { Client } client Discord bot client.
 * @param { TextChannel } channel Discord channel to send message to.
 * @param { Pool } db Database connection pool.
 * @param { string } osuToken osu! API token.
 * @param { Message } message Message that triggered the command.
 *
 * @returns { Promise<void> } Promise object with no return value.
 */
async function verifyUser(client: Client, channel: TextChannel, db: Pool, osuToken: string, message: Message): Promise<void> {
  const serverData = await getServerByDiscordId(db, channel.guild.id);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.WARN, "verifyUser", "Someone asked for user verification, but server not in database.");
    return;
  }

  const contents = message.content.split(/\s+/g); // split by one or more spaces

  if(typeof(contents[2]) !== "string") {
    await channel.send(`You need to specify your osu! user ID: \`@${ client.user?.username } link [osu! user ID]\``);
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

  if(osuUser.country !== serverData.data.country) {
    await channel.send("**Error:** Wrong country code from osu! profile. Please contact server moderators.");
    return;
  }

  const result = await insertUserData(channel, db, message.author.id, osuId, osuUser.userName, osuUser.country);
  if(!result) {
    return;
  }

  if(serverData.data.verifiedRoleId === null) {
    log(LogSeverity.LOG, "verifyUser", `${ serverData.data.discordId }: Server's verifiedRoleId not set. Role granting skipped.`);
    return;
  }

  await channel.send(`Linked Discord user <@${ message.author.id }> to osu! user **${ osuUser.userName }**`);

  await addRole(client, channel, message.author.id, channel.guild.id, serverData.data.verifiedRoleId);
  return;
}

export { verifyUser };
