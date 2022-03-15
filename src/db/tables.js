const { Client } = require("pg");

async function createTables(db) {
  console.log("Creating tables...");

  if(!(db instanceof Client)) {
    console.log("[ERROR] Invalid variable given: Not a node-postgres Client.");
    console.log("Exiting...");
    return false;
  }

  try {
    await db.query(`
      CREATE TABLE users (
        userId serial PRIMARY KEY,
        discordId varchar(255) NOT NULL,
        osuId integer NOT NULL
      );
    `);
    
    await db.query(`
      CREATE TABLE roles (
        roleId serial PRIMARY KEY,
        discordId varchar(255) NOT NULL,
        roleName varchar(255) NOT NULL,
        minPoints integer DEFAULT 0 NOT NULL
      );
    `);

    await db.query(`
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

    console.log("Table creation completed.");
    return true;
  }
  catch (e) {
    if(e instanceof Error) {
      console.log("[ERROR] An error occurred while querying database: " + e.message);
    }
    else {
      console.log("[ERROR] An unknown error occurred.");
    }

    return false;
  }
}

module.exports = {
  createTables
};
