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
    if(typeof(roles[i].discordId) !== "number" || !roles[i].discordId || roles[i].discordId <= 0) {
      console.log("[ERROR] An error occurred while validating role index " + i + ": discordId must be number and higher than 0.");
      console.log("Exiting...");
      return false;
    }

    if(typeof(roles[i].name) !== "string" || !roles[i].name || roles[i].name === "") {
      console.log("[ERROR] An error occurred while validating role index " + i + ". name must be string and not empty.");
      console.log("Exiting...");
      return false;
    }
  }

  console.log("Role data validation completed.")
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

function createTables() {
  console.log("Creating tables...");
}

function importRoles() {
  const roles = Config.roles;
  roles.forEach(role => console.log(role.discordId + ": " + role.name));
}

function main() {
  if(!validateEnvironmentVariables()) {
    process.exit(1);
  }

  if(!validateRolesConfig()) {
    process.exit(1);
  }

  createTables();
  importRoles();
}

main();
