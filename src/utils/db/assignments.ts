import { Pool, DatabaseError } from "pg";
import { LogSeverity, log } from "../log";
import { updateUser } from "./users";
import { DatabaseErrors, AssignmentType, AssignmentSort } from "../common";

// TODO: convert compound object return types into interfaces

// TODO: create conditional types

/**
 * Gets all `assignments` table data.
 *
 * @param { Pool } db - Database connection pool.
 * @param { number } sort - Sort order criteria, using `AssignmentSort` constant.
 * @param { boolean } desc - Whether results should be sorted in descending order.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment: { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date }[]; }> } - Promise object with array of assignments.
 */
async function getAllAssignments(db: Pool, sort: number, desc: boolean): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignments?: { assignmentid: number; username: string; rolename: string; points: number; lastupdate: Date; }[]; }> {
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
 * Gets user assignment by osu! ID from the database.
 *
 * @param { Pool } db - Database connection pool.
 * @param { number } osuId - osu! ID of the user.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; assignment?: { userId: number; discordId: string; osuId: number; }; }> } - Promise object with user assignment.
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
 * Gets last assignment update time.
 *
 * @param { Pool } db - Database connection pool.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; date?: Date; }> } - Promise object with last assignment update time.
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
 * Inserts or updates (if the user has already been inserted) assignment data in the database.
 *
 * @param { Pool } db - Database connection pool.
 * @param { number } osuId - osu! user ID.
 * @param { number } points - Calculated points.
 * @param { string } userName - osu! username.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.USER_NOT_FOUND | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; data?: { type: AssignmentType; discordId: string; role: { oldRoleId?: string; oldRoleName?: string; newRoleId: string; newRoleName: string; }; delta: number; lastUpdate: Date | null; }; }> } Promise object with assignment results object.
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

export { getAllAssignments, getAssignmentByOsuId, getLastAssignmentUpdate, insertOrUpdateAssignment };
