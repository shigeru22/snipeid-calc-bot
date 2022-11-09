import { Pool, DatabaseError } from "pg";
import { Log } from "../utils/log";
import { DatabaseSuccess, DatabaseErrors } from "../utils/common";
import { DBResponseBase } from "../types/db/main";
import { IDBServerQueryData, IDBServerData } from "../types/db/servers";

class DBServers {
  /**
   * Gets all server data from the database.
   *
   * @param { Pool } db Database connection pool.
   *
   * @returns { Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
   */
  static async getAllServers(db: Pool): Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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

    try {
      const result = await db.query<IDBServerQueryData>(selectQuery);

      if(result.rows.length <= 0) {
        return {
          status: DatabaseErrors.NO_RECORD
        };
      }

      return {
        status: DatabaseSuccess.OK,
        data: result.rows.map(row => ({
          serverId: row.serverid,
          discordId: row.discordid,
          country: row.country,
          verifyChannelId: row.verifychannelid,
          verifiedRoleId: row.verifiedroleid,
          commandsChannelId: row.commandschannelid,
          leaderboardsChannelId: row.leaderboardschannelid
        }))
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAllServers", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getAllServers", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAllServers", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getAllServers", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  /**
   * Gets server data from the database by Discord ID.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
   */
  static async getServerByDiscordId(db: Pool, serverDiscordId: string): Promise<DBResponseBase<IDBServerData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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

    try {
      const result = await db.query<IDBServerQueryData>(selectQuery, selectValues);

      if(result.rows.length <= 0) {
        return {
          status: DatabaseErrors.NO_RECORD
        };
      }
      else if(result.rows.length > 1) {
        return {
          status: DatabaseErrors.DUPLICATED_RECORD
        };
      }

      return {
        status: DatabaseSuccess.OK,
        data: {
          serverId: result.rows[0].serverid,
          discordId: result.rows[0].discordid,
          country: result.rows[0].country !== null ? result.rows[0].country.toUpperCase() : null,
          verifyChannelId: result.rows[0].verifychannelid,
          verifiedRoleId: result.rows[0].verifiedroleid,
          commandsChannelId: result.rows[0].commandschannelid,
          leaderboardsChannelId: result.rows[0].leaderboardschannelid
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerByDiscordId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getServerByDiscordId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerByDiscordId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getServerByDiscordId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  /**
   * Inserts server data into the database.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
   */
  static async insertServer(db: Pool, serverDiscordId: string): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.DUPLICATED_DISCORD_ID | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
    const insertQuery = `
      INSERT INTO servers (discordId)
        VALUES ($1)
    `;
    const insertValues = [ serverDiscordId ];

    try {
      {
        const currentData = await this.getServerByDiscordId(db, serverDiscordId);

        switch(currentData.status) {
          case DatabaseSuccess.OK:
            return {
              status: DatabaseErrors.DUPLICATED_DISCORD_ID
            };
          case DatabaseErrors.DUPLICATED_RECORD: // fallthrough
          case DatabaseErrors.CONNECTION_ERROR: // fallthrough
          case DatabaseErrors.CLIENT_ERROR:
            return {
              status: currentData.status
            };
        }
      }

      await db.query(insertQuery, insertValues);

      Log.info("insertServer", "servers: Inserted 1 row.");
      return {
        status: DatabaseSuccess.OK,
        data: true
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("insertServer", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("insertServer", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("insertServer", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("insertServer", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  static async setServerCountry(db: Pool, serverDiscordId: string, countryCode: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
    const updateQuery = `
      UPDATE servers
      SET country = ${ countryCode !== null ? "$1" : "NULL" }
      WHERE discordId = ${ countryCode !== null ? "$2" : "$1" }
    `;
    const updateValues = countryCode !== null ? [ countryCode, serverDiscordId ] : [ serverDiscordId ];

    try {
      await db.query(updateQuery, updateValues);

      Log.info("setServerCountry", "servers: Updated 1 row.");
      return {
        status: DatabaseSuccess.OK,
        data: true
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setServerCountry", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("setServerCountry", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("setServerCountry", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("setServerCountry", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  static async setVerifiedRoleId(db: Pool, serverDiscordId: string, verifiedRoleId: string | null) {
    const updateQuery = `
      UPDATE servers
      SET verifiedRoleId = ${ verifiedRoleId !== null ? "$1" : "NULL" }
      WHERE discordId = ${ verifiedRoleId !== null ? "$2" : "$1" }
    `;
    const updateValues = verifiedRoleId !== null ? [ verifiedRoleId, serverDiscordId ] : [ serverDiscordId ];

    try {
      await db.query(updateQuery, updateValues);

      Log.info("setVerifiedRoleId", "servers: Updated 1 row.");
      return {
        status: DatabaseSuccess.OK,
        data: true
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setVerifiedRoleId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("setVerifiedRoleId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("setVerifiedRoleId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("setVerifiedRoleId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  static async setCommandsChannelId(db: Pool, serverDiscordId: string, commandsChannelId: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
    const updateQuery = `
      UPDATE servers
      SET commandsChannelId = ${ commandsChannelId !== null ? "$1" : "NULL" }
      WHERE discordId = ${ commandsChannelId !== null ? "$2" : "$1" }
    `;
    const updateValues = commandsChannelId !== null ? [ commandsChannelId, serverDiscordId ] : [ serverDiscordId ];

    try {
      await db.query(updateQuery, updateValues);

      Log.info("setServerCountry", "servers: Updated 1 row.");
      return {
        status: DatabaseSuccess.OK,
        data: true
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setCommandsChannelId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("setCommandsChannelId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("setCommandsChannelId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("setCommandsChannelId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  static async setLeaderboardsChannelId(db: Pool, serverDiscordId: string, leaderboardsChannelId: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
    const updateQuery = `
      UPDATE servers
      SET leaderboardsChannelId = ${ leaderboardsChannelId !== null ? "$1" : "NULL" }
      WHERE discordId = ${ leaderboardsChannelId !== null ? "$2" : "$1" }
    `;
    const updateValues = leaderboardsChannelId !== null ? [ leaderboardsChannelId, serverDiscordId ] : [ serverDiscordId ];

    try {
      await db.query(updateQuery, updateValues);

      Log.info("setLeaderboardsChannelId", "servers: Updated 1 row.");
      return {
        status: DatabaseSuccess.OK,
        data: true
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("setLeaderboardsChannelId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("setLeaderboardsChannelId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("setLeaderboardsChannelId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("setLeaderboardsChannelId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  static async isCommandChannel(db: Pool, serverDiscordId: string, channelId: string): Promise<boolean | null> {
    const serverData = await this.getServerByDiscordId(db, serverDiscordId);

    if(serverData.status !== DatabaseSuccess.OK) {
      Log.error("isCommandChannel", "An error occurred while querying server in database.");
      return null;
    }

    if(serverData.data.commandsChannelId !== null && channelId !== serverData.data.commandsChannelId) {
      return false;
    }

    return true;
  }

  static async isLeaderboardChannel(db: Pool, serverDiscordId: string, channelId: string): Promise<boolean | null> {
    const serverData = await this.getServerByDiscordId(db, serverDiscordId);

    if(serverData.status !== DatabaseSuccess.OK) {
      Log.error("isLeaderboardChannel", "An error occurred while querying server in database.");
      return null;
    }

    if(serverData.data.leaderboardsChannelId !== null && channelId !== serverData.data.leaderboardsChannelId) {
      return false;
    }

    return true;
  }
}

export default DBServers;
