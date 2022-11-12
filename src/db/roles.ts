import { Pool, DatabaseError } from "pg";
import { Log } from "../utils/log";
import { IDBServerRoleData, IDBServerRoleQueryData } from "../types/db/roles";
import { DatabaseClientError, DatabaseConnectionError, NoRecordError } from "../errors/db";

/**
 * Database `roles` table class.
 */
class DBRoles {
  /**
   * Returns list of server roles in the database.
   *
   * @param { Pool } db Database connection pool.
   * @param { string } serverDiscordId Server snowflake ID.
   *
   * @returns { Promise<IDBServerRoleData[]> } Promise object with server roles array.
   *
   * @throws { NoRecordError } No server roles data found in database.
   * @throws { DatabaseConnectionError } Database connection error occurred.
   * @throws { DatabaseClientError } Unhandled client error occurred.
   */
  static async getAllServerRoles(db: Pool, serverDiscordId: string): Promise<IDBServerRoleData[]> {
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

    let rolesResult;

    try {
      rolesResult = await db.query<IDBServerRoleQueryData>(selectQuery, selectValues);
    }
    catch (e) {
      if(e instanceof DatabaseError) {
        switch(e.code) {
          case "ECONNREFUSED":
            Log.error("getRolesList", "Database connection failed.");
            throw new DatabaseConnectionError();
          default:
            Log.error("getRolesList", `Unhandled database error occurred.\n${ e.stack }`);
        }
      }
      else if(e instanceof Error) {
        Log.error("getRolesList", `Unhandled error occurred.\n${ e.stack }`);
      }
      else {
        Log.error("getRolesList", "Unknown error occurred.");
      }

      throw new DatabaseClientError();
    }

    if(rolesResult.rows.length <= 0) {
      throw new NoRecordError();
    }

    return rolesResult.rows.map(row => ({
      roleId: row.roleid,
      discordId: row.discordid,
      roleName: row.rolename,
      minPoints: row.minpoints
    }));
  }
}

export default DBRoles;
