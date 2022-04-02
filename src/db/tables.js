const { Pool } = require("pg");
const { LogSeverity, log } = require("../utils/log");

async function createTables(db) {
  log(LogSeverity.LOG, "createTables", "Creating tables...");

  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "createTables", "db must be a Pool object instance.");
    return false;
  }

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

module.exports = {
  createTables
};
