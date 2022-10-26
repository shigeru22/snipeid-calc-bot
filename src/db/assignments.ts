import { Pool, PoolClient, DatabaseError } from "pg";
import { LogSeverity, log } from "../utils/log";
import { getDiscordUserByOsuId, updateUser } from "./users";
import { DatabaseErrors, AssignmentType, AssignmentSort, assignmentSortToString } from "../utils/common";

// TODO: convert compound object return types into interfaces

// TODO: create conditional types

/**
 * Gets all `assignments` table data.
 *
 * @param { Pool } db - Database connection pool.
 * @param { AssignmentSort } sort - Sort order criteria.
 * @param { boolean } desc - Whether results should be sorted in descending order.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment: { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date }[]; }> } - Promise object with array of assignments.
 *
 * @deprecated This function returns all assignments from all servers with the second version of tables. Use `getAllServerAssignments` instead.
 */
async function getAllAssignments(db: Pool, sort: AssignmentSort, desc: boolean): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignments?: { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date; }[]; }> {
  const selectQuery = `
    SELECT
      a."assignmentid", u."username", r."rolename", a."points", a."lastupdate"
    FROM
      assignments AS a
    JOIN
      users AS u
    ON
      a."userid"=u."userid"
    JOIN
      roles AS r
    ON
      a."roleid"=r."roleid"
    ORDER BY
      ${ sort === AssignmentSort.ID ? "a.\"assignmentid\"" : sort === AssignmentSort.ROLE_ID ? "a.\"roleid\"" : sort === AssignmentSort.POINTS ? "a.\"points\"" : "a.\"lastupdate\"" } ${ desc ? "DESC" : "" }
    LIMIT 50
  `;

  try {
    const response = await db.query(selectQuery); // TODO: add type annotation to queries
    return {
      status: DatabaseErrors.OK,
      assignments: response.rows
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
 * Gets all `assignments` table data by Discord server ID.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { AssignmentSort } sort - Sort order criteria.
 * @param { boolean } desc - Whether results should be sorted in descending order.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment: { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date }[]; }> } - Promise object with array of assignments.
 */
async function getAllServerAssignments(db: Pool, serverDiscordId: string, sort = AssignmentSort.POINTS, desc = true): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignments?: { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date; }[]; }> {
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
    const response = await db.query(selectQuery, selectValues); // TODO: add type annotation to queries
    return {
      status: DatabaseErrors.OK,
      assignments: response.rows
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
 * @param { number } osuId - osu! ID of the user.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { userId: number; discordId: string; osuId: number; }; }> } - Promise object with user assignment.
 *
 * @deprecated This function returns all assignments from all servers with the second version of tables. Use `getServerAssignmentByOsuId` instead.
 */
