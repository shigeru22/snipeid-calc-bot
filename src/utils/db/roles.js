const { DatabaseError } = require("pg");
const { LogSeverity, log } = require("../log");
const { DatabaseErrors } = require("../common");

/**
 * Returns list of roles in the database.
 *
 * @param { import("pg").Pool } db - Database connection pool.
 *
 * @returns { Promise<{ roleid: number; discordid: string; rolename: string; minpoints: number; }[] | number> } Promise object with roles array. Returns non-zero `DatabaseErrors` constant in case of errors.
 */
async function getRolesList(db) {
  const selectQuery = "SELECT * FROM roles ORDER BY 4 DESC";

  try {
    const rolesResult = await db.query(selectQuery);
    if(typeof(rolesResult.rows) === "undefined" || rolesResult.rows.length === 0) {
      return DatabaseErrors.ROLES_EMPTY;
    }

    return rolesResult.rows;
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getRolesList", "Database connection failed.");
          return DatabaseErrors.CONNECTION_ERROR;
        default:
          log(LogSeverity.ERROR, "getRolesList", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getRolesList", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getRolesList", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

module.exports = {
  getRolesList
};
