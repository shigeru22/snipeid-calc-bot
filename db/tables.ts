import { Pool } from "pg";
import { LogSeverity, log } from "../src/utils/log";

/**
 * Creates tables in the database.
 *
 * @param { Pool } db - Database pool object.
 *
 * @returns { Promise<boolean> } Promise object, with `true` if tables were created, `false` otherwise.
 *
 * @deprecated This will create the old version (v1) of those tables. Use `create` methods from `Table` class instead.
 */
async function createTables(db: Pool): Promise<boolean> {
  log(LogSeverity.LOG, "createTables", "Creating tables...");

  try {
    const client = await db.connect();

    await client.query(`
      CREATE TABLE users (
        userId serial PRIMARY KEY,
        discordId varchar(255) NOT NULL,
        osuId integer NOT NULL,
        userName varchar(255) NOT NULL
      );
    `);

    await client.query(`
      CREATE TABLE roles (
        roleId serial PRIMARY KEY,
        discordId varchar(255) NOT NULL,
        roleName varchar(255) NOT NULL,
        minPoints integer DEFAULT 0 NOT NULL
      );
    `);

    await client.query(`
      CREATE TABLE assignments (
        assignmentId serial PRIMARY KEY,
        userId integer NOT NULL,
        roleId integer NOT NULL,
        points integer DEFAULT 0,
        lastUpdate timestamp DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT fk_user
          FOREIGN KEY(userId) REFERENCES users(userId),
        CONSTRAINT fk_role
          FOREIGN KEY(roleId) REFERENCES roles(roleId)
      );
    `);

    client.release();

    log(LogSeverity.LOG, "createTables", "Table creation completed.");
    return true;
  }
  catch (e) {
    if(e instanceof Error) {
      log(LogSeverity.LOG, "createTables", "An error occurred while querying database: " + e.message);
    }
    else {
      log(LogSeverity.LOG, "createTables", "An unknown error occurred.");
    }

    return false;
  }
}

class Tables {
  /**
   * Creates all tables in the database (v2).
   *
   * @param { Pool } db - Database pool object.
   *
   * @returns { Promise<boolean> } Promise object, with `true` if all tables were created, `false` otherwise.
   */
  static async createAllTables(db: Pool): Promise<boolean> {
    /*
     * Order of execution:
     * users -> servers -> members -> roles -> assignments
     */

    if(!(await this.createUsersTable(db))) {
      return false;
    }

    if(!(await this.createServersTable(db))) {
      return false;
    }

    if(!(await this.createMembersTable(db))) {
      return false;
    }

    if(!(await this.createRolesTable(db))) {
      return false;
    }

    if(!(await this.createAssignmentsTable(db))) {
      return false;
    }

    return true;
  }

  /**
   * Creates user table in the database (v2).
   *
   * @param { Pool } db - Database pool object.
   *
   * @returns { Promise<boolean> } Promise object, with `true` if the table were created, `false` otherwise.
   */
  static async createUsersTable(db: Pool): Promise<boolean> {
    log(LogSeverity.LOG, "createUsersTable", "Creating tables...");

    const query = `
      CREATE TABLE users (
        userId SERIAL PRIMARY KEY,
        discordId VARCHAR(255) NOT NULL,
        osuId INTEGER NOT NULL,
        userName VARCHAR(255) NOT NULL
      )
    `;

    try {
      const client = await db.connect();

      await client.query(query);

      log(LogSeverity.LOG, "createUsersTable", "Table creation completed.");
      return true;
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.LOG, "createUsersTable", "An error occurred while querying database: " + e.message);
      }
      else {
        log(LogSeverity.LOG, "createUsersTable", "An unknown error occurred.");
      }

