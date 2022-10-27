import { Pool, DatabaseError } from "pg";
import { DatabaseSuccess, DatabaseErrors } from "../utils/common";
import { LogSeverity, log } from "../utils/log";
import { DBResponseBase } from "../types/db/main";
import { IDBServerQueryData, IDBServerData } from "../types/db/servers";

/**
 * Gets all server data from the database.
 *
 * @param { Pool } db - Database connection pool.
 *
 * @returns { Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
 */
async function getAllServers(db: Pool): Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
          log(LogSeverity.ERROR, "getAllServers", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getAllServers", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAllServers", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getAllServers", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Gets server data from the database by Discord ID.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 *
 * @returns { Promise<DBResponseBase<IDBServerData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
 */
async function getServerByDiscordId(db: Pool, serverDiscordId: string): Promise<DBResponseBase<IDBServerData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
        country: result.rows[0].country,
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
          log(LogSeverity.ERROR, "getServerByDiscordId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getServerByDiscordId", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getServerByDiscordId", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getServerByDiscordId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

async function insertServer(db: Pool, serverDiscordId: string): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.DUPLICATED_DISCORD_ID | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const insertQuery = `
    INSERT INTO servers (discordId)
      VALUES ($1)
  `;
  const insertValues = [ serverDiscordId ];

  try {
    {
      const currentData = await getServerByDiscordId(db, serverDiscordId);

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
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "insertServer", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "insertServer", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "insertServer", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "insertServer", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

export { getAllServers, getServerByDiscordId, insertServer };