async function getAssignmentByOsuId(db: Pool, osuId: number): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { userId: number; discordId: string; osuId: number; }; }> {
  const selectQuery = `
    SELECT
      a."assignmentid", a."userid", u."discordid", u."osuid", a."roleid", a."points"
    FROM
      assignments AS a
    JOIN
      users as u
    ON
      a."userid"=u."userid"
    WHERE
      u."osuid"=$1
  `;
  const selectValues = [ osuId ];

  try {
    const discordUserResult = await db.query(selectQuery, selectValues);

    if(typeof(discordUserResult.rows[0]) === "undefined") {
      return {
        status: DatabaseErrors.USER_NOT_FOUND
      };
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    return {
      status: DatabaseErrors.OK,
      assignment: {
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
 * Gets user assignment by osu! ID from the database.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! ID of the user.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { userId: number; discordId: string; osuId: number; }; }> } - Promise object with user assignment.
 */
async function getServerAssignmentByOsuId(db: Pool, serverDiscordId: string, osuId: number): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { userId: number; discordId: string; osuId: number; }; }> {
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

    if(typeof(discordUserResult.rows[0]) === "undefined") {
      return {
        status: DatabaseErrors.USER_NOT_FOUND
      };
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    return {
      status: DatabaseErrors.OK,
      assignment: {
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
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; date?: Date; }> } - Promise object with last assignment update time.
 *
 * @deprecated This function returns all assignments from all servers with the second version of tables. Use `getServerLastAssignmentUpdate` instead.
 */
async function getLastAssignmentUpdate(db: Pool): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; date?: Date; }> {
  const selectQuery = `
    SELECT
      a."lastupdate"
    FROM
      assignments AS a
    ORDER BY
      a."lastupdate" DESC
    LIMIT 1;
  `;

  try {
    const client = await db.connect();

    const result = await client.query(selectQuery);

    if(typeof(result.rows[0]) === "undefined") {
      client.release();
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    client.release();

    return {
      status: DatabaseErrors.OK,
      date: result.rows[0].lastupdate
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
 * Gets last assignment update time.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; date?: Date; }> } - Promise object with last assignment update time.
 */
async function getServerLastAssignmentUpdate(db: Pool, serverDiscordId: string): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; date?: Date; }> {
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
      status: DatabaseErrors.OK,
      date: result.rows[0].lastupdate
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
 * @param { number } osuId - osu! user ID.
 * @param { number } points - Calculated points.
 * @param { string } userName - osu! username.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; data?: { type: AssignmentType; discordId: string; role: { oldRoleId?: string; oldRoleName?: string; newRoleId: string; newRoleName: string; }; delta: number; lastUpdate: Date | null; }; }> } Promise object with assignment results object.
 *
 * @deprecated This function is applicable for first version of tables. Use `insertOrUpdateAssignment2` instead.
 */
async function insertOrUpdateAssignment(db: Pool, osuId: number, points: number, userName: string): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; data?: { type: AssignmentType; discordId: string; role: { oldRoleId?: string; oldRoleName?: string; newRoleId: string; newRoleName: string; }; delta: number; lastUpdate: Date | null; }; }> {
  const selectAssignmentQuery = `
    SELECT
      a."assignmentid", a."userid", u."discordid", u."osuid", u."username", a."roleid", a."points", a."lastupdate"
    FROM
      assignments AS a
    JOIN
      users as u
    ON
      a."userid"=u."userid"
    WHERE
      u."osuid"=$1
  `;
  const selectAssignmentValues = [ osuId ];

  const selectCurrentRoleQuery = `
    SELECT
      r."roleid", r."discordid", r."rolename", r."minpoints"
    FROM
      roles AS r
    JOIN
      assignments AS a
    ON
      r."roleid"=a."roleid"
    JOIN
      users AS u
    ON
      a."userid"=u."userid"
    WHERE
      u."osuid"=$1
  `;
  const selectCurrentRoleValues = [ osuId ];

  const selectRoleQuery = "SELECT roleid, discordid, rolename, minpoints FROM roles WHERE minpoints<=$1 ORDER BY minpoints DESC LIMIT 1";
  const selectRoleValues = [ points ];
  let insert = true;

  try {
    const client = await db.connect();

    const assignmentResult = await client.query(selectAssignmentQuery, selectAssignmentValues);
    const currentRoleResult = await client.query(selectCurrentRoleQuery, selectCurrentRoleValues);

    let query = "";
    const values = [];
    let userId = 0;
    let discordId = "";

    if(assignmentResult.rows.length > 0) {
      // userId found, then update
      if(assignmentResult.rows[0].osuid === osuId) {
        // query = "UPDATE assignments SET roleid=$2, points=$3, lastupdate=$4 WHERE userid=$1"; // this doesn't update the timestamp!
        insert = false;

        if(assignmentResult.rows[0].username !== userName) {
          // update user
          const ret = await updateUser(db, osuId, userName);
          if(ret !== DatabaseErrors.OK) {
            client.release();
            return {
              status: ret
            };
          }
        }

        userId = assignmentResult.rows[0].userid;
        discordId = assignmentResult.rows[0].discordid;

        await client.query("DELETE FROM assignments WHERE assignmentid=$1", [ assignmentResult.rows[0].assignmentid ]);

        query = "INSERT INTO assignments (assignmentid, userid, roleid, points, lastupdate) VALUES ($1, $2, $3, $4, $5)";
        values.push(assignmentResult.rows[0].assignmentid);
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
      const selectUserQuery = "SELECT userid, discordid, osuid FROM users WHERE osuid=$1";
      const selectUserValues = [ osuId ];

      const selectUserResult = await client.query(selectUserQuery, selectUserValues);

      if(selectUserResult.rows.length === 0) {
        client.release();

        log(LogSeverity.LOG, "insertOrUpdateAssignment", "User not found. Skipping assignment data update.");
        return {
          status: DatabaseErrors.USER_NOT_FOUND
        };
      }

      query = "INSERT INTO assignments (userid, roleid, points, lastupdate) VALUES ($1, $2, $3, $4)";

      userId = selectUserResult.rows[0].userid;
      discordId = selectUserResult.rows[0].discordid;
    }

    const rolesResult = await client.query(selectRoleQuery, selectRoleValues);
    if(rolesResult.rows.length === 0) {
      client.release();

      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Role table is empty.");
      return {
        status: DatabaseErrors.ROLES_EMPTY // role data empty
      };
    }

    if(rolesResult.rows[0].minPoints < points) {
      client.release();

      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Invalid role returned due to wrong minimum points.");
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    values.push(userId, rolesResult.rows[0].roleid, points, new Date());
    await client.query(query, values);

    if(insert) {
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Inserted 1 row.");
    }
    else {
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Updated 1 row.");
    }

    client.release();

    const role = insert ? {
      newRoleId: rolesResult.rows[0].discordid,
      newRoleName: rolesResult.rows[0].rolename
    } : {
      oldRoleId: currentRoleResult.rows[0].discordid,
      oldRoleName: currentRoleResult.rows[0].rolename,
      newRoleId: rolesResult.rows[0].discordid,
      newRoleName: rolesResult.rows[0].rolename
    };

    return {
      status: DatabaseErrors.OK,
      data: {
        type: insert ? AssignmentType.INSERT : AssignmentType.UPDATE,
        discordId,
        role,
        delta: insert ? points : points - assignmentResult.rows[0].points,
        lastUpdate: !insert ? assignmentResult.rows[0].lastupdate : null
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

/**
 * Inserts or updates (if the user has already been inserted) assignment data in the database.
 * This function assumes v2 tables are used.
 *
 * @param { Pool } db - Database connection pool.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! user ID.
 * @param { string } userName - osu! username.
 * @param { number } points - Calculated points.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; data?: { type: AssignmentType; discordId: string; role: { oldRoleId?: string; oldRoleName?: string; newRoleId: string; newRoleName: string; }; delta: number; lastUpdate: Date | null; }; }> } Promise object with assignment results object.
 */
async function insertOrUpdateAssignment2(db: Pool, serverDiscordId: string, osuId: number, userName: string, points: number): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; data?: { type: AssignmentType; discordId: string; role: { oldRoleId?: string; oldRoleName?: string; newRoleId: string; newRoleName: string; }; delta: number; lastUpdate: Date | null; }; }> {
  let insert = true;

  try {
    const client = await db.connect();

    const assignmentResult = await getServerUserAssignmentDataByOsuId(client, serverDiscordId, osuId);
    if(assignmentResult.status !== DatabaseErrors.OK) {
      client.release();

      return {
        status: assignmentResult.status
      };
    }

    const currentRoleResult = await getServerUserRoleDataByOsuId(client, serverDiscordId, osuId);
    if(currentRoleResult.status !== DatabaseErrors.OK || currentRoleResult.role === undefined) {
      client.release();

      switch(currentRoleResult.status) {
        case DatabaseErrors.NO_RECORD:
          log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Role table is empty.");
          return {
            status: DatabaseErrors.ROLES_EMPTY // TODO: move error to query function
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

    if(assignmentResult.assignment !== undefined) {
      // userId found, then update
      if(assignmentResult.assignment.osuId === osuId) {
        insert = false;

        if(assignmentResult.assignment.userName !== userName) {
          // update user
          const ret = await updateUser(db, osuId, userName);
          if(ret !== DatabaseErrors.OK) {
            client.release();
            return {
              status: ret
            };
          }
        }

        assignmentId = assignmentResult.assignment.assignmentId;
        userId = assignmentResult.assignment.userId;
        discordId = assignmentResult.assignment.discordId;
        currentPoints = assignmentResult.assignment.points;
        update = assignmentResult.assignment.lastUpdate;

        await deleteAssignmentById(client, assignmentResult.assignment.assignmentId);
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
      if(selectUserResult.status !== DatabaseErrors.OK || selectUserResult.user === undefined) {
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

      userId = selectUserResult.user.userId;
      discordId = selectUserResult.user.discordId;
    }

    const rolesResult = await getTargetServerRoleDataByPoints(client, serverDiscordId, points);
    if(rolesResult.status !== DatabaseErrors.OK || rolesResult.role === undefined) {
      switch(rolesResult.status) {
        case DatabaseErrors.NO_RECORD:
          log(LogSeverity.ERROR, "insertOrUpdateAssignment2", `No roles returned. Make sure the lowest value (0) exist on server ID ${ serverDiscordId }.`);
      }

      return {
        status: rolesResult.status
      };
    }

    if(rolesResult.role.minPoints < points) {
      client.release();

      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Invalid role returned due to wrong minimum points.");
      return {
        status: DatabaseErrors.CLIENT_ERROR
      };
    }

    if(insert) {
      await insertAssignment(client, userId, rolesResult.role.roleId, points);
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Inserted 1 row.");
    }
    else {
      await insertAssignment(client, userId, rolesResult.role.roleId, points, assignmentId);
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Updated 1 row.");
    }

    client.release();

    const role = insert ? {
      newRoleId: rolesResult.role.discordId,
      newRoleName: rolesResult.role.roleName
    } : {
      oldRoleId: currentRoleResult.role.discordId,
      oldRoleName: currentRoleResult.role.roleName,
      newRoleId: rolesResult.role.discordId,
      newRoleName: rolesResult.role.roleName
    };

    return {
      status: DatabaseErrors.OK,
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

/* locally used query functions */

/**
 * Queries specific server's user assignment data.
 *
 * @param { PoolClient } client - Database connection pool client.
 * @param { string } serverDiscordId - Server snowflake ID.
 * @param { number } osuId - osu! user ID.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { assignmentId: number; userId: number; discordId: string; osuId: number; userName: string; roleId: number; points: number; lastUpdate: Date }; }> } - Promise object with queried user data.
 */
async function getServerUserAssignmentDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { assignmentId: number; userId: number; discordId: string; osuId: number; userName: string; roleId: number; points: number; lastUpdate: Date; }; }> {
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
    const result = await client.query(selectQuery, selectValues);

    if(result.rowCount <= 0) {
      return {
        status: DatabaseErrors.OK // TODO: find better way to differentiate in types
      };
    }
    else if(result.rowCount > 1) {
      return {
        status: DatabaseErrors.DUPLICATED_RECORD
      };
    }

    return {
      status: DatabaseErrors.OK,
      assignment: {
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
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; role?: { roleId: number; discordId: string; roleName: string; minPoints: number; }; }> } - Promise object with queried user data.
 */
async function getServerUserRoleDataByOsuId(client: PoolClient, serverDiscordId: string, osuId: number): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; role?: { roleId: number; discordId: string; roleName: string; minPoints: number; }; }> {
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
    const result = await client.query(selectQuery, selectValues);

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
      status: DatabaseErrors.OK,
      role: {
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
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.DUPLICATED_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; role?: { roleId: number; discordId: string; roleName: string; minPoints: number; }; }> } - Promise object with queried user data.
 */
async function getTargetServerRoleDataByPoints(client: PoolClient, serverDiscordId: string, points: number): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; role?: { roleId: number; discordId: string; roleName: string; minPoints: number; }; }> {
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
      status: DatabaseErrors.OK,
      role: {
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

export { getAllAssignments, getAssignmentByOsuId, getLastAssignmentUpdate, insertOrUpdateAssignment, getAllServerAssignments, getServerAssignmentByOsuId, getServerLastAssignmentUpdate, insertOrUpdateAssignment2 };
