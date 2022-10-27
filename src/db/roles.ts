import { Pool, DatabaseError } from "pg";
import { LogSeverity, log } from "../utils/log";
import { DatabaseErrors, DatabaseSuccess } from "../utils/common";
import { DBResponseBase } from "../types/db/main";
import { IDBServerRoleData, IDBServerRoleQueryData } from "../types/db/roles";

/**
 * Returns list of roles in the database.
 *
 * @param { Pool } db - Database connection pool.
 *
 * @returns { Promise<DBResponseBase<IDBServerRoleData> | DBResponseBase<DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with roles array.
 */
async function getRolesList(db: Pool, serverDiscordId: string): Promise<DBResponseBase<IDBServerRoleData[]> | DBResponseBase<DatabaseErrors.ROLES_EMPTY | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      roles."roleid",
      roles."discordid",
      roles."rolename",
      roles."minpoints"
    FROM
      roles
    JOIN
      servers ON roles."serverid" = servers."serverid"
    WHERE
      servers."discordid" = $1
    ORDER BY
      minPoints DESC
  `;
  const selectValues = [ serverDiscordId ];

  try {
    const rolesResult = await db.query<IDBServerRoleQueryData>(selectQuery, selectValues);
    if(typeof(rolesResult.rows) === "undefined" || rolesResult.rows.length === 0) {
      return {
        status: DatabaseErrors.ROLES_EMPTY
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: rolesResult.rows.map(row => ({
        roleId: row.roleid,
        discordId: row.discordid,
        roleName: row.rolename,
        minPoints: row.minpoints
      }))
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
