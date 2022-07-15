"use strict";

const dotenv = require("dotenv");
const { Pool } = require("pg");
const { LogSeverity, log } = require("../utils/log");
const { importRoles } = require("./import");
const { createTables } = require("./tables");
const { validateEnvironmentVariables, validateRolesConfig } = require("./validate");

// configure environment variable file (if any)
dotenv.config();

// main function
async function main() {
  if(!validateEnvironmentVariables()) {
    process.exit(1);
  }

  if(!validateRolesConfig()) {
    process.exit(1);
  }

  log(LogSeverity.LOG,
    "main",
    "Using " + process.env.DB_USERNAME + "@" + process.env.DB_HOST + ":" + process.env.DB_PORT + ", in database named " + process.env.DB_DATABASE + "."
  );

  const db = new Pool({
    host: process.env.DB_HOST,
    port: parseInt(process.env.DB_PORT, 10),
    user: process.env.DB_USERNAME,
    password: process.env.DB_PASSWORD,
    database: process.env.DB_DATABASE
  });

  try {
    log(LogSeverity.LOG, "main", "Connecting to database...");

    // test connection before continuing
    {
      const dbTemp = await db.connect();
      dbTemp.release();
    }

    if(!(await createTables(db))) {
      process.exit(1);
    }

    if(!(await importRoles(db))) {
      process.exit(1);
    }
  }
  catch (e) {
    if(e instanceof Error) {
      log(LogSeverity.ERROR, "main", `Database operations failed.\n${ e.name }: ${ e.message }\n${ e.stack }`);
    }
    else {
      log(LogSeverity.ERROR, "main", "Unknown error occurred while doing database operations.");
    }

    process.exit(1);
  }

  log(LogSeverity.LOG, "main", "Data import finished successfully.");
  process.exit(0);
}

// run the function
main();
