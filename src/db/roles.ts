import { Pool, DatabaseError } from "pg";
import { LogSeverity, log } from "../utils/log";
import { DatabaseErrors } from "../utils/common";

// TODO: convert compound object return types into interfaces

// TODO: create conditional types

/**
 * Returns list of roles in the database.
 *
 * @param { Pool } db - Database connection pool.
 *
 * @returns { Promise<{ status: DatabaseErrors.OK | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR, roles?: { roleid: number; discordid: string; rolename: string; minpoints: number; }[]; }> } Promise object with roles array.
 */
async function getRolesList(db: Pool): Promise<{ status: DatabaseErrors.OK | DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR; roles?: { roleid: number; discordid: string; rolename: string; minpoints: number; }[]; }> {
  const selectQuery = "SELECT * FROM roles ORDER BY 4 DESC";

  try {
    const rolesResult = await db.query(selectQuery); // TODO: add type annotation to queries
    if(typeof(rolesResult.rows) === "undefined" || rolesResult.rows.length === 0) {
      return {
        status: DatabaseErrors.ROLES_EMPTY
      };
    }

    return {
      status: DatabaseErrors.OK,
      roles: rolesResult.rows
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getRolesList", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
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

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

export { getRolesList };
