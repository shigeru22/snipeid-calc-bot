import dotenv from "dotenv";
import fs from "fs";
import { Pool } from "pg";
import { LogSeverity, log } from "../utils/log";
import { importRoles } from "./import";
import { createTables } from "./tables";
import { validateEnvironmentVariables, validateRolesConfig } from "./validate";

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

  if(typeof(dbConfig.ssl) !== "undefined") {
    log(LogSeverity.LOG, "main", `Using SSL, CA path: ${ process.env.DB_SSL_CA }`);
  }
  else {
    log(LogSeverity.WARN, "main", "Not using SSL. Caute procedere.");
  }

  const db = new Pool(dbConfig);

  try {
    log(LogSeverity.LOG, "main", "Connecting to database...");

    // test connection before continuing
    {
      const dbTemp = await db.connect();
      dbTemp.release();
      log(LogSeverity.LOG, "main", "Successfully connected to database.");
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
