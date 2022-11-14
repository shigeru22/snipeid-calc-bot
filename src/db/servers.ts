import { PoolConfig, DatabaseError, Pool } from "pg";
import DBConnectorBase from "./db-base";
import { Log } from "../utils/log";
import { DatabaseErrors, DuplicatedRecordError, ServerNotFoundError, NoRecordError, ConflictError, DatabaseConnectionError, DatabaseClientError } from "../errors/db";
import { IDBServerQueryData, IDBServerData } from "../types/db/servers";

/**
 * Database `servers` table class.
 */
class DBServers extends DBConnectorBase {
  constructor(config: PoolConfig) {
    super(config);
  }

  /**
   * Gets all server data from the database.
   *
   * @returns { Promise<IDBServerData[]> } Promise object with server data array.
   *
   * @throws { NoRecordError } No server data found in database.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async getAllServers(): Promise<IDBServerData[]> {
    const selectQuery = `
      SELECT
        servers."serverid",
        servers."discordid",
        servers."country",
        servers."verifychannelid",
        servers."verifiedroleid",
        servers."commandschannelid",
        servers."leaderboardschannelid"
      FROM
        servers
    `;

    let serversDataResult;

    try {
      serversDataResult = await super.getPool().query<IDBServerQueryData>(selectQuery);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAllServers", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getAllServers", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAllServers", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getAllServers", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(serversDataResult.rows.length <= 0) {
      throw new NoRecordError();
    }

    return serversDataResult.rows.map(row => ({
      serverId: row.serverid,
      discordId: row.discordid,
      country: row.country,
      verifyChannelId: row.verifychannelid,
      verifiedRoleId: row.verifiedroleid,
      commandsChannelId: row.commandschannelid,
      leaderboardsChannelId: row.leaderboardschannelid
    }));
  }

  /**
   * Gets server data from the database by Discord ID.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<IDBServerData> } Promise object with server data.
   *
   * @throws { ServerNotFoundError } Server not found in database.
   * @throws { DuplicatedDiscordIdError } Duplicated record found in `discordId` column at `servers` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async getServerByDiscordId(serverDiscordId: string): Promise<IDBServerData> {
    const selectQuery = `
      SELECT
        servers."serverid",
        servers."discordid",
        servers."country",
        servers."verifychannelid",
        servers."verifiedroleid",
        servers."commandschannelid",
        servers."leaderboardschannelid"
      FROM
        servers
      WHERE
        servers."discordid" = $1
    `;
    const selectValues = [ serverDiscordId ];

    let serverDataResult;

    try {
      serverDataResult = await super.getPool().query<IDBServerQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerByDiscordId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getServerByDiscordId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerByDiscordId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getServerByDiscordId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(serverDataResult.rows.length <= 0) {
      throw new ServerNotFoundError();
    }
    else if(serverDataResult.rows.length > 1) {
      throw new DuplicatedRecordError("servers", "discordId");
    }

    return {
      serverId: serverDataResult.rows[0].serverid,
      discordId: serverDataResult.rows[0].discordid,
      country: serverDataResult.rows[0].country !== null ? serverDataResult.rows[0].country.toUpperCase() : null,
      verifyChannelId: serverDataResult.rows[0].verifychannelid,
      verifiedRoleId: serverDataResult.rows[0].verifiedroleid,
      commandsChannelId: serverDataResult.rows[0].commandschannelid,
      leaderboardsChannelId: serverDataResult.rows[0].leaderboardschannelid
    };
  }

  /**
   * Inserts server data into the database.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DuplicatedDiscordIdError } Duplicated Discord server ID found in database.
   * @throws { ConflictError } Specified Discord server ID already in database.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async insertServer(serverDiscordId: string): Promise<void> {
    const insertQuery = `
      INSERT INTO servers (discordId)
        VALUES ($1)
    `;
    const insertValues = [ serverDiscordId ];

    {
      let serverData;

      try {
        serverData = await this.getServerByDiscordId(serverDiscordId);
      }
      catch (e) {
        if(e instanceof DatabaseErrors) {
          if(!(e instanceof ServerNotFoundError)) {
            throw e;
          }
        }
        else if(e instanceof Error) {
          Log.error("insertServer", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
        }
        else {
          Log.error("insertServer", "Unknown error occurred.");
        }
      }

      if(serverData !== undefined && serverData.discordId === serverDiscordId) {
        throw new ConflictError("servers", "discordId");
      }
    }

    try {
      await super.getPool().query(insertQuery, insertValues);
      Log.info("insertServer", "servers: Inserted 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("insertServer", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("insertServer", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("insertServer", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("insertServer", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Updates server's country restriction configuration in the database.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string | null } countryCode Country code to be set. Set to `null` to disable.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async setServerCountry(serverDiscordId: string, countryCode: string | null): Promise<void> {
    const updateQuery = `
      UPDATE servers
        SET country = ${ countryCode !== null ? "$1" : "NULL" }
        WHERE discordId = ${ countryCode !== null ? "$2" : "$1" }
    `;
    const updateValues = countryCode !== null ? [ countryCode, serverDiscordId ] : [ serverDiscordId ];

    try {
      await super.getPool().query(updateQuery, updateValues);
      Log.info("setServerCountry", "servers: Updated 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setServerCountry", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("setServerCountry", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("setServerCountry", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("setServerCountry", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Updates server's verified role ID configuration in the database.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string | null } verifiedRoleId Verified role ID to be set. Set to `null` to disable.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async setVerifiedRoleId(serverDiscordId: string, verifiedRoleId: string | null): Promise<void> {
    const updateQuery = `
      UPDATE servers
        SET verifiedRoleId = ${ verifiedRoleId !== null ? "$1" : "NULL" }
        WHERE discordId = ${ verifiedRoleId !== null ? "$2" : "$1" }
    `;
    const updateValues = verifiedRoleId !== null ? [ verifiedRoleId, serverDiscordId ] : [ serverDiscordId ];

    try {
      await super.getPool().query(updateQuery, updateValues);
      Log.info("setVerifiedRoleId", "servers: Updated 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setVerifiedRoleId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("setVerifiedRoleId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("setVerifiedRoleId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("setVerifiedRoleId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Updates server's commands channel ID restriction configuration in the database.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string | null } commandsChannelId Commands channel ID to be set. Set to `null` to disable.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async setCommandsChannelId(serverDiscordId: string, commandsChannelId: string | null): Promise<void> {
    const updateQuery = `
      UPDATE servers
        SET commandsChannelId = ${ commandsChannelId !== null ? "$1" : "NULL" }
        WHERE discordId = ${ commandsChannelId !== null ? "$2" : "$1" }
    `;
    const updateValues = commandsChannelId !== null ? [ commandsChannelId, serverDiscordId ] : [ serverDiscordId ];

    try {
      await super.getPool().query(updateQuery, updateValues);
      Log.info("setServerCountry", "servers: Updated 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setCommandsChannelId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("setCommandsChannelId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("setCommandsChannelId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("setCommandsChannelId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Updates server's leaderboards channel ID restriction configuration in the database.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string | null } leaderboardsChannelId Leaderboards channel ID to be set. Set to `null` to disable.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async setLeaderboardsChannelId(serverDiscordId: string, leaderboardsChannelId: string | null): Promise<void> {
    const updateQuery = `
      UPDATE servers
        SET leaderboardsChannelId = ${ leaderboardsChannelId !== null ? "$1" : "NULL" }
        WHERE discordId = ${ leaderboardsChannelId !== null ? "$2" : "$1" }
    `;
    const updateValues = leaderboardsChannelId !== null ? [ leaderboardsChannelId, serverDiscordId ] : [ serverDiscordId ];

    try {
      await super.getPool().query(updateQuery, updateValues);
      Log.info("setLeaderboardsChannelId", "servers: Updated 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setLeaderboardsChannelId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("setLeaderboardsChannelId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("setLeaderboardsChannelId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("setLeaderboardsChannelId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Whether specified `channelId` is server's restricted commands channel.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string } channelId Channel snowflake ID.
   *
   * @returns { Promise<boolean> } Promise object with `true` if specified `channelId` is server's restricted commands channel (also if configuration is disabled (`null`)). `false` otherwise.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async isCommandChannel(serverDiscordId: string, channelId: string): Promise<boolean> {
    try {
      const serverData = await this.getServerByDiscordId(serverDiscordId);
      return serverData.commandsChannelId !== null && channelId !== serverData.commandsChannelId;
    }
    catch (e) {
      if(e instanceof DatabaseErrors) {
        throw e;
      }
      else {
        Log.error("isCommandChannel", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Whether specified `channelId` is server's restricted leaderboard commands channel.
   *
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string } channelId Channel snowflake ID.
   *
   * @returns { Promise<boolean> } Promise object with `true` if specified `channelId` is server's restricted leaderboard commands channel (also if configuration is disabled (`null`)). `false` otherwise.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  async isLeaderboardChannel(serverDiscordId: string, channelId: string): Promise<boolean | null> {
    try {
      const serverData = await this.getServerByDiscordId(serverDiscordId);
      return serverData.commandsChannelId !== null && channelId !== serverData.commandsChannelId;
    }
    catch (e) {
      if(e instanceof DatabaseErrors) {
        throw e;
      }
      else {
        Log.error("isLeaderboardChannel", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /* static query functions */

