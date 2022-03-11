"use strict"

const dotenv = require("dotenv");
const { Client } = require("pg");

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

function createTables() {
  console.log("Creating tables...");
}

function main() {
  if(!validateEnvironmentVariables()) {
    process.exit(1);
  }

  createTables();
}

main();