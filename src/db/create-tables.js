"use strict"

const dotenv = require("dotenv");
const { Client } = require("pg");
const Config = require("../../config.json");

dotenv.config();

function validateEnvironmentVariables() {
  console.log("Checking for environment variables...");

  if(typeof(process.env.DB_HOST) !== "string" || !process.env.DB_HOST) {
    console.log("[ERROR] DB_HOST must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.DB_PORT) !== "string" || !process.env.DB_PORT) {
    console.log("[ERROR] DB_PORT must be defined in environment variables. Exiting.");
    return false;
  }

  if(parseInt(process.env.DB_PORT, 10) === NaN) {
    console.log("[ERROR] DB_PORT must be number. Exiting.");
    return false;
  }

  if(typeof(process.env.DB_USERNAME) !== "string" || !process.env.DB_USERNAME) {
    console.log("[ERROR] DB_USERNAME must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.DB_PASSWORD) !== "string" || !process.env.DB_PASSWORD) {
    console.log("[ERROR] DB_PASSWORD must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.DB_DATABASE) !== "string" || !process.env.DB_DATABASE) {
    console.log("[ERROR] DB_DATABASE must be defined in environment variables. Exiting.");
    return false;
  }

  console.log("Environment variable checks completed.");
  return true;
}

function validateRolesConfig() {
  console.log("Validating role data from config.json...");

  const roles = Config.roles;
  if(!roles) {
    console.log("[ERROR] Roles array is not defined. Define in config.json with the following format:");
    printRolesFormat();
    console.log("Exiting...");
    return false;
  }

  const len = roles.length;
  if(len <= 0) {
    console.log("[ERROR] Roles must not be empty. Define in config.json with the following format:");
    printRolesFormat();
    console.log("Exiting...");
    return false;
  }

  for(let i = 0; i < len; i++) {
    if(typeof(roles[i].discordId) !== "string" || !roles[i].discordId || roles[i].discordId.length <= 0) {
      console.log("[ERROR] An error occurred while validating role index " + i + ": discordId must be in snowflake ID string format.");
      console.log("Exiting...");
      return false;
    }

    try {
      const tempId = BigInt(roles[i].discordId);
      if(tempId.toString() !== roles[i].discordId) {
        console.log("[ERROR] An error occurred while validating role index " + i + ": discordId parsing outputs a different value.");
        console.log("Exiting...");
        return false;
      }
    }
    catch (e) {
      if(e instanceof Error) {
        console.log("[ERROR] An error occurred while validating role index " + i + ": " + e.message);
        console.log("Exiting...");
        return false;
      }
    }

    if(typeof(roles[i].name) !== "string" || !roles[i].name || roles[i].name === "") {
      console.log("[ERROR] An error occurred while validating role index " + i + ". name must be string and not empty.");
      console.log("Exiting...");
      return false;
    }
  }

  console.log("Role data validation completed.");
  return true;
}

function printRolesFormat() {
  console.log("  {");
  console.log("     \"roles\": [");
  console.log("       {");
  console.log("         \"discordId\": number,");
  console.log("         \"name\": string");
  console.log("       }, // ...");
  console.log("     ]");
  console.log("  }");
}

async function createTables(db) {
  console.log("Creating tables...");

  if(!(db instanceof Client)) {
    console.log("[ERROR] Invalid variable given: Not a node-postgres Client.");
    console.log("Exiting...");
    return false;
  }

  try {
    await db.connect();

    await db.query(`
      CREATE TABLE users (
        userId integer PRIMARY KEY,
        discordId varchar(255) NOT NULL,
        osuId integer NOT NULL
      );
    `);
    
    await db.query(`
      CREATE TABLE roles (
        roleId integer PRIMARY KEY,
        discordId varchar(255) NOT NULL,
        roleName varchar(255) NOT NULL
      );
    `);

    await db.query(`
      CREATE TABLE assignments (
        assignmentId integer PRIMARY KEY,
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
  finally {
    db.end();
  }
}

function importRoles(db) {
  console.log("Importing roles...");

  const roles = Config.roles;
}

async function main() {
  if(!validateEnvironmentVariables()) {
    process.exit(1);
  }

  if(!validateRolesConfig()) {
    process.exit(1);
  }

  console.log(
    "Using " + process.env.DB_USERNAME + "@" + process.env.DB_HOST + ":" + process.env.DB_PORT +
    ", in database named " + process.env.DB_DATABASE
  );

  const db = new Client({
    host: process.env.DB_HOST,
    port: parseInt(process.env.DB_PORT, 10),
    user: process.env.DB_USERNAME,
    password: process.env.DB_PASSWORD,
    database: process.env.DB_DATABASE
  });

  if(!(await createTables(db))) {
    process.exit(1);
  }

  importRoles(db);
}

main();
