import { Pool, PoolClient, DatabaseError } from "pg";
import { Log } from "../utils/log";
import DBUsers from "./users";
import DBServers from "./servers";
import { AssignmentType } from "../utils/common";
import { DatabaseErrors, DuplicatedRecordError, UserNotFoundError, NoRecordError, DatabaseConnectionError, DatabaseClientError } from "../errors/db";
import { IDBServerAssignmentQueryData, IDBServerAssignmentData, IDBAssignmentResultData } from "../types/db/assignments";
import { IDBServerRoleData, IDBServerRoleQueryData } from "../types/db/roles";

/* locally used query interfaces */

/**
 * Database server member's assignment query interface.
 */
interface IDBServerUserAssignmentQueryData {
  assignmentid: number;
  userid: number;
  discordid: string;
  osuid: number;
  username: string;
  country: string;
  roleid: number;
  points: number;
  lastupdate: Date;
}

/**
 * Database server member's assignment interface.
 */
interface IDBServerUserAssignmentData {
    assignmentId: number;
    userId: number;
    discordId: string;
    osuId: number;
    userName: string;
    country: string;
    roleId: number;
    points: number;
    lastUpdate: Date;
}

class DBAssignments {
  /**
   * Gets user assignment by osu! ID from the database.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } osuId osu! ID of the user.
   *
   * @returns { Promise<IDBServerAssignmentData> } Promise object with user assignment data.
   *
   * @throws { UserNotFoundError } Assignment not found in database.
   * @throws { DuplicatedRecordError } Duplicated record found in `userId` column at `assignments` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getAssignmentByOsuId(db: Pool, serverDiscordId: string, osuId: number): Promise<IDBServerAssignmentData> {
    const selectQuery = `
      SELECT
        assignments."assignmentid",
        users."username",
        roles."rolename",
        assignments."lastupdate"
      FROM
        assignments
      JOIN
        users ON assignments."userid" = users."userid"
      JOIN
        roles ON assignments."roleid" = roles."roleid"
      JOIN
        servers ON assignments."serverid" = servers."serverid"
      WHERE
        users."osuid" = $1 AND servers."discordid" = $2
    `;
    const selectValues = [ osuId, serverDiscordId ];

    let discordUserResult;

    try {
      discordUserResult = await db.query<IDBServerAssignmentQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAssignmentByOsuId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getAssignmentByOsuId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAssignmentByOsuId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getAssignmentByOsuId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(discordUserResult.rows.length <= 0) {
      throw new UserNotFoundError();
    }
    else if(discordUserResult.rows.length > 1) {
      throw new DuplicatedRecordError("assignments", "userId");
    }

    return {
      assignmentId: discordUserResult.rows[0].assignmentid,
      userName: discordUserResult.rows[0].username,
      roleName: discordUserResult.rows[0].rolename
    };
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { Pool } db Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string } discordId Discord user ID.
   *
   * @returns { Promise<IDBServerRoleData> } Promise object with queried user role data.
   *
   * @throws { UserNotFoundError } Assignment not found in database.
   * @throws { DuplicatedRecordError } Duplicated record found in `userId` column at `assignments` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getAssignmentRoleDataByDiscordId(db: Pool, serverDiscordId: string, discordId: string): Promise<IDBServerRoleData> {
    const selectQuery = `
      SELECT
        roles."roleid",
        roles."discordid",
        roles."rolename",
        roles."minpoints"
      FROM
        roles
      JOIN
        assignments ON assignments."roleid" = roles."roleid"
      JOIN
        users ON assignments."userid" = users."userid"
      JOIN
        servers ON assignments."serverid" = servers."serverid"
      WHERE
        users."discordid" = $1 AND servers."discordid" = $2
    `;
    const selectValues = [ discordId, serverDiscordId ];

    try {
      const result = await db.query<IDBServerRoleQueryData>(selectQuery, selectValues);

      if(result.rows.length <= 0) {
        throw new NoRecordError();
      }
      else if(result.rows.length > 1) {
        throw new DuplicatedRecordError("assignments", "userId");
      }

      return {
        roleId: result.rows[0].roleid,
        discordId: result.rows[0].discordid,
        roleName: result.rows[0].rolename,
        minPoints: result.rows[0].minpoints
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAssignmentRoleDataByDiscordId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getAssignmentRoleDataByDiscordId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAssignmentRoleDataByDiscordId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getAssignmentRoleDataByDiscordId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Inserts or updates (if the user has already been inserted) assignment data in the database.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } osuId osu! user ID.
   * @param { string } userName osu! username.
   * @param { string } countryCode User's country code.
   * @param { number } points Calculated points.
   *
   * @returns { Promise<IDBAssignmentResultData> } Promise object with assignment result object.
   *
   * @throws { ServerNotFoundError } Server not found in database.
   * @throws { UserNotFoundError } User not found in database.
   * @throws { NoRecordError } No server roles found in database.
   * @throws { DuplicatedRecordError } Duplicate `serverId` (servers)  or `userId` (assignments) found in database.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async insertOrUpdateAssignment(db: Pool, serverDiscordId: string, osuId: number, userName: string, countryCode: string, points: number): Promise<IDBAssignmentResultData> {
    const client = await db.connect();
    let insert = true;

    let assignmentId = 0; // update
    let userId = 0;
    let discordId = "";
    let serverId = 0;
    let currentPoints = 0; // update
    let update = new Date(); // update

    let currentRoleDiscordId = ""; // update
    let currentRoleName = ""; // update

    let targetRoleId = 0;
    let targetRoleDiscordId = "";
    let targetRoleName = "";
    let targetRoleMinimumPoints = 0;

    // serverId
    try {
      const serverResult = await DBServers.getServerByDiscordId(db, serverDiscordId);
      serverId = serverResult.serverId;
    }
    catch (e) {
      if(e instanceof DatabaseErrors) {
        throw e;
      }

      Log.error("insertOrUpdateAssignment", "Unknown error occurred while querying server in database.");
      throw new DatabaseClientError("Querying server failed.");
    }

    // currentRoleDiscordId and currentRoleName
    {
      try {
        const currentRoleResult = await this.#getServerUserRoleDataByOsuId(client, serverDiscordId, osuId);
        currentRoleDiscordId = currentRoleResult.discordId;
        currentRoleName = currentRoleResult.roleName;
      }
      catch (e) {
        if(e instanceof DatabaseErrors) {
          throw e;
        }

        Log.error("insertOrUpdateAssignment", "Unknown error occurred while querying current user role in database.");
        throw new DatabaseClientError("Querying user role failed.");
      }
    }

    // assignment
    {
      let noRecord = false;

      try {
        const assignmentResult = await this.#getServerUserAssignmentDataByOsuId(client, serverDiscordId, osuId);

        if(assignmentResult.osuId === osuId) {
          insert = false;

          if(assignmentResult.userName !== userName || assignmentResult.country !== countryCode) {
            // update user, also update points here
            await DBUsers.updateUser(
              db,
              osuId,
              points,
              assignmentResult.userName !== userName ? userName : null,
              assignmentResult.country !== countryCode ? countryCode : null
            );
          }

          assignmentId = assignmentResult.assignmentId;
          userId = assignmentResult.userId;
          discordId = assignmentResult.discordId;
          currentPoints = assignmentResult.points;
          update = assignmentResult.lastUpdate;

          await this.#deleteAssignmentById(client, assignmentResult.assignmentId);
        }
      }
      catch (e) {
        if(e instanceof DatabaseErrors && e instanceof NoRecordError) {
          noRecord = true;
        }
        else if(e instanceof DatabaseErrors) {
          throw e;
        }
        else {
          Log.error("insertOrUpdateAssignment", "Unknown error occurred while querying assignment in database.");
          throw new DatabaseClientError("Querying assignment failed.");
        }
      }

      if(noRecord) {
        // userId not found, then insert
        try {
          const selectUserResult = await DBUsers.getDiscordUserByOsuId(db, osuId);

          userId = selectUserResult.userId;
          discordId = selectUserResult.discordId;
        }
        catch (e) {
          if(e instanceof DatabaseError && e instanceof UserNotFoundError) {
            Log.info("insertOrUpdateAssignment", "User not found. Skipping assignment data update.");
            throw new UserNotFoundError(); // handle this at calling function level
          }
          else if(e instanceof DatabaseError) {
            throw e;
          }

          Log.error("insertOrUpdateAssignment", "Unknown error occurred while querying user in database.");
          throw new DatabaseClientError("Querying user failed.");
        }
      }
    }

    // targetRoleId, targetRoleDiscordId, targetRoleName
    {
      try {
        const roleResult = await this.#getTargetServerRoleDataByPoints(client, serverDiscordId, points);

        targetRoleId = roleResult.roleId;
        targetRoleDiscordId = roleResult.discordId;
        targetRoleName = roleResult.roleName;
        targetRoleMinimumPoints = roleResult.minPoints;
      }
      catch (e) {
        if(e instanceof DatabaseError) {
          if(e instanceof NoRecordError) {
            Log.info("insertOrUpdateAssignment", `No roles returned. Make sure the lowest value (0) exist on server ID ${ serverDiscordId }.`);
          }

          throw e;
        }

        Log.error("insertOrUpdateAssignment", "Unknown error occurred while querying user in database.");
        throw new DatabaseClientError("Querying user failed.");
      }
    }

    Log.debug("insertOrUpdateAssignment", `rolename: ${ targetRoleName }, min: ${ targetRoleMinimumPoints }`);

    if(insert) {
      await this.#insertAssignment(client, userId, targetRoleId, serverId);
      Log.info("insertOrUpdateAssignment", "assignment: Inserted 1 row.");
    }
    else {
      await this.#insertAssignment(client, userId, targetRoleId, serverId, assignmentId);
      Log.info("insertOrUpdateAssignment", "assignment: Updated 1 row.");
    }

    client.release();

    const role = insert ? {
      newRoleId: targetRoleDiscordId,
      newRoleName: targetRoleName
    } : {
      oldRoleId: currentRoleDiscordId,
      oldRoleName: currentRoleName,
      newRoleId: targetRoleDiscordId,
      newRoleName: targetRoleName
    };

    return {
      type: insert ? AssignmentType.INSERT : AssignmentType.UPDATE,
      discordId,
      role,
      delta: insert ? points : points - currentPoints,
      lastUpdate: !insert ? update : null
    };
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { PoolClient } client Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } osuId osu! user ID.
   *
   * @returns { Promise<IDBServerUserAssignmentData> } Promise object with queried user assignment data.
   *
   * @throws { UserNotFoundError } Assignment not found in database.
   * @throws { DuplicatedRecordError } Duplicated record found in `userId` column at `assignments` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async #getServerUserAssignmentDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<IDBServerUserAssignmentData> {
    const selectQuery = `
      SELECT
        assignments."assignmentid",
        assignments."userid",
        users."discordid",
        users."osuid",
        users."username",
        users."country",
        users."points",
        users."lastupdate",
        assignments."roleid"
      FROM
        assignments
      JOIN
        users ON assignments."userid" = users."userid"
      JOIN
        servers ON assignments."serverid" = servers."serverid"
      WHERE
        users."osuid" = $1 AND servers."discordid" = $2
    `;
    const selectValues = [ osuId, serverDiscordId ];

    try {
      const result = await client.query<IDBServerUserAssignmentQueryData>(selectQuery, selectValues);

      if(result.rowCount <= 0) {
        throw new NoRecordError();
      }
      else if(result.rowCount > 1) {
        throw new DuplicatedRecordError("assignments", "userId");
      }

      return {
        assignmentId: result.rows[0].assignmentid,
        userId: result.rows[0].userid,
        discordId: result.rows[0].discordid,
        osuId: result.rows[0].osuid,
        userName: result.rows[0].username,
        country: result.rows[0].country,
        roleId: result.rows[0].roleid,
        points: result.rows[0].points,
        lastUpdate: result.rows[0].lastupdate
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerUserAssignmentDataByOsuId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getServerUserAssignmentDataByOsuId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerUserAssignmentDataByOsuId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getServerUserAssignmentDataByOsuId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { PoolClient } client Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } osuId osu! user ID.
   *
   * @returns { Promise<IDBServerRoleData> } Promise object with queried user data.
   *
   * @throws { UserNotFoundError } Assignment not found in database.
   * @throws { DuplicatedRecordError } Duplicated record found in `userId` column at `assignments` table.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async #getServerUserRoleDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<IDBServerRoleData> {
    const selectQuery = `
      SELECT
        roles."roleid",
        roles."discordid",
        roles."rolename",
        roles."minpoints"
      FROM
        roles
      JOIN
        assignments ON assignments."roleid" = roles."roleid"
      JOIN
        users ON assignments."userid" = users."userid"
      JOIN
        servers ON assignments."serverid" = servers."serverid"
      WHERE
        users."osuid" = $1 AND servers."discordid" = $2
    `;
    const selectValues = [ osuId, serverDiscordId ];

    try {
      const result = await client.query<IDBServerRoleQueryData>(selectQuery, selectValues);

      if(result.rows.length <= 0) {
        throw new UserNotFoundError();
      }
      else if(result.rows.length > 1) {
        throw new DuplicatedRecordError("assignments", "userId");
      }

      return {
        roleId: result.rows[0].roleid,
        discordId: result.rows[0].discordid,
        roleName: result.rows[0].rolename,
        minPoints: result.rows[0].minpoints
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerUserRoleDataByOsuId", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getServerUserRoleDataByOsuId", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerUserRoleDataByOsuId", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getServerUserRoleDataByOsuId", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Queries specific server's target role assignment data.
   *
   * @param { PoolClient } client Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } points Calculated points.
   *
   * @returns { Promise<IDBServerRoleData> } Promise object with queried role data.
   *
   * @throws { NoRecordError } No target role returned.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async #getTargetServerRoleDataByPoints(client: PoolClient, serverDiscordId: string, points: number): Promise<IDBServerRoleData> {
    const selectQuery = `
      SELECT
        roles."roleid",
        roles."discordid",
        roles."rolename",
        roles."minpoints"
      FROM
        roles
      JOIN
        servers ON servers."serverid" = roles."serverid"
      WHERE
        minpoints <= $1 AND servers."discordid" = $2
      ORDER BY
        minpoints DESC
      LIMIT 1
    `;
    const selectValues = [ points, serverDiscordId ];

    try {
      const result = await client.query(selectQuery, selectValues);

      if(result.rows.length <= 0) {
        throw new NoRecordError();
      }

      return {
        roleId: result.rows[0].roleid,
        discordId: result.rows[0].discordid,
        roleName: result.rows[0].rolename,
        minPoints: result.rows[0].minpoints
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getTargetServerRoleDataByPoints", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getTargetServerRoleDataByPoints", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getTargetServerRoleDataByPoints", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getTargetServerRoleDataByPoints", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Inserts assignment data to the database.
   *
   * @param { PoolClient } client Database pool client.
   * @param { number } userId User ID in the database.
   * @param { number } roleId Role ID in the database.
   * @param { number } serverId Database server ID.
   * @param { number? } assignmentId Assignment ID. Leave `null` to insert sequentially.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async #insertAssignment(client: PoolClient, userId: number, roleId: number, serverId: number, assignmentId: number | null = null): Promise<boolean> {
    const insertQuery = `
      INSERT INTO assignments (${ assignmentId !== null ? "assignmentid, " : "" }userid, roleid, serverid)
        VALUES ($1, $2, $3${ assignmentId !== null ? ", $4" : "" })
    `;

    const insertValues = [ userId, roleId, serverId ];
    if(assignmentId !== null) {
      insertValues.unshift(assignmentId);
    }

    try {
      await client.query(insertQuery, insertValues);
      return true;
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("insertAssignment", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("insertAssignment", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("insertAssignment", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("insertAssignment", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }

  /**
   * Deletes assignment in the database by ID.
   *
   * @param { PoolClient } client Database pool client.
   * @param { number } assignmentId Assignment ID in the database.
   *
   * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
   *
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async #deleteAssignmentById(client: PoolClient, assignmentId: number): Promise<boolean> {
    const deleteQuery = `
      DELETE FROM
        assignments
      WHERE
        assignments."assignmentid" = $1
    `;
    const deleteValue = [ assignmentId ];

    try {
      await client.query(deleteQuery, deleteValue);
      return true;
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("deleteAssignmentById", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("deleteAssignmentById", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("deleteAssignmentById", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("deleteAssignmentById", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }
  }
}

export default DBAssignments;
