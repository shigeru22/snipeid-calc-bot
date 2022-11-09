import { Pool, PoolClient, DatabaseError } from "pg";
import { Log } from "../utils/log";
import DBUsers from "./users";
import DBServers from "./servers";
import { DatabaseErrors, DatabaseSuccess, AssignmentType } from "../utils/common";
import { DBResponseBase } from "../types/db/main";
import { IDBServerAssignmentQueryData, IDBServerAssignmentData, IDBAssignmentResultData } from "../types/db/assignments";
import { IDBServerRoleData, IDBServerRoleQueryData } from "../types/db/roles";

/* locally used query interfaces and functions */

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
   * @returns { Promise<DBResponseBase<IDBServerAssignmentData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> }> } Promise object with user assignment.
   */
  static async getAssignmentByOsuId(db: Pool, serverDiscordId: string, osuId: number): Promise<DBResponseBase<IDBServerAssignmentData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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

    try {
      const discordUserResult = await db.query<IDBServerAssignmentQueryData>(selectQuery, selectValues);

      if(discordUserResult.rows.length <= 0) {
        return {
          status: DatabaseErrors.USER_NOT_FOUND
        };
      }

      if(discordUserResult.rows.length > 1) {
        return {
          status: DatabaseErrors.DUPLICATED_RECORD
        };
      }

      return {
        status: DatabaseSuccess.OK,
        data: {
          assignmentId: discordUserResult.rows[0].assignmentid,
          userName: discordUserResult.rows[0].username,
          roleName: discordUserResult.rows[0].rolename
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAssignmentByOsuId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getAssignmentByOsuId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAssignmentByOsuId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getAssignmentByOsuId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { Pool } db Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { string } discordId Discord user ID.
   *
   * @returns { Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> }> } Promise object with queried user data.
   */
  static async getAssignmentRoleDataByDiscordId(db: Pool, serverDiscordId: string, discordId: string): Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
          roleId: result.rows[0].roleid,
          discordId: result.rows[0].discordid,
          roleName: result.rows[0].rolename,
          minPoints: result.rows[0].minpoints
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getAssignmentRoleDataByDiscordId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getAssignmentRoleDataByDiscordId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getAssignmentRoleDataByDiscordId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getAssignmentRoleDataByDiscordId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
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
   * @returns { Promise<DBResponseBase<IDBAssignmentResultData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with assignment results object.
   */
  static async insertOrUpdateAssignment(db: Pool, serverDiscordId: string, osuId: number, userName: string, countryCode: string, points: number): Promise<DBResponseBase<IDBAssignmentResultData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
    let insert = true;

    try {
      const client = await db.connect();

      const assignmentResult = await this.getServerUserAssignmentDataByOsuId(client, serverDiscordId, osuId);
      if(assignmentResult.status !== DatabaseSuccess.OK && assignmentResult.status !== DatabaseErrors.NO_RECORD) {
        client.release();

        switch(assignmentResult.status) {
          case DatabaseErrors.DUPLICATED_RECORD:
            Log.error("insertOrUpdateAssignment", `Duplicated assignment data found in database with osu! ID ${ osuId } (server ID ${ serverDiscordId }).`);
        }

        return {
          status: assignmentResult.status
        };
      }

      const currentRoleResult = await this.getServerUserRoleDataByOsuId(client, serverDiscordId, osuId);
      if(currentRoleResult.status !== DatabaseSuccess.OK) {
        client.release();

        switch(currentRoleResult.status) {
          case DatabaseErrors.NO_RECORD:
            Log.warn("insertOrUpdateAssignment", "Role table is empty.");
            return {
              status: DatabaseErrors.ROLES_EMPTY
            };
        }

        return {
          status: currentRoleResult.status
        };
      }

      let assignmentId = 0; // update
      let userId = 0;
      let discordId = "";
      let serverId = 0;
      let currentPoints = 0; // update
      let update = new Date(); // update

      // serverId
      {
        const serverResult = await DBServers.getServerByDiscordId(db, serverDiscordId);
        if(serverResult.status !== DatabaseSuccess.OK) {
          Log.error("insertOrUpdateAssignment", "Server not found in database.");

          return {
            status: DatabaseErrors.CLIENT_ERROR
          };
        }

        serverId = serverResult.data.serverId;
      }

      if(assignmentResult.status !== DatabaseErrors.NO_RECORD) {
        // userId found, then update
        if(assignmentResult.data.osuId === osuId) {
          insert = false;

          if(assignmentResult.data.userName !== userName || assignmentResult.data.country !== countryCode) {
            // update user, also update points here
            const ret = await DBUsers.updateUser(
              db,
              osuId,
              points,
              assignmentResult.data.userName !== userName ? userName : null,
              assignmentResult.data.country !== countryCode ? countryCode : null
            );

            if(ret.status !== DatabaseSuccess.OK) {
              client.release();

              Log.error("insertOrUpdateAssignment", "Failed to update user due to connection or client error.");

              return {
                status: ret.status
              };
            }
          }

          assignmentId = assignmentResult.data.assignmentId;
          userId = assignmentResult.data.userId;
          discordId = assignmentResult.data.discordId;
          currentPoints = assignmentResult.data.points;
          update = assignmentResult.data.lastUpdate;

          await this.deleteAssignmentById(client, assignmentResult.data.assignmentId);
        }
        else {
          // should not fall here, but whatever
          client.release();

          Log.error("insertOrUpdateAssignment", "Invalid osuId returned from the database.");
          return {
            status: DatabaseErrors.CLIENT_ERROR
          };
        }
      }
      else {
        // userId not found, then insert
        const selectUserResult = await DBUsers.getDiscordUserByOsuId(db, osuId);
        if(selectUserResult.status !== DatabaseSuccess.OK) {
          client.release();

          switch(selectUserResult.status) {
            case DatabaseErrors.USER_NOT_FOUND:
              Log.info("insertOrUpdateAssignment", "User not found. Skipping assignment data update.");
              break;
          }

          return {
            status: selectUserResult.status
          };
        }

        userId = selectUserResult.data.userId;
        discordId = selectUserResult.data.discordId;
      }

      const rolesResult = await this.getTargetServerRoleDataByPoints(client, serverDiscordId, points);
      if(rolesResult.status !== DatabaseSuccess.OK) {
        switch(rolesResult.status) {
          case DatabaseErrors.NO_RECORD:
            Log.error("insertOrUpdateAssignment", `No roles returned. Make sure the lowest value (0) exist on server ID ${ serverDiscordId }.`);
        }

        return {
          status: rolesResult.status
        };
      }

      Log.debug("insertOrUpdateAssignment", `rolename: ${ rolesResult.data.roleName }, min: ${ rolesResult.data.minPoints }`);

      if(insert) {
        await this.insertAssignment(client, userId, rolesResult.data.roleId, serverId);
        Log.info("insertOrUpdateAssignment", "assignment: Inserted 1 row.");
      }
      else {
        await this.insertAssignment(client, userId, rolesResult.data.roleId, serverId, assignmentId);
        Log.info("insertOrUpdateAssignment", "assignment: Updated 1 row.");
      }

      client.release();

      const role = insert ? {
        newRoleId: rolesResult.data.discordId,
        newRoleName: rolesResult.data.roleName
      } : {
        oldRoleId: currentRoleResult.data.discordId,
        oldRoleName: currentRoleResult.data.roleName,
        newRoleId: rolesResult.data.discordId,
        newRoleName: rolesResult.data.roleName
      };

      return {
        status: DatabaseSuccess.OK,
        data: {
          type: insert ? AssignmentType.INSERT : AssignmentType.UPDATE,
          discordId,
          role,
          delta: insert ? points : points - currentPoints,
          lastUpdate: !insert ? update : null
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("insertOrUpdateAssignment", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("insertOrUpdateAssignment", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("insertOrUpdateAssignment", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("insertOrUpdateAssignment", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { PoolClient } client Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } osuId osu! user ID.
   *
   * @returns { Promise<DBResponseBase<IDBServerUserAssignmentQueryData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with queried user data.
   */
  static async getServerUserAssignmentDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<DBResponseBase<IDBServerUserAssignmentData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
        return {
          status: DatabaseErrors.NO_RECORD
        };
      }
      else if(result.rowCount > 1) {
        return {
          status: DatabaseErrors.DUPLICATED_RECORD
        };
      }

      return {
        status: DatabaseSuccess.OK,
        data: {
          assignmentId: result.rows[0].assignmentid,
          userId: result.rows[0].userid,
          discordId: result.rows[0].discordid,
          osuId: result.rows[0].osuid,
          userName: result.rows[0].username,
          country: result.rows[0].country,
          roleId: result.rows[0].roleid,
          points: result.rows[0].points,
          lastUpdate: result.rows[0].lastupdate
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerUserAssignmentDataByOsuId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getServerUserAssignmentDataByOsuId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerUserAssignmentDataByOsuId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getServerUserAssignmentDataByOsuId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { PoolClient } client Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } osuId osu! user ID.
   *
   * @returns { Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> }> } Promise object with queried user data.
   */
  static async getServerUserRoleDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
          roleId: result.rows[0].roleid,
          discordId: result.rows[0].discordid,
          roleName: result.rows[0].rolename,
          minPoints: result.rows[0].minpoints
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getServerUserRoleDataByOsuId", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getServerUserRoleDataByOsuId", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getServerUserRoleDataByOsuId", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getServerUserRoleDataByOsuId", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }
  }

  /**
   * Queries specific server's user assignment data.
   *
   * @param { PoolClient } client Database connection pool client.
   * @param { string } serverDiscordId Server snowflake ID.
   * @param { number } points Calculated points.
   *
   * @returns { Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with queried user data.
   */
  static async getTargetServerRoleDataByPoints(client: PoolClient, serverDiscordId: string, points: number): Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
        return {
          status: DatabaseErrors.NO_RECORD
        };
      }

      return {
        status: DatabaseSuccess.OK,
        data: {
          roleId: result.rows[0].roleid,
          discordId: result.rows[0].discordid,
          roleName: result.rows[0].rolename,
          minPoints: result.rows[0].minpoints
        }
      };
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getTargetServerRoleDataByPoints", "Database connection failed.");
            return {
              status: DatabaseErrors.CONNECTION_ERROR
            };
          default:
            Log.error("getTargetServerRoleDataByPoints", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("getTargetServerRoleDataByPoints", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("getTargetServerRoleDataByPoints", "Unknown error occurred.");
      }

      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
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
   * @returns { Promise<boolean> } Promise object with `true` if inserted successfully, `false` otherwise.
   */
  static async insertAssignment(client: PoolClient, userId: number, roleId: number, serverId: number, assignmentId: number | null = null): Promise<boolean> {
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
            break;
          default:
            Log.error("insertAssignment", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("insertAssignment", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("insertAssignment", "Unknown error occurred.");
      }

      return false;
    }
  }

  /**
   * Deletes assignment in the database by ID.
   *
   * @param { PoolClient } client Database pool client.
   * @param { number } assignmentId Assignment ID in the database.
   *
   * @returns { Promise<boolean> } Promise object with `true` if deleted successfully, `false` otherwise.
   */
  static async deleteAssignmentById(client: PoolClient, assignmentId: number): Promise<boolean> {
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
            break;
          default:
            Log.error("deleteAssignmentById", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
        }
      }
      else if(e instanceof Error) {
        Log.error("deleteAssignmentById", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
      }
      else {
        Log.error("deleteAssignmentById", "Unknown error occurred.");
      }

      return false;
    }
  }
}

export default DBAssignments;
