const { Pool } = require("pg");
const { DatabaseErrors } = require("../common");

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
      return DatabaseErrors.ROLES_EMPTY;
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
  getRolesList
};
