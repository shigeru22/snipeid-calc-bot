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

function validateRolesConfig(roles) {
  console.log("Validating role data from config.json...");

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
      console.log("[ERROR] An error occurred while validating role index " + i + ": name must be string and not empty.");
      console.log("Exiting...");
      return false;
    }

    if(typeof(roles[i].minPoints) !== "number" || !roles[i].minPoints) {
      console.log("[ERROR] An error occurred while validating role index " + i + ": minPoints must be number.");
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

module.exports = {
  validateEnvironmentVariables,
  validateRolesConfig
};
