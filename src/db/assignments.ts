import { Pool, PoolClient, DatabaseError } from "pg";
import { LogSeverity, log } from "../utils/log";
import { getDiscordUserByOsuId, updateUser } from "./users";
import { DatabaseErrors, DatabaseSuccess, AssignmentType, AssignmentSort, assignmentSortToString } from "../utils/common";
import { DBResponseBase } from "../types/db/main";
import { IDBServerAssignmentQueryData, IDBServerAssignmentData, IDBAssignmentResultData } from "../types/db/assignments";
import { IDBServerRoleData, IDBServerRoleQueryData } from "../types/db/roles";
import { IDBServerUserData } from "../types/db/users";

/**
 * Gets all `assignments` table data by Discord server ID.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { AssignmentSort } sort - Sort order criteria.
 * @param { boolean } desc - Whether results should be sorted in descending order.
 *
 * @returns { Promise<DBResponseBase<IDBServerAssignmentData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } - Promise object with array of assignments.
 */
async function getAllAssignments(db: Pool, serverDiscordId: string, sort = AssignmentSort.POINTS, desc = true): Promise<DBResponseBase<IDBServerAssignmentData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      assignments."assignmentid",
      users."username",
      roles."rolename",
      assignments."points",
      assignments."lastupdate"
    FROM
      assignments
    JOIN
      users ON assignments."userid" = users."userid"
    JOIN
      servers ON assignments."serverid" = servers."serverid"
    JOIN
      roles ON assignments."roleid" = roles."roleid"
    WHERE
      servers."discordid" = $1
    ORDER BY
      ${ assignmentSortToString(sort) } ${ desc ? "DESC" : "" }
  `;
  const selectValues = [ serverDiscordId ];

  try {
    const response = await db.query <IDBServerAssignmentQueryData>(selectQuery, selectValues);

    if(response.rows.length <= 0) {
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: response.rows.map(row => ({
        assignmentId: row.assignmentid,
        userName: row.username,
        roleName: row.rolename,
        points: row.points,
        lastUpdate: row.lastupdate
      }))
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getAllAssignments", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getAllAssignments", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAllAssignments", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getAllAssignments", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Gets user assignment by osu! ID from the database.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! ID of the user.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { userId: number; discordId: string; osuId: number; }; }> } - Promise object with user assignment.
 */
async function getAssignmentByOsuId(db: Pool, serverDiscordId: string, osuId: number): Promise<DBResponseBase<IDBServerUserData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      assignments."assignmentid",
      assignments."userid",
      users."discordid",
      users."osuid",
      users."username",
      assignments."roleid",
      assignments."points",
      assignments."lastupdate"
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
    const discordUserResult = await db.query(selectQuery, selectValues);

    if(discordUserResult.rows.length <= 0) {
      return {
        status: DatabaseErrors.USER_NOT_FOUND
      };
    }

    if(discordUserResult.rows[0].osuid > 1) {
      return {
        status: DatabaseErrors.DUPLICATED_RECORD
      };
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: {
        userId: discordUserResult.rows[0].userid,
        discordId: discordUserResult.rows[0].discordid,
        osuId: discordUserResult.rows[0].osuid
      }
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getAssignmentByOsuId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getAssignmentByOsuId", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAssignmentByOsuId", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getAssignmentByOsuId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Gets last assignment update time.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 *
 * @returns { Promise<DBResponseBase<Date> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } - Promise object with last assignment update time.
 */
async function getLastAssignmentUpdate(db: Pool, serverDiscordId: string): Promise<DBResponseBase<Date> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      assignments."lastupdate"
    FROM
      assignments
    JOIN
      servers ON assignments."serverid" = servers."serverid"
    WHERE
      servers."discordid" = $1
    ORDER BY
      assignments."lastupdate" DESC
    LIMIT 1
  `;
  const selectValues = [ serverDiscordId ];

  try {
    const client = await db.connect();

    const result = await client.query(selectQuery, selectValues);

    if(typeof(result.rows[0]) === "undefined") {
      client.release();
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    client.release();

    return {
      status: DatabaseSuccess.OK,
      data: result.rows[0].lastupdate
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getLastAssignmentUpdate", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getLastAssignmentUpdate", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getLastAssignmentUpdate", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getLastAssignmentUpdate", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Inserts or updates (if the user has already been inserted) assignment data in the database.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! user ID.
 * @param { string } userName - osu! username.
 * @param { number } points - Calculated points.
 *
 * @returns { Promise<DBResponseBase<IDBAssignmentResultData> | DBResponseBase<DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with assignment results object.
 */
async function insertOrUpdateAssignment(db: Pool, serverDiscordId: string, osuId: number, userName: string, points: number): Promise<DBResponseBase<IDBAssignmentResultData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  let insert = true;

  try {
    const client = await db.connect();

    const assignmentResult = await getServerUserAssignmentDataByOsuId(client, serverDiscordId, osuId);
    if(assignmentResult.status !== DatabaseSuccess.OK && assignmentResult.status !== DatabaseErrors.NO_RECORD) {
      client.release();

      switch(assignmentResult.status) {
        case DatabaseErrors.DUPLICATED_RECORD:
          log(LogSeverity.ERROR, "insertOrUpdateAssignment", `Duplicated assignment data found in database with osu! ID ${ osuId } (server ID ${ serverDiscordId }).`);
      }

      return {
        status: assignmentResult.status
      };
    }

    const currentRoleResult = await getServerUserRoleDataByOsuId(client, serverDiscordId, osuId);
    if(currentRoleResult.status !== DatabaseSuccess.OK) {
      client.release();

      switch(currentRoleResult.status) {
        case DatabaseErrors.NO_RECORD:
          log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Role table is empty.");
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
    let currentPoints = 0; // update
    let update = new Date(); // update

    if(assignmentResult.status !== DatabaseErrors.NO_RECORD) {
      // userId found, then update
      if(assignmentResult.data.osuId === osuId) {
        insert = false;

        if(assignmentResult.data.userName !== userName) {
          // update user
          const ret = await updateUser(db, osuId, userName);
          if(ret.status !== DatabaseSuccess.OK) {
            client.release();

            log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Failed to update user due to connection or client error.");

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

        await deleteAssignmentById(client, assignmentResult.data.assignmentId);
      }
      else {
        // should not fall here, but whatever
        client.release();

        log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Invalid osuId returned from the database.");
        return {
          status: DatabaseErrors.CLIENT_ERROR
        };
      }
    }
    else {
      // userId not found, then insert
      const selectUserResult = await getDiscordUserByOsuId(db, osuId);
      if(selectUserResult.status !== DatabaseSuccess.OK) {
        client.release();

        switch(selectUserResult.status) {
          case DatabaseErrors.USER_NOT_FOUND:
            log(LogSeverity.LOG, "insertOrUpdateAssignment", "User not found. Skipping assignment data update.");
            break;
        }

        return {
          status: selectUserResult.status
        };
      }

      userId = selectUserResult.data.userId;
      discordId = selectUserResult.data.discordId;
    }

    const rolesResult = await getTargetServerRoleDataByPoints(client, serverDiscordId, points);
    if(rolesResult.status !== DatabaseSuccess.OK) {
      switch(rolesResult.status) {
        case DatabaseErrors.NO_RECORD:
          log(LogSeverity.ERROR, "insertOrUpdateAssignment2", `No roles returned. Make sure the lowest value (0) exist on server ID ${ serverDiscordId }.`);
      }

      return {
        status: rolesResult.status
      };
    }

    if(rolesResult.data.minPoints < points) {
      client.release();

      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Invalid role returned due to wrong minimum points.");
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    if(insert) {
      await insertAssignment(client, userId, rolesResult.data.roleId, points);
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Inserted 1 row.");
    }
    else {
      await insertAssignment(client, userId, rolesResult.data.roleId, points, assignmentId);
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Updated 1 row.");
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
          log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/* locally used query interfaces and functions */

interface IDBServerUserAssignmentQueryData {
  assignmentid: number;
  userid: number;
  discordid: string;
  osuid: number;
  username: string;
  roleid: number;
  points: number;
  lastupdate: Date;
}

interface IDBServerUserAssignmentData {
    assignmentId: number;
    userId: number;
    discordId: string;
    osuId: number;
    userName: string;
    roleId: number;
    points: number;
    lastUpdate: Date;
}

/**
 * Queries specific server's user assignment data.
 *
 * @param { PoolClient } client - Database connection pool client.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! user ID.
 *
 * @returns { Promise<DBResponseBase<IDBServerUserAssignmentQueryData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } - Promise object with queried user data.
 */
async function getServerUserAssignmentDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<DBResponseBase<IDBServerUserAssignmentData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      assignments."assignmentid",
      assignments."userid",
      users."discordid",
      users."osuid",
      users."username",
      assignments."roleid",
      assignments."points",
      assignments."lastupdate"
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
          log(LogSeverity.ERROR, "getServerUserAssignmentDataByOsuId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getServerUserAssignmentDataByOsuId", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getServerUserAssignmentDataByOsuId", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getServerUserAssignmentDataByOsuId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Queries specific server's user assignment data.
 *
 * @param { PoolClient } client - Database connection pool client.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! user ID.
 *
 * @returns { Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> }> } - Promise object with queried user data.
 */
async function getServerUserRoleDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
          log(LogSeverity.ERROR, "getServerUserRoleDataByOsuId", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getServerUserRoleDataByOsuId", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getServerUserRoleDataByOsuId", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getServerUserRoleDataByOsuId", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Queries specific server's user assignment data.
 *
 * @param { PoolClient } client - Database connection pool client.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } points - Calculated points.
 *
 * @returns { Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } - Promise object with queried user data.
 */
async function getTargetServerRoleDataByPoints(client: PoolClient, serverDiscordId: string, points: number): Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
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
          log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Inserts assignment data to the database.
 *
 * @param { PoolClient } client - Database pool client.
 * @param { number } userId - User ID in the database.
 * @param { number } roleId - Role ID in the database.
 * @param { number } points - Calculated points.
 * @param { number? } assignmentId - Assignment ID. Leave `null` to insert sequentially.
 *
 * @returns { Promise<boolean> } Promise object with `true` if inserted successfully, `false` otherwise.
 */
async function insertAssignment(client: PoolClient, userId: number, roleId: number, points: number, assignmentId: number | null = null): Promise<boolean> {
  const insertQuery = `
    INSERT INTO assignments (${ assignmentId !== null ? "assignmentid, " : "" }userid, roleid, points, lastupdate)
      VALUES ($1, $2, $3, $4${ assignmentId !== null ? ", $5" : "" })
  `;

  const insertValues = [ userId, roleId, points, new Date() ];
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
          log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "Database connection failed.");
          break;
        default:
          log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getTargetServerRoleDataByPoints", "Unknown error occurred.");
    }

    return false;
  }
}

/**
 * Deletes assignment in the database by ID.
 *
 * @param { PoolClient } client - Database pool client.
 * @param { number } assignmentId - Assignment ID in the database.
 *
 * @returns { Promise<boolean> } Promise object with `true` if deleted successfully, `false` otherwise.
 */
async function deleteAssignmentById(client: PoolClient, assignmentId: number): Promise<boolean> {
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
          log(LogSeverity.ERROR, "deleteAssignmentById", "Database connection failed.");
          break;
        default:
          log(LogSeverity.ERROR, "deleteAssignmentById", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "deleteAssignmentById", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "deleteAssignmentById", "Unknown error occurred.");
    }

    return false;
  }
}

export { getAllAssignments, getAssignmentByOsuId, getLastAssignmentUpdate, insertOrUpdateAssignment };