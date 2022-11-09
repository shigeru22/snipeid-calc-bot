import dotenv from "dotenv";
import fs from "fs";
import { Client, GatewayIntentBits, ActivityType, ChannelType, Guild, GuildMember, Message } from "discord.js";
import { Pool, PoolConfig } from "pg";
import { createInterface } from "readline";
import { OsuToken } from "./api/osu-token";
import { DBServers } from "./db";
import { handleCommands, Conversations, Roles } from "./commands";
import { Environment } from "./utils";
import { DatabaseErrors } from "./utils/common";
import { LogSeverity, log } from "./utils/log";

// configure environment variable file (if any)
dotenv.config();

if(!Environment.validateEnvironmentVariables()) {
  process.exit(1);
}

// database pool

const dbConfig: PoolConfig = {
  host: process.env.DB_HOST,
  port: parseInt(process.env.DB_PORT as string, 10),
  user: process.env.DB_USERNAME,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE,
  ssl: (typeof(process.env.DB_SSL_CA) !== "undefined" ? {
    rejectUnauthorized: true,
    ca: fs.readFileSync(process.env.DB_SSL_CA).toString()
  } : undefined)
};

const db = new Pool(dbConfig);

// bot client
const client = new Client({ intents: [ GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages ] });

// osu! API token object
const token = new OsuToken(process.env.OSU_CLIENT_ID as string, process.env.OSU_CLIENT_SECRET as string);

// handle Windows interrupt event
if(process.platform === "win32") {
  const rl = createInterface({
    input: process.stdin,
    output: process.stdout
  });

  rl.on("SIGINT", () => process.emit("SIGINT"));
}

// interrupt and termination signal handling
process.on("SIGINT", () => onExit());
process.on("SIGTERM", () => onExit());
process.on("uncaughtException", (e: unknown) => onException(e));

// bot client event handling
client.on("ready", async () => await onStartup());
client.on("messageCreate", async (msg: Message) => await onNewMessage(msg));
client.on("guildCreate", async (guild: Guild) => await onJoinGuild(guild));
client.on("guildDelete", (guild: Guild) => onLeaveGuild(guild));
client.on("guildMemberAdd", async (member: GuildMember) => await onMemberJoinGuild(member));

/**
 * Startup event function.
 */
async function onStartup() {
  await token.getToken();

  if(typeof(dbConfig.ssl) !== "undefined") {
    log(LogSeverity.LOG, "onStartup", `Using SSL for database connection, CA path: ${ process.env.DB_SSL_CA }`);
  }
  else {
    log(LogSeverity.WARN, "onStartup", "Not using SSL for database connection. Caute procedere.");
  }

  // test connection before continuing
  {
    log(LogSeverity.LOG, "onStartup", "Testing database connection...");

    try {
      const dbTemp = await db.connect();
      dbTemp.release();
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.ERROR, "onStartup", `Database connection error.\n${ e.stack }`);
      }
      else {
        log(LogSeverity.ERROR, "onStartup", `Unknown error occurred while connecting to database.\n${ e }`);
      }

      process.exit(1);
    }

    log(LogSeverity.LOG, "onStartup", "Successfully connected to database.");
  }

  if(client.user === null) {
    // this should not happen, but whatever
    log(LogSeverity.ERROR, "onStartup", "client.user is null.");
    client.destroy();
    process.exit(1);
  }

  client.user.setActivity("Bathbot everyday", { type: ActivityType.Watching });
  log(LogSeverity.LOG, "onStartup", process.env.BOT_NAME + " is now running.");
}

/**
 * New message event function.
 *
 * @param { Message } msg Message received by the client.
 */
async function onNewMessage(msg: Message) {
  if(client.user === null) {
    // this should not happen, but whatever
    log(LogSeverity.ERROR, "onNewMessage", "client.user is null.");
    client.destroy();
    process.exit(1);
  }

  const contents = msg.content.split(/\s+/g); // split by one or more spaces
  const isClientMentioned = msg.mentions.users.has(client.user.id) && contents[0].includes(client.user.id);
  let processed = false;

  const channel = msg.channel;
  if(channel.type === ChannelType.GuildText) {
    const tempToken = await token.getToken();
    if(tempToken === "") {
      await channel.send("**Error:** Unable to retrieve osu! client authorizations. Check osu!status?");
      return;
    }

    processed = await handleCommands(client, channel, db, tempToken, isClientMentioned, msg);

    // if bot is mentioned but nothing processed, send a random message.
    (!processed && isClientMentioned) && await Conversations.sendMessage(channel, contents);
  }
}

/**
 * Event function upon joining guild.
 *
 * @param { Message } guild Entered guild object.
 */
async function onJoinGuild(guild: Guild) {
  const result = await DBServers.insertServer(db, guild.id);
  switch(result.status) {
    case DatabaseErrors.DUPLICATED_RECORD:
      log(LogSeverity.ERROR, "onJoinGuild", `Duplicated server data found with server ID ${ guild.id } (${ guild.name }).`);
      return;
    case DatabaseErrors.CONNECTION_ERROR: // fallthrough
    case DatabaseErrors.CLIENT_ERROR: // TODO: handle insertion by queuing
      log(LogSeverity.ERROR, "onJoinGuild", `Failed to query database after joining server ID ${ guild.id } (${ guild.name }).`);
      return;
  }

  if(result.status === DatabaseErrors.DUPLICATED_DISCORD_ID) {
    log(LogSeverity.LOG, "onJoinGuild", `Rejoined server with ID ${ guild.id } (${ guild.name }).`);
    return;
  }

  log(LogSeverity.LOG, "onJoinGuild", `Joined server with ID ${ guild.id } (${ guild.name }).`);
}

/**
 * Event function upon leaving guild.
 *
 * @param { Message } guild Entered guild object.
 */
function onLeaveGuild(guild: Guild) {
  log(LogSeverity.LOG, "onJoinGuild", `Left server with ID ${ guild.id } (${ guild.name }).`);
}

async function onMemberJoinGuild(member: GuildMember) {
  log(LogSeverity.LOG, "onMemberJoin", `${ member.user.username }#${ member.user.discriminator } joined server ID ${ member.guild.id } (${ member.guild.name })`);
  await Roles.reassignRole(db, member); // TODO: test usage
}

/**
 * Unhandled exception event function.
 *
 * @param { unknown } e Thrown error variable.
 */
function onException(e: unknown) {
  if(e instanceof Error) {
    if(e.name === "Error [TOKEN_INVALID]") {
      log(LogSeverity.ERROR, "onException", "Invalid token provided. Check token in .env file and restart the client.");
    }
    else {
      log(LogSeverity.ERROR, "onException", `Unhandled error occurred. Exception details below.\n${ e.stack }`);
    }
  }
  else {
    log(LogSeverity.ERROR, "onException", "Unknown error occurred.");
  }
}

/**
 * Exit event function.
 */
async function onExit() {
  log(LogSeverity.LOG, "onExit", "Exit signal received. Cleaning up process...");

  await token.revokeToken();
  client.destroy();

  log(LogSeverity.LOG, "onExit", "Cleanup success. Exiting...");

  process.exit(0);
}

// main execution procedure
client.login(process.env.BOT_TOKEN);
