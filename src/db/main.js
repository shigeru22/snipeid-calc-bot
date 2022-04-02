"use strict";

const dotenv = require("dotenv");
const { Pool } = require("pg");
const { LogSeverity, log } = require("../utils/log");
const { importRoles } = require("./import");
const { createTables } = require("./tables");
const { validateEnvironmentVariables, validateRolesConfig } = require("./validate");
const Config = require("../../config.json");

dotenv.config();

async function main() {
  if(!validateEnvironmentVariables()) {
    process.exit(1);
  }

  const roles = Config.roles;

  if(!validateRolesConfig(roles)) {
    process.exit(1);
  }

  log(LogSeverity.LOG, "main",
    "Using " + process.env.DB_USERNAME + "@" + process.env.DB_HOST + ":" + process.env.DB_PORT +
    ", in database named " + process.env.DB_DATABASE + "."
  );

  const db = new Pool({
    host: process.env.DB_HOST,
    port: parseInt(process.env.DB_PORT, 10),
    user: process.env.DB_USERNAME,
    password: process.env.DB_PASSWORD,
    database: process.env.DB_DATABASE
  });

  log(LogSeverity.LOG, "main", "Connecting to database...");
  await db.connect();

  if(!(await createTables(db))) {
    process.exit(1);
  }

  if(!(await importRoles(db, roles))) {
    process.exit(1);
  }

  log(LogSeverity.LOG, "main", "Data import finished successfully.");
  process.exit(0);
}

main();
