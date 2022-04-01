const { Client } = require("pg");
const { LogSeverity, log } = require("../log");
const { DatabaseErrors, AssignmentType, AssignmentSort, isSortEnumAvailable } = require("../common");

async function getAllAssignments(db, sort, desc) {
  if(!(db instanceof Client)) {
    log(LogSeverity.ERROR, "getAllAssignments", "db must be a Client object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(sort) !== "number") {
    log(LogSeverity.ERROR, "getAllAssignments", "userId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(desc) !== "boolean") {
    log(LogSeverity.ERROR, "getAllAssignments", "desc must be boolean.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(!isSortEnumAvailable(sort)) {
    log(LogSeverity.ERROR, "getAllAssignments", "sort value is not available in AssignmentSort enum.");
    return DatabaseErrors.TYPE_ERROR;
  }

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
  ` +
  (
    sort === AssignmentSort.ID
      ? `a."assignmentid"`
      : sort === AssignmentSort.ROLE_ID
        ? `a."roleid"`
        : sort === AssignmentSort.POINTS
          ? `a."points"`
          : `a."lastupdate"`
  ) +
  (
    desc && " DESC"
  ) +
  `
    LIMIT 50;
  `;

  try {
    const response = await db.query(selectQuery);

    return response.rows;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "getAllAssignments", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "getAllAssignments", "An error occurred while querying assignment: " + e.message);
      }
    }
    else {
      log(LogSeverity.ERROR, "getAllAssignments", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function getAssignmentByOsuId(db, osuId) {
  if(!(db instanceof Client)) {
    log(LogSeverity.ERROR, "getAssignmentByOsuId", "db must be a Client object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(userId) !== "number") {
    log(LogSeverity.ERROR, "getAssignmentByOsuId", "userId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = `SELECT
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

    if(typeof(discordUserResult.rows[0]) === "undefined")  {
      return DatabaseErrors.USER_NOT_FOUND;
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      return DatabaseErrors.CLIENT_ERROR;
    }

    return {
      userId: discordUserResult.rows[0].userid,
      discordId: discordUserResult.rows[0].discordid,
      osuId: discordUserResult.rows[0].osuid
    };
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "getAssignmentByOsuId", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "getAssignmentByOsuId", "An error occurred while querying assignment: " + e.message);
      }
    }
    else {
      log(LogSeverity.ERROR, "getAssignmentByOsuId", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function getLastAssignmentUpdate(db) {
  if(!(db instanceof Client)) {
    log(LogSeverity.ERROR, "getLastAssignmentUpdate", "db must be a Client object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

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
    const result = await db.query(selectQuery);

    if(typeof(result.rows[0]) === "undefined") {
      return DatabaseErrors.NO_RECORD;
    }

    return result.rows[0].lastupdate;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "getLastAssignmentUpdate", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "getLastAssignmentUpdate", "An error occurred while querying assignment: " + e.message);
      }
    }
    else {
      log(LogSeverity.ERROR, "getLastAssignmentUpdate", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function insertOrUpdateAssignment(db, osuId, points, userName) {
  if(!(db instanceof Client)) {
    log(LogSeverity.ERROR, "insertOrUpdateAssignment", "db must be a Client object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    log(LogSeverity.ERROR, "insertOrUpdateAssignment", "osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

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

  const selectRoleQuery = "SELECT roleid, discordid, rolename, minpoints FROM roles WHERE minpoints<$1 ORDER BY minpoints DESC LIMIT 1";
  const selectRoleValues = [ points ];
  let insert = true;

  try {
    const assignmentResult = await db.query(selectAssignmentQuery, selectAssignmentValues);
    const currentRoleResult = await db.query(selectCurrentRoleQuery, selectCurrentRoleValues);

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
            return ret;
          }
        }

        userId = assignmentResult.rows[0].userid;
        discordId = assignmentResult.rows[0].discordid;

        await db.query("DELETE FROM assignments WHERE assignmentid=$1", [ assignmentResult.rows[0].assignmentid ]);

        query = "INSERT INTO assignments (assignmentid, userid, roleid, points, lastupdate) VALUES ($1, $2, $3, $4, $5)";
        values.push(assignmentResult.rows[0].assignmentid);
      }
      else {
        // should not fall here, but whatever
        log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Invalid osuId returned from the database.");
        return DatabaseErrors.CLIENT_ERROR;
      }
    }
    else {
      // userId not found, then insert
      const selectUserQuery = "SELECT userid, discordid, osuid FROM users WHERE osuid=$1";
      const selectUserValues = [ osuId ];

      const selectUserResult = await db.query(selectUserQuery, selectUserValues);

      if(selectUserResult.rows.length === 0) {
        log(LogSeverity.LOG, "insertOrUpdateAssignment", "User not found. Skipping assignment data update.");
        return DatabaseErrors.USER_NOT_FOUND;
      }

      query = "INSERT INTO assignments (userid, roleid, points, lastupdate) VALUES ($1, $2, $3, $4)";

      userId = selectUserResult.rows[0].userid;
      discordId = selectUserResult.rows[0].discordid;
    }

    const rolesResult = await db.query(selectRoleQuery, selectRoleValues);
    if(rolesResult.rows.length === 0) {
      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Role table is empty.");
      return DatabaseErrors.ROLES_EMPTY; // role data empty
    }
    
    if(rolesResult.rows[0].minPoints < points) {
      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Invalid role returned due to wrong minimum points.");
      return DatabaseErrors.CLIENT_ERROR;
    }

    values.push(userId, rolesResult.rows[0].roleid, points, new Date());
    await db.query(query, values);

    if(insert) {
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Inserted 1 row.");
    }
    else {
      log(LogSeverity.LOG, "insertOrUpdateAssignment", "assignment: Updated 1 row.");
    }

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
      type: insert ? AssignmentType.INSERT : AssignmentType.UPDATE,
      discordId,
      role,
      delta: insert ? points : points - assignmentResult.rows[0].points,
      lastUpdate: !insert ? assignmentResult.rows[0].lastupdate : null
    };
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "insertOrUpdateAssignment", "An error occurred while " + (insert ? "inserting" : "updating") + " assignment: " + e.message + "\n" + e.stack);
      }
    }
    else {
      log(LogSeverity.ERROR, "insertOrUpdateAssignment", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

module.exports = {
  getAllAssignments,
  getAssignmentByOsuId,
  getLastAssignmentUpdate,
  insertOrUpdateAssignment
};
