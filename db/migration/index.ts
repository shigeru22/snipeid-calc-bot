import fs from "fs";
import DotEnv from "dotenv";
import { Pool } from "pg";
import { LogSeverity, log } from "../../src/utils/log";
import { DatabaseErrors, DatabaseSuccess } from "../../src/utils/common";
import { insertServer } from "../../src/db/servers";
import { Tables } from "../tables";
import { addColumnToTable, alterTableColumn, getColumnNames } from "./tables";

DotEnv.config();

async function main() {
  const dbConfig = {
    host: process.env.DB_HOST,
    port: parseInt(process.env.DB_PORT as string, 10),
    user: process.env.DB_USERNAME,
    password: process.env.DB_PASSWORD,
    database: process.env.DB_DATABASE,
    ssl: (typeof(process.env.DB_SSL_CA) !== "undefined" ? {
      rejectUnauthorized: true,
      ca: fs.readFileSync(process.env.DB_SSL_CA).toString()
    } : undefined)
  };

  const db = new Pool(dbConfig);

  if(typeof(dbConfig.ssl) !== "undefined") {
    log(LogSeverity.LOG, "onStartup", `Using SSL for database connection, CA path: ${ process.env.DB_SSL_CA }`);
  }
  else {
    log(LogSeverity.WARN, "onStartup", "Not using SSL for database connection. Caute procedere.");
  }

  let serversTableExists = false;
  let assignmentsServerIdExists = false;
  let rolesServerIdExists = false;

  // database column check
  {
    log(LogSeverity.LOG, "main", "Querying database...");

    const serverTableData = await getColumnNames(db, "servers");
    const assignmentTableData = await getColumnNames(db, "assignments");
    const rolesTableData = await getColumnNames(db, "roles");

    if(serverTableData.status !== DatabaseSuccess.OK) {
      let exit = true;

      switch(serverTableData.status) {
        case DatabaseErrors.NO_RECORD:
          exit = false;
          break;
        case DatabaseErrors.CONNECTION_ERROR:
          log(LogSeverity.ERROR, "main", "Database connection error. Check database connectivity and try again.");
          break;
        case DatabaseErrors.CLIENT_ERROR:
          log(LogSeverity.ERROR, "main", "Client error detected. See previous logs for details.");
          break;
      }

      if(exit) {
        process.exit(1);
      }
    }
    else {
      log(LogSeverity.LOG, "main", "Servers table already exists.");
      serversTableExists = true;
    }

    if(assignmentTableData.status !== DatabaseSuccess.OK) {
      switch(assignmentTableData.status) {
        case DatabaseErrors.NO_RECORD:
          log(LogSeverity.ERROR, "main", "Assignments table doesn't exist. Initialize the database with this command:\n  npm run init-db\n");
          break;
        case DatabaseErrors.CONNECTION_ERROR:
          log(LogSeverity.ERROR, "main", "Database connection error. Check database connectivity and try again.");
          break;
        case DatabaseErrors.CLIENT_ERROR:
          log(LogSeverity.ERROR, "main", "Client error detected. See previous logs for details.");
          break;
      }

      process.exit(1);
    }

    if(rolesTableData.status !== DatabaseSuccess.OK) {
      switch(rolesTableData.status) {
        case DatabaseErrors.NO_RECORD:
          log(LogSeverity.ERROR, "main", "Roles table doesn't exist. Initialize the database with this command:\n  npm run init-db\n");
          break;
        case DatabaseErrors.CONNECTION_ERROR:
          log(LogSeverity.ERROR, "main", "Database connection error. Check database connectivity and try again.");
          break;
        case DatabaseErrors.CLIENT_ERROR:
          log(LogSeverity.ERROR, "main", "Client error detected. See previous logs for details.");
          break;
      }

      process.exit(1);
    }

    if(assignmentTableData.data.findIndex(column => column.columnName === "serverid") >= 0) {
      log(LogSeverity.LOG, "main", "Assignments table columns are up-to-date.");
      assignmentsServerIdExists = true;
    }

    if(rolesTableData.data.findIndex(column => column.columnName === "serverid") >= 0) {
      log(LogSeverity.LOG, "main", "Roles table columns are up-to-date.");
      rolesServerIdExists = true;
    }
  }

  if(serversTableExists && assignmentsServerIdExists && rolesServerIdExists) {
    log(LogSeverity.LOG, "main", "There is nothing to do. Exiting.");
    process.exit(0);
  }

  if(!serversTableExists) {
    log(LogSeverity.LOG, "main", "Creating servers table...");

    const result = await Tables.createServersTable(db);
    if(!result) {
      process.exit(1);
    }
  }

  log(LogSeverity.LOG, "main", "Inserting server based on .env file...");
  await insertServer(db, process.env.SERVER_ID as string);

  if(!assignmentsServerIdExists) {
    log(LogSeverity.LOG, "main", "Updating assignments table...");
    {
      const result = await addColumnToTable(db, "assignments", "serverId", "INTEGER NOT NULL DEFAULT 1", "servers", "serverId");
      if(result.status !== DatabaseSuccess.OK) {
        process.exit(1);
      }
    }
  }

  if(!rolesServerIdExists) {
    log(LogSeverity.LOG, "main", "Updating roles table...");

    const result = await addColumnToTable(db, "roles", "serverId", "INTEGER NOT NULL DEFAULT 1", "servers", "serverId");
    if(result.status !== DatabaseSuccess.OK) {
      process.exit(1);
    }
  }

  {
    log(LogSeverity.LOG, "main", "Resetting server ID column defaults...");

    if(!assignmentsServerIdExists) {
      const result = await alterTableColumn(db, "assignments", "serverId", "DROP DEFAULT");
      if(result.status !== DatabaseSuccess.OK) {
        process.exit(1);
      }
    }

    if(!rolesServerIdExists) {
      const result = await alterTableColumn(db, "roles", "serverId", "DROP DEFAULT");
      if(result.status !== DatabaseSuccess.OK) {
        process.exit(1);
      }
    }
  }

  log(LogSeverity.LOG, "main", "Database migration completed.");
  process.exit(0);
}

main();
