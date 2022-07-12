const { Pool } = require("pg");
const { LogSeverity, log } = require("../utils/log");

/**
 * Imports roles from into database.
 *
 * @param { Pool } db
 * @param { { discordId: string; name: string; minPoints: number; }[] }
 *
 * @returns { Promise<boolean> }
 */
async function importRoles(db, roles) {
  log(LogSeverity.LOG, "importRoles", "Importing roles...");

  const len = roles.length;
  let query = "INSERT INTO roles (discordId, roleName, minPoints) VALUES ";
  roles.forEach((role, index) => {
    query += "('" + role.discordId + "', '" + role.name + "', " + role.minPoints.toString() + ")";
    if(index < len - 1) {
      query += ", ";
    }
    else {
      query += ";";
    }
  });

  try {
    await db.query(query);

    log(LogSeverity.LOG, "importRoles", "Role import completed.");
    return true;
  }
  catch (e) {
    if(e instanceof Error) {
      log(LogSeverity.ERROR, "importRoles", "An error occurred while querying database: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "importRoles", "An unknown error occurred.");
    }

    return false;
  }
}

module.exports = {
  importRoles
};
