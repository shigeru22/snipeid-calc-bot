import { Pool, DatabaseError } from "pg";
import { Log } from "../utils/log";
import { DuplicatedRecordError, UserNotFoundError, NoRecordError, ConflictError, DatabaseConnectionError, DatabaseClientError } from "../errors/db";
import { IDBServerUserData, IDBServerLeaderboardQueryData, IDBServerUserQueryData, IDBServerLeaderboardData } from "../types/db/users";

/**
 * Database `users` table class.
 */
class DBUsers {
  /**
   * Gets Discord user by Discord ID from the database.
   *
   * @param { Pool } db Database connection pool.
   *
   * @returns { Promise<IDBServerUserData[]> } Promise object with all users data array.
   *
   * @throws { NoRecordError } No user data found in database.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getAllUsers(db: Pool): Promise<IDBServerUserData[]> {
    const selectQuery = `
      SELECT
        users."userid",
        users."discordid",
        users."osuid",
        users."points",
        users."country",
        users."lastupdate"
      FROM
        users
    `;

    let usersResult;

    try {
      usersResult = await db.query<IDBServerUserQueryData>(selectQuery);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAllUsers", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getAllUsers", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAllUsers", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getAllUsers", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(usersResult.rows.length <= 0) {
      throw new NoRecordError();
    }

    return usersResult.rows.map(row => ({
      userId: row.userid,
      discordId: row.discordid,
      osuId: row.osuid,
      points: row.points,
      country: row.country,
      lastUpdate: row.lastupdate
    }));
  }

  /**
   * Gets Discord user by osu! ID from the database.
   *
   * @param { Pool } db Database connection pool.
   * @param { number } osuId osu! user ID.
   *
   * @returns { Promise<IDBServerUserData> } Promise object with user data.
   *
   * @throws { UserNotFoundError } User with specified osu! ID not found in database.
   * @throws { DuplicatedRecordError } Duplicated `osuId` data found in `users` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Client error occurred.
   */
  static async getDiscordUserByOsuId(db: Pool, osuId: number): Promise<IDBServerUserData> {
    const selectQuery = `
      SELECT
        users."userid",
        users."discordid",
        users."osuid",
        users."points",
        users."country",
        users."lastupdate"
      FROM
        users
      WHERE
        users."osuid" = $1
    `;
    const selectValues = [ osuId ];

    let discordUserResult;

    try {
      discordUserResult = await db.query<IDBServerUserQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getDiscordUserByOsuId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getDiscordUserByOsuId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getDiscordUserByOsuId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getDiscordUserByOsuId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(discordUserResult.rows.length <= 0) {
      throw new UserNotFoundError();
    }

    if(discordUserResult.rows.length > 1) {
      throw new DuplicatedRecordError("users", "osuId");
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      Log.error("getDiscordUserByOsuId", "Invalid row returned.");
      throw new DatabaseClientError("Invalid row returned.");
    }

    return {
      userId: discordUserResult.rows[0].userid,
      discordId: discordUserResult.rows[0].discordid,
      osuId: discordUserResult.rows[0].osuid,
      points: discordUserResult.rows[0].points,
      country: discordUserResult.rows[0].country,
      lastUpdate: discordUserResult.rows[0].lastupdate
    };
  }

  /**
   * Gets Discord user by Discord ID from the database.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } discordId Discord ID of the user.
   *
   * @returns { Promise<IDBServerUserData> } Promise object with user data.
   *
   * @throws { UserNotFoundError } User with specified osu! ID not found in database.
   * @throws { DuplicatedRecordError } Duplicated `discordId` data found in `users` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Client error occurred.
   */
  static async getDiscordUserByDiscordId(db: Pool, discordId: string): Promise<IDBServerUserData> {
    const selectQuery = `
      SELECT
        users."userid",
        users."discordid",
        users."osuid",
        users."points",
        users."country",
        users."lastupdate"
      FROM
        users
      WHERE
        users."discordid" = $1
    `;
    const selectValues = [ discordId ];

    let discordUserResult;

    try {
      discordUserResult = await db.query<IDBServerUserQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getDiscordUserByDiscordId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getDiscordUserByDiscordId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getDiscordUserByDiscordId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getDiscordUserByDiscordId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(discordUserResult.rows.length <= 0) {
      throw new UserNotFoundError();
    }

    if(discordUserResult.rows.length > 1) {
      throw new DuplicatedRecordError("users", "discordId");
    }

    return {
      userId: discordUserResult.rows[0].userid,
      discordId: discordUserResult.rows[0].discordid,
      osuId: discordUserResult.rows[0].osuid,
      points: discordUserResult.rows[0].points,
      country: discordUserResult.rows[0].country,
      lastUpdate: discordUserResult.rows[0].lastupdate
    };
  }

  // TODO: create getAllServerPointsLeaderboard?

  /**
   * Retrieves points leaderboard by server ID.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { boolean } desc Whether results should be sorted in descending order.
   *
   * @returns { Promise<IDBServerLeaderboardData[]> } Promise object with server points leaderboard data array.
   *
   * @throws { NoRecordError } No user data found in database for the specified `serverDiscordId`.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getServerPointsLeaderboard(db: Pool, serverDiscordId: string, desc = true): Promise<IDBServerLeaderboardData[]> {
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

    let serverLeaderboardData;

    try {
      serverLeaderboardData = await db.query<IDBServerLeaderboardQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getPointsLeaderboard", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getPointsLeaderboard", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getPointsLeaderboard", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getPointsLeaderboard", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(serverLeaderboardData.rows.length <= 0) {
      throw new NoRecordError();
    }

    return serverLeaderboardData.rows.map(row => ({
      userId: row.userid,
      userName: row.username,
      points: row.points
    }));
  }

  /**
   * Retrieves points leaderboard by server ID and country specified.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string } countryCode Country code.
   * @param { boolean } desc Whether results should be sorted in descending order.
   *
   * @throws { NoRecordError } No user data found in database for the specified `serverDiscordId` (and/or `countryCode`).
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getServerPointsLeaderboardByCountry(db: Pool, serverDiscordId: string, countryCode: string, desc = true): Promise<IDBServerLeaderboardData[]> {
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

    let serverLeaderboardData;

    try {
      serverLeaderboardData = await db.query <IDBServerLeaderboardQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getPointsLeaderboardByCountry", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getPointsLeaderboardByCountry", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getPointsLeaderboardByCountry", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getPointsLeaderboardByCountry", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(serverLeaderboardData.rows.length <= 0) {
      throw new NoRecordError();
    }

    return serverLeaderboardData.rows.map(row => ({
      userId: row.userid,
      userName: row.username,
      points: row.points
    }));
  }

  // TODO: create getLastPointUpdate?

  /**
   * Gets last points update time.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<Date> } Promise object with last assignment update time.
   *
   * @throws { NoRecordError } No user data found in database for the specified `serverDiscordId`.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getServerLastPointUpdate(db: Pool, serverDiscordId: string): Promise<Date> {
    const selectQuery = `
      SELECT
        users."lastupdate"
      FROM
        users
      JOIN
        assignments ON assignments."userid" = users."userid"
      JOIN
        servers ON assignments."serverid" = servers."serverid"
      WHERE
        servers."discordid" = $1
      ORDER BY
        users."lastupdate" DESC
      LIMIT 1
    `;
    const selectValues = [ serverDiscordId ];

    let lastUpdatedResult;

    try {
      lastUpdatedResult = await db.query(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getLastPointUpdate", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getLastPointUpdate", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getLastPointUpdate", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getLastPointUpdate", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(lastUpdatedResult.rows[0] === undefined) {
      throw new NoRecordError();
    }

    return lastUpdatedResult.rows[0].lastupdate; // should be date
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
   * @returns { Promise<void> } Promise object no return value. Throws errors below if failed.
   *
   * @throws { ConflictError } Specified Discord user ID or osu! ID already in database.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async insertUser(db: Pool, discordId: string, osuId: number, userName: string, country: string): Promise<void> {
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

    const client = await db.connect();

    try {
      {
        const discordIdResult = await client.query<IDBServerUserQueryData>(selectDiscordIdQuery, selectDiscordIdValues);
        if(discordIdResult.rows.length > 0) {
          if(discordIdResult.rows[0].discordid === discordId) {
            client.release();
            throw new ConflictError("users", "discordId");
          }
        }
      }

      {
        const osuIdResult = await client.query<IDBServerUserQueryData>(selectOsuIdQuery, selectOsuIdValues);
        if(osuIdResult.rows.length > 0) {
          if(osuIdResult.rows[0].osuid === osuId) {
            client.release();
            throw new ConflictError("users", "osuId");
          }
        }
      }

      await client.query(insertQuery, insertValues);
      client.release();

      Log.info("insertUser", "users: Inserted 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("insertUser", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("insertUser", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("insertUser", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("insertUser", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
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
   * @returns { Promise<void> } Promise object no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async updateUser(db: Pool, osuId: number, points: number, userName: string | null = null, country: string | null = null): Promise<void> {
    // only points, userName, and country are updatable

    const updateQuery = `
      UPDATE
        users
      SET
        points = $1,
        lastUpdate = $2${ userName !== null || country !== null ? "," : "" }
        ${ userName !== null ? "username = $3" : "" }${ userName !== null && country !== null ? "," : "" }
        ${ country !== null ? `country = ${ userName !== null ? "$4" : "$3" }` : "" }
      WHERE
        osuid = ${ userName !== null && country !== null ? "$5" : (userName !== null || country !== null) ? "$4" : "$3" }
    `;
    const updateValues: (string | number | Date)[] = [ osuId ];

    if(country !== null) {
      updateValues.unshift(country);
    }

    if(userName !== null) {
      updateValues.unshift(userName);
    }

    updateValues.unshift(points, new Date());

    try {
      await db.query(updateQuery, updateValues);
      Log.info("updateUser", "users: Updated 1 row.");
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("updateUser", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("updateUser", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("updateUser", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("updateUser", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }
}

export default DBUsers;
