import { Client, TextChannel, Message } from "discord.js";
import { Pool } from "pg";
import { getUserByOsuId } from "../api/osu";
import { getTopCounts, getTopCountsFromRespektive } from "../api/osustats";
import { DBUsers, DBServers } from "../db";
import Reactions from "./reactions";
import UserData from "./userdata";
import { calculatePoints, calculateRespektivePoints, counter, counterRespektive } from "../messages/counter";
import { WhatIfParserStatus, Parser, Environment } from "../utils";
import { Log } from "../utils/log";
import { UserNotFoundError, ServerNotFoundError } from "../errors/db";
import { NonOKError, NotFoundError } from "../errors/api";
import { isOsuUser } from "../types/api/osu";
import { OsuUserStatus } from "../utils/common";

class Count {// <osc, using Bathbot message response
  /**
   * Sends calculated points from Bathbot `<osc` command.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Channel to send points result to.
   * @param { Pool } db Database connection pool.
   * @param { string } osuToken osu! API token.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async userLeaderboardsCountFromBathbot(client: Client, channel: TextChannel, db: Pool, osuToken: string, message: Message): Promise<void> {
    {
      let isCommand;

      try {
        isCommand = await DBServers.isCommandChannel(db, channel.guild.id, channel.id);
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

    // [ top_1, top_8, top_15, top_25, top_50 ]
    const topCounts = Parser.parseTopCountDescription(desc);
    const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
    await this.countPoints(client, channel, username, topCounts);

    await UserData.updateUserData(osuToken, client, channel, db, osuId, points);
  }

  // @[BOT_NAME] count
  /**
   * Sends top leaderboard count to specified channel.
   * Basically, this is Bathbot's `<osc` command.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Channel to send points result to.
   * @param { Pool } db Database connection pool.
   * @param { string } osuToken osu! API token.
   * @param { string } discordId Discord ID of the user who sent the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async userLeaderboardsCount(client: Client, channel: TextChannel, db: Pool, osuToken: string, discordId: string): Promise<void> {
    let serverData;
    let user;
    let osuUser;

    try {
      serverData = await DBServers.getServerByDiscordId(db, channel.guild.id);
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
        isCommand = await DBServers.isCommandChannel(db, channel.guild.id, channel.id);
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
      user = await DBUsers.getDiscordUserByDiscordId(db, discordId);
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

    const topCounts: number[] = [];
    if(!Environment.useRespektive()) { // TODO: refactor for userWhatIfCount function usage
      const topCountsRequest = [
        getTopCounts(osuUsername, 1),
        getTopCounts(osuUsername, 8),
        getTopCounts(osuUsername, 15),
        getTopCounts(osuUsername, 25),
        getTopCounts(osuUsername, 50)
      ];

      let tempResponse;

      try {
        tempResponse = await Promise.all(topCountsRequest);
      }
      catch (e) {
        if(e instanceof NotFoundError) {
          await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
        }
        else if(e instanceof NonOKError) {
          await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }

      topCounts.push(
        tempResponse[0].count,
        tempResponse[1].count,
        tempResponse[2].count,
        tempResponse[3].count,
        tempResponse[4].count
      );
    }
    else {
      let tempResponse;

      try {
        tempResponse = await getTopCountsFromRespektive(user.osuId);
      }
      catch (e) {
        if(e instanceof NotFoundError) {
          await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
        }
        else if(e instanceof NonOKError) {
          await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }

      topCounts.push(...tempResponse);
    }

    let points = 0;
    if(!Environment.useRespektive()) {
      points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
      await this.countPoints(client, channel, osuUsername, topCounts);
    }
    else {
      points = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);
      await this.countRespektivePoints(client, channel, osuUsername, topCounts);
    }

    await UserData.updateUserData(osuToken, client, channel, db, user.osuId, points);
  }

  // @[BOT_NAME] whatif [what-if expression]
  /**
   * Sends user's points in the specified what-if situation.
   *
   * @param { Client } client Discord bot client.
   * @param { TextChannel } channel Channel to send points result to.
   * @param { Pool } db Database connection pool.
   * @param { string } osuToken osu! API token.
   * @param { Message } message Message that triggered the command.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  static async userWhatIfCount(client: Client, channel: TextChannel, db: Pool, osuToken: string, message: Message): Promise<void> {
    let serverData;
    let user;
    let osuUser;

    try {
      serverData = await DBServers.getServerByDiscordId(db, channel.guild.id);
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
        isCommand = await DBServers.isCommandChannel(db, channel.guild.id, channel.id);
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
      let status = WhatIfParserStatus.OK;
      let errorIndex = -1;

      const len = commands.length;
      for(let i = 0; i < len; i++) {
        const temp = Parser.parseWhatIfCount(commands[i]);
        if(typeof(temp) === "number") {
          status = temp;
          errorIndex = i;
          break;
        }

        whatIfsArray.push(temp);
      }

      if(status > WhatIfParserStatus.OK) {
        switch(status) {
          case WhatIfParserStatus.INVALID_EXPRESSION: // fallthrough
          case WhatIfParserStatus.TYPE_ERROR:
            await channel.send(`**Error:** Invalid what if expression${ len > 1 ? "s" : "" } [at command index ${ errorIndex + 2 }].`);
            return;
          case WhatIfParserStatus.TOP_RANK_ERROR:
            await channel.send(`**Error:** Top rank must be higher than or equal to 1 [at command index ${ errorIndex + 2 }].`);
            return;
          case WhatIfParserStatus.NUMBER_OF_RANKS_ERROR:
            await channel.send(`**Error:** Number of ranks must be higher than or equal to 0 [at command index ${ errorIndex + 2 }].`);
            return;
          default:
            await channel.send("**Error:** Unhandled error occurred. Please contact bot administrator.");
            return;
        }
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
      user = await DBUsers.getDiscordUserByDiscordId(db, message.author.id);
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
    if(!Environment.useRespektive()) {
      {
        const topCountsRequest = [
          getTopCounts(osuUsername, 1),
          getTopCounts(osuUsername, 8),
          getTopCounts(osuUsername, 15),
          getTopCounts(osuUsername, 25),
          getTopCounts(osuUsername, 50)
        ];

        let tempResponse;

        try {
          tempResponse = await Promise.all(topCountsRequest);
        }
        catch (e) {
          if(e instanceof NotFoundError) {
            await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
          }
          else if(e instanceof NonOKError) {
            await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
          }
          else {
            await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
          }

          return;
        }

        topCounts.push(
          tempResponse[0].count,
          tempResponse[1].count,
          tempResponse[2].count,
          tempResponse[3].count,
          tempResponse[4].count
        );
      }
    }
    else {
      let tempResponse;

      try {
        tempResponse = await getTopCountsFromRespektive(user.osuId);
      }
      catch (e) {
        if(e instanceof NotFoundError) {
          await channel.send("**Error:** osu!Stats API said you're not found. Check osu!Stats manually?");
        }
        else if(e instanceof NonOKError) {
          await channel.send("**Error:** osu!Stats API error occurred. Please contact bot administrator.");
        }
        else {
          await channel.send("**Error:** Client error occurred. Please contact bot administrator.");
        }

        return;
      }

      topCounts.push(...tempResponse);
    }

    let originalPoints = 0;
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
      newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);

      await this.countPoints(client, channel, osuUsername, topCounts);
    }
    else {
      newPoints = calculateRespektivePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3]);

      await this.countRespektivePoints(client, channel, osuUsername, topCounts);
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
   * @param { number[] } topCounts Array of top counts.
   *
   * @returns { Promise<Message> } Promise object with `Discord.Message` sent message object.
   */
  static async countPoints(client: Client, channel: TextChannel, username: string, topCounts: number[]): Promise<Message> {
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
   * @param { number[] } topCounts Array of top counts.
   *
   * @returns { Promise<Message> } Promise object with `Discord.Message` sent message object.
   */
  static async countRespektivePoints(client: Client, channel: TextChannel, username: string, topCounts: number[]): Promise<Message> {
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