      return false;
    }
  }

  /**
   * Creates servers table in the database (v2).
   *
   * @param { Pool } db - Database pool object.
   *
   * @returns { Promise<boolean> } Promise object, with `true` if the table were created, `false` otherwise.
   */
  static async createServersTable(db: Pool): Promise<boolean> {
    log(LogSeverity.LOG, "createServersTable", "Creating tables...");

    const query = `
      CREATE TABLE servers (
        serverId SERIAL PRIMARY KEY,
        discordId VARCHAR(255) NOT NULL,
        country VARCHAR(2),
        verifyChannelId VARCHAR(255) DEFAULT NULL,
        verifiedRoleId VARCHAR(255) DEFAULT NULL,
        commandsChannelId VARCHAR(255) DEFAULT NULL,
        leaderboardsChannelId VARCHAR(255) DEFAULT NULL
      )
    `;

    try {
      const client = await db.connect();

      await client.query(query);

      log(LogSeverity.LOG, "createServersTable", "Table creation completed.");
      return true;
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.LOG, "createServersTable", "An error occurred while querying database: " + e.message);
      }
      else {
        log(LogSeverity.LOG, "createServersTable", "An unknown error occurred.");
      }

      return false;
    }
  }

  /**
   * Creates members table in the database (v2).
   *
   * @param { Pool } db - Database pool object.
   *
   * @returns { Promise<boolean> } Promise object, with `true` if the table were created, `false` otherwise.
   */
  static async createMembersTable(db: Pool): Promise<boolean> {
    log(LogSeverity.LOG, "createMembersTable", "Creating tables...");

    const query = `
      CREATE TABLE members (
        memberId SERIAL PRIMARY KEY,
        userId INTEGER NOT NULL,
        serverId INTEGER NOT NULL,
        CONSTRAINT fk_user
          FOREIGN KEY(userId) REFERENCES users(userId),
        CONSTRAINT fk_server
          FOREIGN KEY(serverId) REFERENCES servers(serverId)
      )
    `;

    try {
      const client = await db.connect();

      await client.query(query);

      log(LogSeverity.LOG, "createMembersTable", "Table creation completed.");
      return true;
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.LOG, "createMembersTable", "An error occurred while querying database: " + e.message);
      }
      else {
        log(LogSeverity.LOG, "createMembersTable", "An unknown error occurred.");
      }

      return false;
    }
  }

  /**
   * Creates roles table in the database (v2).
   *
   * @param { Pool } db - Database pool object.
   *
   * @returns { Promise<boolean> } Promise object, with `true` if the table were created, `false` otherwise.
   */
  static async createRolesTable(db: Pool): Promise<boolean> {
    log(LogSeverity.LOG, "createRolesTable", "Creating tables...");

    const query = `
      CREATE TABLE roles (
        roleId SERIAL PRIMARY KEY,
        discordId VARCHAR(255) NOT NULL,
        serverId INTEGER NOT NULL,
        roleName VARCHAR(255) NOT NULL,
        minPoints INTEGER DEFAULT 0 NOT NULL,
        CONSTRAINT fk_server
          FOREIGN KEY(serverId) REFERENCES servers(serverId)
      )
    `;

    try {
      const client = await db.connect();

      await client.query(query);

      log(LogSeverity.LOG, "createRolesTable", "Table creation completed.");
      return true;
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.LOG, "createRolesTable", "An error occurred while querying database: " + e.message);
      }
      else {
        log(LogSeverity.LOG, "createRolesTable", "An unknown error occurred.");
      }

      return false;
    }
  }

  /**
   * Creates assignments table in the database (v2).
   *
   * @param { Pool } db - Database pool object.
   *
   * @returns { Promise<boolean> } Promise object, with `true` if the table were created, `false` otherwise.
   */
  static async createAssignmentsTable(db: Pool): Promise<boolean> {
    log(LogSeverity.LOG, "createAssignmentsTable", "Creating tables...");

    const query = `
      CREATE TABLE assignments (
        assignmentId SERIAL PRIMARY KEY,
        userId INTEGER NOT NULL,
        serverId INTEGER NOT NULL,
        roleId INTEGER NOT NULL,
        points INTEGER DEFAULT 0 NOT NULL,
        lastUpdate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT fk_user
          FOREIGN KEY(userId) REFERENCES users(userId),
        CONSTRAINT fk_server
          FOREIGN KEY(serverId) REFERENCES servers(serverId),
        CONSTRAINT fk_role
          FOREIGN KEY(roleId) REFERENCES roles(roleId)
      )
    `;

    try {
      const client = await db.connect();

      await client.query(query);

      log(LogSeverity.LOG, "createAssignmentsTable", "Table creation completed.");
      return true;
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.LOG, "createAssignmentsTable", "An error occurred while querying database: " + e.message);
      }
      else {
        log(LogSeverity.LOG, "createAssignmentsTable", "An unknown error occurred.");
      }

      return false;
    }
  }
}

export { createTables, Tables };
