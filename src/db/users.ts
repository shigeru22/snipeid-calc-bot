import { Pool, DatabaseError } from "pg";
import { LogSeverity, log } from "../utils/log";
import { DatabaseErrors, DatabaseSuccess } from "../utils/common";
import { DBResponseBase } from "../types/db/main";
import { IDBServerUserData, IDBServerLeaderboardQueryData, IDBServerUserQueryData, IDBServerLeaderboardData } from "../types/db/users";

/**
 * Gets Discord user by Discord ID from the database.
 *
 * @param { Pool } db Database connection pool.
 *
 * @returns { Promise<DBResponseBase<IDBServerUserData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data array.
 */
async function getAllUsers(db: Pool): Promise<DBResponseBase<IDBServerUserData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      users."userid",
      users."discordid",
      users."osuid",
      users."points",
      users."country"
    FROM
      users
  `;

  try {
    const result = await db.query<IDBServerUserQueryData>(selectQuery);
    if(result.rows.length <= 0) {
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: result.rows.map(row => ({
        userId: row.userid,
        discordId: row.discordid,
        osuId: row.osuid,
        points: row.points,
        country: row.country
      }))
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getDiscordUserById", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getDiscordUserById", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getDiscordUserById", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserById", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Gets Discord user by osu! ID from the database.
 *
 * @param { Pool } db Database connection pool.
 * @param { number } osuId osu! user ID.
 *
 * @returns { Promise<DBResponseBase<IDBServerUserData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
 */
async function getDiscordUserByOsuId(db: Pool, osuId: number): Promise<DBResponseBase<IDBServerUserData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      users."userid",
      users."discordid",
      users."osuid",
      users."points",
      users."country"
    FROM
      users
    WHERE
      users."osuid" = $1
  `;
  const selectValues = [ osuId ];

  try {
    const discordUserResult = await db.query<IDBServerUserQueryData>(selectQuery, selectValues);

    if(typeof(discordUserResult.rows[0]) === "undefined") {
      return {
        status: DatabaseErrors.USER_NOT_FOUND
      };
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Invalid row returned.");
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: {
        userId: discordUserResult.rows[0].userid,
        discordId: discordUserResult.rows[0].discordid,
        osuId: discordUserResult.rows[0].osuid,
        points: discordUserResult.rows[0].points,
        country: discordUserResult.rows[0].country
      }
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Gets Discord user by Discord ID from the database.
 *
 * @param { Pool } db Database connection pool.
 * @param { string } discordId Discord ID of the user.
 *
 * @returns { Promise<DBResponseBase<IDBServerUserData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with user data.
 */
async function getDiscordUserByDiscordId(db: Pool, discordId: string): Promise<DBResponseBase<IDBServerUserData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      users."userid",
      users."discordid",
      users."osuid",
      users."points",
      users."country"
    FROM
      users
    WHERE
      users."discordid" = $1
  `;
  const selectValues = [ discordId ];

  try {
    const result = await db.query<IDBServerUserQueryData>(selectQuery, selectValues);
    if(typeof(result.rows[0]) === "undefined") {
      return {
        status: DatabaseErrors.USER_NOT_FOUND
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: {
        userId: result.rows[0].userid,
        discordId: result.rows[0].discordid,
        osuId: result.rows[0].osuid,
        points: result.rows[0].points,
        country: result.rows[0].country
      }
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Retrieves points leaderboard by server ID.
 *
 * @param { Pool } db Database connection pool.
 * @param { string } serverDiscordId Server snowflake ID.
 * @param { boolean } desc Whether results should be sorted in descending order.
 */
async function getPointsLeaderboard(db: Pool, serverDiscordId: string, desc = true): Promise<DBResponseBase<IDBServerLeaderboardData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      users."userid",
      users."username",
      users."points"
    FROM
      users
    JOIN
      assignments ON assignments."userid" = users."userid"
    JOIN
      servers ON servers."serverid" = assignments."serverid"
    WHERE
      servers."discordid" = $1
    ORDER BY
      users."points" ${ desc ? "DESC" : "" }
  `;
  const selectValues = [ serverDiscordId ];

  try {
    const response = await db.query<IDBServerLeaderboardQueryData>(selectQuery, selectValues);

    if(response.rows.length <= 0) {
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: response.rows.map(row => ({
        userId: row.userid,
        userName: row.username,
        points: row.points
      }))
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getPointsLeaderboard", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getPointsLeaderboard", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getPointsLeaderboard", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getPointsLeaderboard", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Retrieves points leaderboard by server ID and country specified.
 *
 * @param { Pool } db Database connection pool.
 * @param { string } serverDiscordId Server snowflake ID.
 * @param { string } countryCode Country code.
 * @param { boolean } desc Whether results should be sorted in descending order.
 */
async function getPointsLeaderboardByCountry(db: Pool, serverDiscordId: string, countryCode: string, desc = true): Promise<DBResponseBase<IDBServerLeaderboardData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      users."userid",
      users."username",
      users."points"
    FROM
      users
    JOIN
      assignments ON assignments."userid" = users."userid"
    JOIN
      servers ON servers."serverid" = assignments."serverid"
    WHERE
      servers."discordid" = $1 AND users."country" = $2
    ORDER BY
      users."points" ${ desc ? "DESC" : "" }
  `;
  const selectValues = [ serverDiscordId, countryCode ];

  try {
    const response = await db.query <IDBServerLeaderboardQueryData>(selectQuery, selectValues);

    if(response.rows.length <= 0) {
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: response.rows.map(row => ({
        userId: row.userid,
        userName: row.username,
        points: row.points
      }))
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getPointsLeaderboardByCountry", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getPointsLeaderboardByCountry", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getPointsLeaderboardByCountry", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getPointsLeaderboardByCountry", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Inserts user to the database.
 *
 * @param { Pool } db Database connection pool.
 * @param { string } discordId Discord ID of the user.
 * @param { number } osuId osu! user ID.
 * @param { string } userName osu! username.
 * @param { string } country Country code.
 *
 * @returns { Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.DUPLICATED_DISCORD_ID | DatabaseErrors.DUPLICATED_OSU_ID | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with `true` if inserted successfully. Returns `DatabaseErrors` enum otherwise.
 */
async function insertUser(db: Pool, discordId: string, osuId: number, userName: string, country: string): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.DUPLICATED_DISCORD_ID | DatabaseErrors.DUPLICATED_OSU_ID | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectDiscordIdQuery = `
    SELECT
      users."userid",
      users."discordid",
      users."osuid",
      users."country"
    FROM
      users
    WHERE
      users."discordid" = $1
  `;
  const selectDiscordIdValues = [ discordId ];

  const selectOsuIdQuery = `
    SELECT
      users."userid",
      users."discordid",
      users."osuid",
      users."country"
    FROM
      users
    WHERE
      users."osuid" = $1
  `;
  const selectOsuIdValues = [ osuId ];

  const insertQuery = `
    INSERT INTO users (discordId, osuId, userName, country)
      VALUES ($1, $2, $3, $4)
  `;
  const insertValues = [ discordId, osuId, userName, country ];

  try {
    const client = await db.connect();

    {
      const discordIdResult = await client.query<IDBServerUserQueryData>(selectDiscordIdQuery, selectDiscordIdValues);
      if(discordIdResult.rows.length > 0) {
        if(discordIdResult.rows[0].discordid === discordId) {
          client.release();
          return {
            status: DatabaseErrors.DUPLICATED_DISCORD_ID
          };
        }
      }
    }

    {
      const osuIdResult = await client.query<IDBServerUserQueryData>(selectOsuIdQuery, selectOsuIdValues);
      if(osuIdResult.rows.length > 0) {
        if(osuIdResult.rows[0].osuid === osuId) {
          client.release();
          return {
            status: DatabaseErrors.DUPLICATED_OSU_ID
          };
        }
      }
    }

    await client.query(insertQuery, insertValues);
    client.release();

    log(LogSeverity.LOG, "insertUser", "users: Inserted 1 row.");
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "insertUser", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "insertUser", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "insertUser", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "insertUser", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Updates user in the database.
 *
 * @param { Pool } db Database connection pool.
 * @param { number } osuId osu! user ID.
 * @param { number | null } points Calculated points.
 * @param { string | null } userName osu! username.
 * @param { string | null } country Country code.
 *
 * @returns { Promise<DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR> } Promise object with `true` if updated successfully. Returns `DatabaseErrors` enum otherwise.
 */
async function updateUser(db: Pool, osuId: number, points: number, userName: string | null, country: string | null): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const updateQuery = `
    UPDATE
      users
    SET
      ${ userName !== null ? "users.\"username\" = $1" : "" }${ userName !== null && country !== null ? "," : "" }
      ${ country !== null ? `users."country" = ${ userName !== null ? "$2" : "$1" }` : "" }
    WHERE
      users."osuid" = ${ userName !== null && country !== null ? "$3" : "$2" }
  `;
  const updateValues: (string | number)[] = [ osuId ];

  if(country !== null) {
    updateValues.unshift(country);
  }

  if(userName !== null) {
    updateValues.unshift(userName);
  }

  try {
    await db.query(updateQuery, updateValues);

    log(LogSeverity.LOG, "updateUser", "users: Updated 1 row.");
    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "updateUser", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "updateUser", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "updateUser", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "updateUser", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

export { getAllUsers, getDiscordUserByOsuId, getDiscordUserByDiscordId, getPointsLeaderboard, getPointsLeaderboardByCountry, insertUser, updateUser };
