import { Client, TextChannel, Message } from "discord.js";
import { getUserByOsuId } from "../api/osu";
import { DatabaseWrapper } from "../db";
import { Reactions, UserData } from ".";
import { Parser, Environment } from "../utils";
import { Log } from "../utils/log";
import { OsuUserStatus } from "../utils/common";
import { calculatePoints, calculateRespektivePoints, counter, counterRespektive } from "../messages/counter";
import { NotFoundError } from "../errors/api";
import { UserNotFoundError, ServerNotFoundError } from "../errors/db";
import { InvalidExpressionError, InvalidTypeError, InvalidNumberOfRanksError, InvalidTopRankError } from "../errors/utils/parser";
import { isOsuUser } from "../types/api/osu";

/**
 * Count commands class.
 */
class Count {
  // <osc, using Bathbot message response
  /**
   * Sends calculated points from Bathbot `<osc` command.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Channel to send points result to.
   * @param { string } osuToken osu! API token.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async userLeaderboardsCountFromBathbot(client: Client, channel: TextChannel, osuToken: string, message: Message): Promise<void> {
    {
      let isCommand;

      try {
        isCommand = await DatabaseWrapper.getInstance().getServersModule().isCommandChannel(channel.guild.id, channel.id);
      }
      catch (e) {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        return;
      }

      switch(isCommand) {
        case false:
          Log.warn("userLeaderboardsCountFromBathbot", `${ channel.guild.id }: Not in commands channel.`); // fallthrough
        case null:
          return;
      }
    }

    const index = message.embeds.findIndex(
      embed => typeof(embed.title) === "string" && embed.title.toLowerCase().startsWith("in how many top x map leaderboards is")
    ); // <osc command should return at index 0, else it's not the specified command

    if(index === -1) {
      return;
    }

    let title: string;
    let desc: string;
    let link: string;

    {
      const tempTitle = message.embeds[index].title;
      const tempDesc = message.embeds[index].description;
      const author = message.embeds[index].author;

      if(tempTitle === null || tempDesc === null) {
        return;
      }

      if(author === null || author.url === undefined) {
        return;
      }

      title = tempTitle;
      desc = tempDesc;
      link = author.url;
    }

    const username = Parser.parseUsername(title);
    const osuId = Parser.parseOsuIdFromLink(link);

    let topCounts; // [ top_1, top_8, top_15, top_25, top_50 ]

    try {
      topCounts = Parser.parseTopCountDescription(desc);
    }
    catch (e) {
      return;
    }
    const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
    await this.countPoints(client, channel, username, topCounts);

    await UserData.updateUserData(osuToken, client, channel, osuId, points);
  }

  // @[BOT_NAME] count
  /**
   * Sends top leaderboard count to specified channel.
   * Basically, this is Bathbot's `<osc` command.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Channel to send points result to.
   * @param { string } osuToken osu! API token.
   * @param { string } discordId Discord ID of the user who sent the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async userLeaderboardsCount(client: Client, channel: TextChannel, osuToken: string, discordId: string): Promise<void> {
    let serverData;
    let user;
    let osuUser;

    try {
      serverData = await DatabaseWrapper.getInstance().getServersModule().getServerByDiscordId(channel.guild.id);
    }
    catch (e) {
      if(e instanceof ServerNotFoundError) {
        Log.error("userLeaderboardsCount", `Server with ID ${ channel.guild.id } not found in database.`);
        await channel.send("**Error:** Server not in database.");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return;
    }

    {
      let isCommand;

      try {
        isCommand = await DatabaseWrapper.getInstance().getServersModule().isCommandChannel(channel.guild.id, channel.id);
      }
      catch (e) {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        return;
      }

      switch(isCommand) {
        case false:
          Log.warn("userLeaderboardsCount", `${ channel.guild.id }: Not in commands channel.`); // fallthrough
        case null:
          return;
      }
    }

    try {
      user = await DatabaseWrapper.getInstance().getUsersModule().getDiscordUserByDiscordId(discordId);
    }
    catch (e) {
      if(e instanceof UserNotFoundError) {
        await channel.send(`**Error:** You haven't linked your account. Link using \`${ client.user?.username } [osu! user ID]\`${ serverData.verifyChannelId !== null ? ` in <#${ serverData.verifyChannelId }> channel` : "" }.`);
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return;
    }

    try {
      osuUser = await getUserByOsuId(osuToken, user.osuId);

      if(!isOsuUser(osuUser)) {
        switch(osuUser.status) {
          case OsuUserStatus.BOT:
            await channel.send("**Error:** Suddenly, you turned into a skynet...");
            break;
          case OsuUserStatus.DELETED: // fallthrough
          case OsuUserStatus.NOT_FOUND:
            await channel.send("**Error:** Did you do something to your osu! account?");
            break;
        }

        return;
      }
    }
    catch (e) {
      if(e instanceof NotFoundError) {
        await channel.send("**Error:** osu! user not found.");
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return;
    }

    const osuUsername = osuUser.user.userName;
    let points = 0;

    if(!Environment.useRespektive()) {
      const topCounts = await UserData.fetchOsuStats(channel, osuUser.user.userName);
      if(topCounts === null) {
        return;
      }

      points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
      await this.countPoints(client, channel, osuUsername, topCounts);
    }
    else {
      const topCounts = await UserData.fetchRespektiveOsuStats(channel, user.osuId);
      if(topCounts === null) {
        return;
      }

      points = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
      await this.countRespektivePoints(client, channel, osuUsername, topCounts);
    }

    await UserData.updateUserData(osuToken, client, channel, user.osuId, points);
  }

  // @[BOT_NAME] whatif [what-if expression]
  /**
   * Sends user's points in the specified what-if situation.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Channel to send points result to.
   * @param { string } osuToken osu! API token.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async userWhatIfCount(client: Client, channel: TextChannel, osuToken: string, message: Message): Promise<void> {
    let serverData;
    let user;
    let osuUser;

    try {
      serverData = await DatabaseWrapper.getInstance().getServersModule().getServerByDiscordId(channel.guild.id);
    }
    catch (e) {
      if(e instanceof ServerNotFoundError) {
        Log.error("userWhatIfCount", `Server with ID ${ channel.guild.id } not found in database.`);
        await channel.send("**Error:** Server not in database.");
      }
      else {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
      }

      return;
    }

    {
      let isCommand;

      try {
        isCommand = await DatabaseWrapper.getInstance().getServersModule().isCommandChannel(channel.guild.id, channel.id);
      }
      catch (e) {
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        return;
      }

      switch(isCommand) {
        case false:
          Log.warn("userLeaderboardsCountFromBathbot", `${ channel.guild.id }: Not in commands channel.`); // fallthrough
        case null:
          return;
      }
    }

    const commands = message.content.split(/\s+/g); // split by one or more spaces
    commands.splice(0, 2); // remove first two elements, which is the mentioned bot and the command itself

    if(commands.length <= 0) {
      await channel.send("**Error:** You need to specify what-if expression.");
      return;
    }

    // TODO: add other users specification

    const whatIfsArray: number[][] = [];
    {
      const len = commands.length;
      let current = 0;

      try {
        while(current < len) {
          whatIfsArray.push(Parser.parseWhatIfCount(commands[current]));
          current++;
        }
      }
      catch (e) {
        if(e instanceof InvalidExpressionError || e instanceof InvalidTypeError) {
          await channel.send(`**Error:** Invalid what if expression${ len > 1 ? "s" : "" } [at command index ${ current + 2 }].`);
        }
        else if(e instanceof InvalidTopRankError) {
          await channel.send(`**Error:** Top rank must be higher than or equal to 1 [at command index ${ current + 2 }].`);
        }
        else if(e instanceof InvalidNumberOfRanksError) {
          await channel.send(`**Error:** Number of ranks must be higher than or equal to 0 [at command index ${ current + 2 }].`);
        }
        else if(e instanceof Error) {
          Log.error("userWhatIfCount", `Unhandled error occurred while processing command.\n${ e.stack }`);
          await channel.send(`**Error:** An error occurred [at command index ${ current + 2 }].`);
        }

        return;
      }
    }

    const tops = [ 1, 8, 15, 25, 50 ]; // match bathbot <osc top ranks data

    let valid = true;
    whatIfsArray.forEach(whatif => {
      if(!tops.includes(whatif[0])) {
        valid = false;
      }
    });

    if(!valid) {
      await channel.send("**Error:** Rank query must be 1, 8, 15, 25, or 50.");
      return;
    }

    try {
      user = await DatabaseWrapper.getInstance().getUsersModule().getDiscordUserByDiscordId(message.author.id);
    }
    catch (e) {
      if(e instanceof UserNotFoundError) {
        await channel.send(`**Error:** You haven't linked your account. Link using \`${ client.user?.username } [osu! user ID]\`${ serverData.verifyChannelId !== null ? ` in <#${ serverData.verifyChannelId }> channel` : "" }.`);
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return;
    }

    try {
      osuUser = await getUserByOsuId(osuToken, user.osuId);
    }
    catch (e) {
      if(e instanceof NotFoundError) {
        await channel.send("**Error:** osu! user not found.");
      }
      else {
        await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
      }

      return;
    }

    {
      if(!isOsuUser(osuUser)) {
        switch(osuUser.status) {
          case OsuUserStatus.BOT:
            await channel.send("**Error:** Suddenly, you turned into a skynet...");
            break;
          case OsuUserStatus.DELETED: // fallthrough
          case OsuUserStatus.NOT_FOUND:
            await channel.send("**Error:** Did you do something to your osu! account?");
            break;
        }

        return;
      }
    }

    const osuUsername = osuUser.user.userName as string;

    Log.info("userWhatIfCount", `Calculating what-ifs for user: ${ osuUsername }`);

    const topCounts: number[] = [];
    let originalPoints = 0;

    if(!Environment.useRespektive()) {
      const topCounts = await UserData.fetchOsuStats(channel, osuUser.user.userName);
      if(topCounts === null) {
        return;
      }

      originalPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
      await this.countPoints(client, channel, osuUsername, topCounts);
    }
    else {
      const topCounts = await UserData.fetchRespektiveOsuStats(channel, user.osuId);
      if(topCounts === null) {
        return;
      }

      originalPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
      await this.countRespektivePoints(client, channel, osuUsername, topCounts);
    }

    if(!Environment.useRespektive()) {
      originalPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
    }
    else {
      originalPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
    }

    {
      let error = false;

      const len = whatIfsArray.length;
      for(let i = 0; i < len; i++) {
        const topIndex = tops.findIndex(top => top === whatIfsArray[i][0]);
        if(topIndex < 0) { // top count index not found
          error = true;
          break;
        }

        topCounts[topIndex] = whatIfsArray[i][1];
      }

      if(error) {
        await channel.send("**Error:** Rank query must be 1, 8, 15, 25, or 50.");
        return;
      }
    }

    let newPoints = 0;
    if(!Environment.useRespektive()) {
      if(topCounts.length !== 5) {
        Log.error("userWhatIfCount", "Invalid number of elements for osu!Stats' what-if count.");
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        return;
      }

      newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
      await this.countPoints(client, channel, osuUsername, topCounts as [ number, number, number, number, number ]);
    }
    else {
      if(topCounts.length !== 4) {
        Log.error("userWhatIfCount", "Invalid number of elements for osu!Stats (respektive)'s what-if count.");
        await channel.send("**Error:** An error occurred. Please contact bot administrator.");
        return;
      }

      newPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
      await this.countRespektivePoints(client, channel, osuUsername, topCounts as [ number, number, number, number ]);
    }

    const difference = newPoints - originalPoints;
    if(difference === 0) {
      await channel.send(`<@${ message.author.id }> would increase nothing!`);
      return;
    }

    await channel.send(`<@${ message.author.id }> would **${ difference > 0 ? "increase" : "decrease" } ${ Math.abs(difference) }** points from current top count.`);
  }

  /**
   * Sends calculated points and embed to specified channel.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { string } username osu! username.
   * @param { [ number, number, number, number, number ] } topCounts Array of top counts.
   *
   * @returns { Promise<Message> } Promise object with `Discord.Message` sent message object.
   */
  static async countPoints(client: Client, channel: TextChannel, username: string, topCounts: [ number, number, number, number, number ]): Promise<Message> {
    Log.info("countPoints", `Calculating points for username: ${ username }`);

    const newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
    const draft = counter(
      topCounts[0],
      topCounts[1],
      topCounts[2],
      topCounts[3],
      topCounts[4],
      username
    );

    const ret = await channel.send({ embeds: [ draft ] });
    await Reactions.addWysiReaction(client, ret, topCounts, newPoints);
    return ret;
  }

  /**
   * Sends respektive API's calculated points and embed to specified channel.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Discord channel to send message to.
   * @param { string } username osu! username.
   * @param { [ number, number, number, number ] } topCounts Array of top counts.
   *
   * @returns { Promise<Message> } Promise object with `Discord.Message` sent message object.
   */
  static async countRespektivePoints(client: Client, channel: TextChannel, username: string, topCounts: [ number, number, number, number ]): Promise<Message> {
    Log.info("countRespektivePoints", `Calculating points for username: ${ username }`);

    const newPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
    const draft = counterRespektive(
      topCounts[0],
      topCounts[1],
      topCounts[2],
      topCounts[3],
      username
    );

    const ret = await channel.send({ embeds: [ draft ] });
    await Reactions.addWysiReaction(client, ret, topCounts, newPoints);
    return ret;
  }
}

export default Count;
