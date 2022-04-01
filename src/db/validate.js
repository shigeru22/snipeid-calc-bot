const { LogSeverity, log } = require("../utils/log");

function validateEnvironmentVariables() {
  log(LogSeverity.LOG, "validateEnvironmentVariables", "Checking for environment variables...");

  if(typeof(process.env.DB_HOST) !== "string" || !process.env.DB_HOST) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "DB_HOST must be defined in environment variables.");
    return false;
  }

  if(typeof(process.env.DB_PORT) !== "string" || !process.env.DB_PORT) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "DB_PORT must be defined in environment variables.");
    return false;
  }

  if(parseInt(process.env.DB_PORT, 10) === NaN) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "DB_PORT must be number.");
    return false;
  }

  if(typeof(process.env.DB_USERNAME) !== "string" || !process.env.DB_USERNAME) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "DB_USERNAME must be defined in environment variables.");
    return false;
  }

  if(typeof(process.env.DB_PASSWORD) !== "string" || !process.env.DB_PASSWORD) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "DB_PASSWORD must be defined in environment variables.");
    return false;
  }

  if(typeof(process.env.DB_DATABASE) !== "string" || !process.env.DB_DATABASE) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "DB_DATABASE must be defined in environment variables.");
    return false;
  }

  log(LogSeverity.LOG, "validateEnvironmentVariables", "Environment variable checks completed.");
  return true;
}

function validateRolesConfig(roles) {
  log(LogSeverity.LOG, "validateRolesConfig", "Validating role data from config.json...");

  if(!roles) {
    log(LogSeverity.ERROR, "validateRolesConfig", "Roles array is not defined. Define in config.json with the following format:");
    printRolesFormat();
    return false;
  }

  const len = roles.length;
  if(len <= 0) {
    log(LogSeverity.ERROR, "validateRolesConfig", "Roles must not be empty. Define in config.json with the following format:");
    printRolesFormat();
    return false;
  }

  for(let i = 0; i < len; i++) {
    if(typeof(roles[i].discordId) !== "string" || !roles[i].discordId || roles[i].discordId.length <= 0) {
      log(LogSeverity.ERROR, "validateRolesConfig", "An error occurred while validating role index " + i + ": discordId must be in snowflake ID string format.");
      return false;
    }

    try {
      const tempId = BigInt(roles[i].discordId);
      if(tempId.toString() !== roles[i].discordId) {
        log(LogSeverity.ERROR, "validateRolesConfig", "An error occurred while validating role index " + i + ": discordId parsing outputs a different value.");
        return false;
      }
    }
    catch (e) {
      if(e instanceof Error) {
        log(LogSeverity.ERROR, "validateRolesConfig", "An error occurred while validating role index " + i + ": " + e.message);
        return false;
      }
    }

    if(typeof(roles[i].name) !== "string" || !roles[i].name || roles[i].name === "") {
      log(LogSeverity.ERROR, "validateRolesConfig", "An error occurred while validating role index " + i + ": name must be string and not empty.");
      return false;
    }

    if(typeof(roles[i].minPoints) !== "number" || roles[i].minPoints < 0) {
      log(LogSeverity.ERROR, "validateRolesConfig", "An error occurred while validating role index " + i + ": minPoints must be number.");
      return false;
    }
  }

  log(LogSeverity.LOG, "validateRolesConfig", "Role data validation completed.");
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

module.exports = {
  validateEnvironmentVariables,
  validateRolesConfig
};
