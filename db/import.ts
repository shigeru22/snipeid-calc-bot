import { Pool } from "pg";
import { LogSeverity, log } from "../src/utils/log";
import Config from "../config.json";

/**
 * Imports roles from into database.
 *
 * @param { Pool } db Database pool object.
 *
 * @returns { Promise<boolean> } Promise object, with `true` if roles were imported, `false` otherwise.
 */
async function importRoles(db: Pool): Promise<boolean> {
  log(LogSeverity.LOG, "importRoles", "Importing roles...");

  const roles = Config.roles;
  const len = roles.length;
  let query = "INSERT INTO roles (discordId, serverId, roleName, minPoints) VALUES ";
  roles.forEach((role, index) => {
    query += "('" + role.discordId + "', '1', '" + role.name + "', " + role.minPoints.toString() + ")";
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

export { importRoles };
