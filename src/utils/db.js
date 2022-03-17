const { Pool } = require("pg");

const DatabaseErrors = {
  OK: 0,
  CONNECTION_ERROR: 1,
  TYPE_ERROR: 2,
  DUPLICATED_DISCORD_ID: 3,
  DUPLICATED_OSU_ID: 4,
  USER_NOT_FOUND: 5,
  ROLES_EMPTY: 6,
  CLIENT_ERROR: 7
};

const AssignmentType = {
  INSERT: 0,
  UPDATE: 1
};

/* user operations */

async function insertUser(pool, discordId, osuId) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] insertUser :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(discordId) !== "string") {
    console.log("[ERROR] insertUser :: discordId must be string.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] insertUser :: osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectDiscordIdQuery = "SELECT * FROM users WHERE discordId=$1";
  const selectDiscordIdValues = [ discordId ];

  const selectOsuIdQuery = "SELECT * FROM users WHERE osuId=$1;"
  const selectOsuIdValues = [ osuId ];

  const insertQuery = "INSERT INTO users (discordId, osuId) VALUES ($1, $2);";
  const insertValues = [ discordId, osuId ];

  try {  
    const client = await pool.connect();

    const discordIdResult = await client.query(selectDiscordIdQuery, selectDiscordIdValues);
    if(typeof(discordIdResult.rows[0]) !== "undefined") {
      if(discordIdResult.rows[0].discordid === discordId) {
        client.release();
        return DatabaseErrors.DUPLICATED_DISCORD_ID;
      }
    }
    
    const osuIdResult = await client.query(selectOsuIdQuery, selectOsuIdValues);
    if(typeof(osuIdResult.rows[0]) !== "undefined") {
      if(osuIdResult.rows[0].osuid === osuId) {
        client.release();
        return DatabaseErrors.DUPLICATED_OSU_ID;
      }
    }

    await client.query(insertQuery, insertValues);

    client.release();

    console.log("[LOG] insertUser :: users: Inserted 1 row.");
    return DatabaseErrors.OK;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        console.log("[ERROR] insertUser :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] insertUser :: An error occurred while inserting user: " + e.message);
      }
    }
    else {
      console.log("[ERROR] insertUser :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function getDiscordUserByOsuId(pool, osuId) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] getDiscordUserByOsuId :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] getDiscordUserByOsuId :: osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM users WHERE osuId=$1";
  const selectValues = [ osuId ];

  try {
    const client = await pool.connect();

    const discordUserResult = await client.query(selectQuery, selectValues);
    client.release();

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
        console.log("[ERROR] insertUser :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] insertUser :: An error occurred while inserting user: " + e.message);
      }
    }
    else {
      console.log("[ERROR] insertUser :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

/* assignment operations */

async function getAssignmentByOsuId(pool, osuId) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] getAssignmentByOsuId :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(userId) !== "number") {
    console.log("[ERROR] getAssignmentByOsuId :: userId must be number.");
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
    const client = await pool.connect();

    const discordUserResult = await client.query(selectQuery, selectValues);
    client.release();

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
        console.log("[ERROR] getAssignmentByOsuId :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] getAssignmentByOsuId :: An error occurred while querying assignment: " + e.message);
      }
    }
    else {
      console.log("[ERROR] getAssignmentByOsuId :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function insertOrUpdateAssignment(pool, osuId, points) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] insertOrUpdateAssignment :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] insertOrUpdateAssignment :: osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectAssignmentQuery = `
    SELECT
      a."assignmentid", a."userid", u."discordid", u."osuid", a."roleid", a."points", a."lastupdate"
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
    const client = await pool.connect();

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

        userId = assignmentResult.rows[0].userid;
        discordId = assignmentResult.rows[0].discordid;

        await client.query("DELETE FROM assignments WHERE assignmentid=$1", [ assignmentResult.rows[0].assignmentid ]);

        query = "INSERT INTO assignments (assignmentid, userid, roleid, points, lastupdate) VALUES ($1, $2, $3, $4, $5)";
        values.push(assignmentResult.rows[0].assignmentid);
      }
      else {
        client.release(); // should not fall here, but whatever
        console.log("[ERROR] insertOrUpdateAssignment :: Invalid osuId returned from the database.");
        return DatabaseErrors.CLIENT_ERROR;
      }
    }
    else {
      // userId not found, then insert
      const selectUserQuery = "SELECT userid, discordid, osuid FROM users WHERE osuid=$1";
      const selectUserValues = [ osuId ];

      const selectUserResult = await client.query(selectUserQuery, selectUserValues);

      if(selectUserResult.rows.length === 0) {
        client.release();
        console.log("[LOG] User not found. Won't update any data.");
        return DatabaseErrors.USER_NOT_FOUND;
      }

      query = "INSERT INTO assignments (userid, roleid, points, lastupdate) VALUES ($1, $2, $3, $4)";

      userId = selectUserResult.rows[0].userid;
      discordId = selectUserResult.rows[0].discordid;
    }

    const rolesResult = await client.query(selectRoleQuery, selectRoleValues);
    if(rolesResult.rows.length === 0) {
      client.release();
      console.log("[ERROR] insertOrUpdateAssignment :: Role table is empty.");
      return DatabaseErrors.ROLES_EMPTY; // role data empty
    }
    
    if(rolesResult.rows[0].minPoints < points) {
      client.release();
      console.log("[ERROR] insertOrUpdateAssignment :: Invalid role returned due to wrong minimum points.");
      return DatabaseErrors.CLIENT_ERROR;
    }

    values.push(userId, rolesResult.rows[0].roleid, points, new Date());
    await client.query(query, values);

    if(insert) {
      console.log("[LOG] insertOrUpdateAssignment :: assignment: Inserted 1 row.");
    }
    else {
      console.log("[LOG] insertOrUpdateAssignment :: assignment: Updated 1 row.");
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
        console.log("[ERROR] insertOrUpdateAssignment :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] insertOrUpdateAssignment :: An error occurred while " + (insert ? "inserting" : "updating") + " assignment: " + e.message + "\n" + e.stack);
      }
    }
    else {
      console.log("[ERROR] insertOrUpdateAssignment :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

/* roles operations */

async function getRolesList(pool) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] getRolesList :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM roles ORDER BY 4 DESC";

  try {
    const client = await pool.connect();

    const rolesResult = await client.query(selectQuery);
    if(typeof(rolesResult.rows) === "undefined" || rolesResult.rows.length === 0) {
      client.release();
      return DatabaseErrors.USER_NOT_FOUND; // role data empty
    }

    return rolesResult.rows;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        console.log("[ERROR] getRolesList :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] getRolesList :: An error occurred while querying roles: " + e.message);
      }
    }
    else {
      console.log("[ERROR] getRolesList :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

module.exports = {
  DatabaseErrors,
  AssignmentType,
  insertUser,
  getDiscordUserByOsuId,
  getAssignmentByOsuId,
  insertOrUpdateAssignment,
  getRolesList
};
