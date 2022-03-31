const { Client } = require("pg");
const { LogSeverity, log } = require("../log");
const { DatabaseErrors } = require("../common");

async function getRolesList(db) {
  if(!(db instanceof Client)) {
    log(LogSeverity.ERROR, "getRolesList", "db must be a Client object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM roles ORDER BY 4 DESC";

  try {
    const rolesResult = await db.query(selectQuery);
    if(typeof(rolesResult.rows) === "undefined" || rolesResult.rows.length === 0) {
      return DatabaseErrors.ROLES_EMPTY;
    }

    return rolesResult.rows;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "getRolesList", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "getRolesList", "An error occurred while querying roles: " + e.message);
      }
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
