import { Client, TextChannel, Message } from "discord.js";
import { DatabaseWrapper } from "../db";
import { UserData, Roles } from ".";
import { Log } from "../utils/log";
import { ServerNotFoundError } from "../errors/db";

/**
 * Verification commands class.
 */
class Verification {
  /**
   * Verifies the user and inserts their data into the database.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { string } osuToken osu! API token.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async verifyUser(client: Client, channel: TextChannel, osuToken: string, message: Message): Promise<void> {
    let serverData;

    try {
      serverData = await DatabaseWrapper.getInstance()
        .getServersModule()
        .getServerByDiscordId(channel.guild.id);
    }
    catch (e) {
      if(e instanceof ServerNotFoundError) {
        Log.error("verifyUser", `Server with ID ${ channel.guild.id } not found in database.`);
        await channel.send("**Error:** Server not in database.");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

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

    const osuUser = await UserData.fetchOsuUser(channel, osuToken, osuId);
    if(osuUser === null) {
      return;
    }

    const result = await UserData.insertUserData(channel, message.author.id, osuId, osuUser.userName, osuUser.country);
    if(!result) {
      return;
    }

    await channel.send(`Linked Discord user <@${ message.author.id }> to osu! user **${ osuUser.userName }**.`);

    if(serverData.verifiedRoleId === null) {
      Log.info("verifyUser", `${ serverData.discordId }: Server's verifiedRoleId not set. Role granting skipped.`);
      return;
    }

    await Roles.addRole(client, channel, message.author.id, channel.guild.id, serverData.verifiedRoleId);
    return;
  }
}

export default Verification;
