import dotenv from "dotenv";
import fs from "fs";
import { createInterface } from "readline";
import { Client, GatewayIntentBits, ActivityType, ChannelType, Guild, GuildMember, Message, Interaction } from "discord.js";
import { PoolConfig } from "pg";
import { OsuToken } from "./api/osu-token";
import { DatabaseWrapper } from "./db";
import { handleCommands, Conversations, Roles, Servers } from "./commands";
import { handleInteractionCommands } from "./commands/interactions";
import { Environment } from "./utils";
import { Log } from "./utils/log";

// configure environment variable file (if any)
dotenv.config();

// database pool configuration
const dbConfig: PoolConfig = {
  host: process.env.DB_HOST,
  port: parseInt(process.env.DB_PORT as string, 10),
  user: process.env.DB_USERNAME,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE,
  ssl: Environment.getCAPath() !== null ? {
    rejectUnauthorized: true,
    ca: fs.readFileSync(Environment.getCAPath() as string).toString()
  } : undefined
};

// bot client
const client = new Client({ intents: [ GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages ] });

// osu! API token object
let token: OsuToken;

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
client.on("interactionCreate", async (interaction: Interaction) => await onInteraction(interaction));
client.on("guildCreate", async (guild: Guild) => await Servers.onJoinServer(guild));
client.on("guildDelete", (guild: Guild) => onLeaveGuild(guild));
client.on("guildMemberAdd", async (member: GuildMember) => await onMemberJoinGuild(member));

/**
 * Startup event function.
 */
async function onStartup() {
  Log.info("onStartup", "(1/4) Fetching osu!api token...");

  token = new OsuToken(Environment.getOsuClientId(), Environment.getOsuClientSecret());
  await token.getToken();

  Log.info("onStartup", "(2/4) Configuring database...");

  if(dbConfig.ssl !== undefined) {
    Log.info("onStartup", `Using SSL for database connection, CA path: ${ Environment.getCAPath() }`);
  }
  else {
    Log.warn("onStartup", "Not using SSL for database connection. Caute procedere.");
  }

  DatabaseWrapper.getInstance().setConfig(dbConfig);

  Log.info("onStartup", "(3/4) Testing database connection...");

  // test connection before continuing
  {
    try {
      const dbTemp = await DatabaseWrapper.getInstance()
        .getServersModule()
        .getPoolClient();

      await dbTemp.releasePoolClient();
    }
    catch (e) {
      if(e instanceof Error) {
        Log.error("onStartup", `Database connection error.\n${ e.stack }`);
      }
      else {
        Log.error("onStartup", `Unknown error occurred while connecting to database.\n${ e }`);
      }

      process.emit("SIGINT");
      return;
    }

    Log.info("onStartup", "Successfully connected to database.");
  }

  if(client.user === null) {
    // this should not happen, but whatever
    Log.error("onStartup", "client.user is null.");
    client.destroy();
    process.exit(1);
  }

  Log.info("onStartup", "(4/4) Setting bot activity message...");
  client.user.setActivity("Bathbot everyday", { type: ActivityType.Watching });

  Log.info("onStartup", `${ client.user.username } is now ready.`);
}

/**
 * New message event function.
 *
 * @param { Message } msg Message received by the client.
 */
async function onNewMessage(msg: Message) {
  if(client.user === null) {
    // this should not happen, but whatever
    Log.error("onNewMessage", "client.user is null.");
    client.destroy();
    process.exit(1);
  }

  const contents = msg.content.split(/\s+/g); // split by one or more spaces
  const isClientMentioned = msg.mentions.users.has(client.user.id) && contents[0].includes(client.user.id);
  let processed = false;

  const channel = msg.channel;
  if(channel.type === ChannelType.GuildText) {
    const tempToken = await token.getToken();
    if(tempToken === null) {
      await channel.send("**Error:** Unable to retrieve osu! client authorizations. Check osu!status?");
      return;
    }

    processed = await handleCommands(client, channel, tempToken, isClientMentioned, msg);

    // if bot is mentioned but nothing processed, send a random message.
    (!processed && isClientMentioned) && await Conversations.sendMessage(channel, contents);
  }
}

async function onInteraction(interaction: Interaction) {
  if(interaction.isCommand()) {
    await handleInteractionCommands(client, interaction);
  }
}

/**
 * Event function upon leaving guild.
 *
 * @param { Message } guild Entered guild object.
 */
function onLeaveGuild(guild: Guild) {
  Log.info("onJoinGuild", `Left server with ID ${ guild.id } (${ guild.name }).`);
}

async function onMemberJoinGuild(member: GuildMember) {
  Log.info("onMemberJoin", `${ member.user.username }#${ member.user.discriminator } joined server ID ${ member.guild.id } (${ member.guild.name })`);
  await Roles.reassignRole(member); // TODO: test usage
}

/**
 * Unhandled exception event function.
 *
 * @param { unknown } e Thrown error variable.
 */
function onException(e: unknown) {
  if(e instanceof Error) {
    if(e.name === "Error [TOKEN_INVALID]") {
      Log.error("onException", "Invalid token provided. Check token in .env file and restart the client.");
    }
    else {
      Log.error("onException", `Unhandled error occurred. Exception details below.\n${ e.stack }`);
    }
  }
  else {
    Log.error("onException", "Unknown error occurred.");
  }
}

/**
 * Exit event function.
 */
async function onExit() {
  Log.info("onExit", "Exit signal received. Cleaning up process...");

  await token.revokeToken();
  client.destroy();

  Log.info("onExit", "Cleanup success. Exiting...");

  process.exit(0);
}

(() => {
  if(!Environment.validateEnvironmentVariables()) {
    process.exit(1);
  }

  // main execution procedure
  client.login(process.env.BOT_TOKEN);
})();