  /**
   * Gets server data from the database by Discord ID.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<IDBServerData> } Promise object with server data.
   *
   * @throws { ServerNotFoundError } Server not found in database.
   * @throws { DuplicatedDiscordIdError } Duplicated record found in `discordId` column at `servers` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getServerByDiscordId(db: Pool, serverDiscordId: string): Promise<IDBServerData> {
    const selectQuery = `
      SELECT
        servers."serverid",
        servers."discordid",
        servers."country",
        servers."verifychannelid",
        servers."verifiedroleid",
        servers."commandschannelid",
        servers."leaderboardschannelid"
      FROM
        servers
      WHERE
        servers."discordid" = $1
    `;
    const selectValues = [ serverDiscordId ];

    let serverDataResult;

    try {
      serverDataResult = await db.query<IDBServerQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerByDiscordId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getServerByDiscordId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerByDiscordId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getServerByDiscordId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(serverDataResult.rows.length <= 0) {
      throw new ServerNotFoundError();
    }
    else if(serverDataResult.rows.length > 1) {
      throw new DuplicatedRecordError("servers", "discordId");
    }

    return {
      serverId: serverDataResult.rows[0].serverid,
      discordId: serverDataResult.rows[0].discordid,
      country: serverDataResult.rows[0].country !== null ? serverDataResult.rows[0].country.toUpperCase() : null,
      verifyChannelId: serverDataResult.rows[0].verifychannelid,
      verifiedRoleId: serverDataResult.rows[0].verifiedroleid,
      commandsChannelId: serverDataResult.rows[0].commandschannelid,
      leaderboardsChannelId: serverDataResult.rows[0].leaderboardschannelid
    };
  }
}

export default DBServers;
