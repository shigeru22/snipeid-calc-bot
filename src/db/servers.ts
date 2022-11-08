import { Pool, DatabaseError } from "pg";
import { DatabaseSuccess, DatabaseErrors } from "../utils/common";
import { LogSeverity, log } from "../utils/log";
import { DBResponseBase } from "../types/db/main";
import { IDBServerQueryData, IDBServerData } from "../types/db/servers";

/**
 * Gets all server data from the database.
 *
 * @param { Pool } db Database connection pool.
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
          log(LogSeverity.ERROR, "getAllServers", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAllServers", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
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
 * @param { Pool } db Database connection pool.
 * @param { string } serverDiscordId Server snowflake ID.
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
          log(LogSeverity.ERROR, "getServerByDiscordId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getServerByDiscordId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getServerByDiscordId", "Unknown error occurred.");
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

    log(LogSeverity.LOG, "insertServer", "servers: Inserted 1 row.");
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
          log(LogSeverity.ERROR, "insertServer", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "insertServer", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "insertServer", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

async function setServerCountry(db: Pool, serverDiscordId: string, countryCode: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const updateQuery = `
    UPDATE servers
    SET country = ${ countryCode !== null ? "$1" : "NULL" }
    WHERE discordId = ${ countryCode !== null ? "$2" : "$1" }
  `;
  const updateValues = countryCode !== null ? [ countryCode, serverDiscordId ] : [ serverDiscordId ];

  try {
    await db.query(updateQuery, updateValues);

    log(LogSeverity.LOG, "setServerCountry", "servers: Updated 1 row.");
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "setServerCountry", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "setServerCountry", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "setServerCountry", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "setServerCountry", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

async function setVerifiedRoleId(db: Pool, serverDiscordId: string, verifiedRoleId: string | null) {
  const updateQuery = `
    UPDATE servers
    SET verifiedRoleId = ${ verifiedRoleId !== null ? "$1" : "NULL" }
    WHERE discordId = ${ verifiedRoleId !== null ? "$2" : "$1" }
  `;
  const updateValues = verifiedRoleId !== null ? [ verifiedRoleId, serverDiscordId ] : [ serverDiscordId ];

  try {
    await db.query(updateQuery, updateValues);

    log(LogSeverity.LOG, "setVerifiedRoleId", "servers: Updated 1 row.");
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "setVerifiedRoleId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "setVerifiedRoleId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "setVerifiedRoleId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "setVerifiedRoleId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

async function setCommandsChannelId(db: Pool, serverDiscordId: string, commandsChannelId: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const updateQuery = `
    UPDATE servers
    SET commandsChannelId = ${ commandsChannelId !== null ? "$1" : "NULL" }
    WHERE discordId = ${ commandsChannelId !== null ? "$2" : "$1" }
  `;
  const updateValues = commandsChannelId !== null ? [ commandsChannelId, serverDiscordId ] : [ serverDiscordId ];

  try {
    await db.query(updateQuery, updateValues);

    log(LogSeverity.LOG, "setServerCountry", "servers: Updated 1 row.");
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "setCommandsChannelId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "setCommandsChannelId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "setCommandsChannelId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "setCommandsChannelId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

async function setLeaderboardsChannelId(db: Pool, serverDiscordId: string, leaderboardsChannelId: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const updateQuery = `
    UPDATE servers
    SET leaderboardsChannelId = ${ leaderboardsChannelId !== null ? "$1" : "NULL" }
    WHERE discordId = ${ leaderboardsChannelId !== null ? "$2" : "$1" }
  `;
  const updateValues = leaderboardsChannelId !== null ? [ leaderboardsChannelId, serverDiscordId ] : [ serverDiscordId ];

  try {
    await db.query(updateQuery, updateValues);

    log(LogSeverity.LOG, "setLeaderboardsChannelId", "servers: Updated 1 row.");
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "setLeaderboardsChannelId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "setLeaderboardsChannelId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "setLeaderboardsChannelId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "setLeaderboardsChannelId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

async function isCommandChannel(db: Pool, serverDiscordId: string, channelId: string): Promise<boolean | null> {
  const serverData = await getServerByDiscordId(db, serverDiscordId);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.ERROR, "isCommandChannel", "An error occurred while querying server in database.");
    return null;
  }

  if(serverData.data.commandsChannelId !== null && channelId !== serverData.data.commandsChannelId) {
    return false;
  }

  return true;
}

async function isLeaderboardChannel(db: Pool, serverDiscordId: string, channelId: string): Promise<boolean | null> {
  const serverData = await getServerByDiscordId(db, serverDiscordId);

  if(serverData.status !== DatabaseSuccess.OK) {
    log(LogSeverity.ERROR, "isLeaderboardChannel", "An error occurred while querying server in database.");
    return null;
  }

  if(serverData.data.leaderboardsChannelId !== null && channelId !== serverData.data.leaderboardsChannelId) {
    return false;
  }

  return true;
}

export { getAllServers, getServerByDiscordId, insertServer, setServerCountry, setVerifiedRoleId, setCommandsChannelId, setLeaderboardsChannelId, isCommandChannel, isLeaderboardChannel };
