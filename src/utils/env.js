const { LogSeverity, log } = require("../utils/log");

function validateEnvironmentVariables() {
  log(LogSeverity.LOG, "validateEnvironmentVariables", "Checking for environment variables...");

  if(typeof(process.env.BOT_NAME) !== "string" || !process.env.BOT_NAME) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "BOT_NAME must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.BOT_TOKEN) !== "string" || !process.env.BOT_TOKEN) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "BOT_TOKEN must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.CHANNEL_ID) !== "string" || !process.env.CHANNEL_ID) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "CHANNEL_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.LEADERBOARD_CHANNEL_ID) !== "string" || !process.env.LEADERBOARD_CHANNEL_ID) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "LEADERBOARD_CHANNEL_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.VERIFICATION_CHANNEL_ID) !== "string" || !process.env.VERIFICATION_CHANNEL_ID) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "VERIFICATION_CHANNEL_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.COUNTRY_CODE) !== "string" || !process.env.COUNTRY_CODE) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "COUNTRY_CODE must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.SERVER_ID) === "string" && process.env.SERVER_ID !== "") {
    // verify role ID if server ID is defined
    if(typeof(process.env.VERIFIED_ROLE_ID) !== "string" || !process.env.VERIFIED_ROLE_ID) {
      log(LogSeverity.ERROR, "validateEnvironmentVariables", "If SERVER_ID is defined, VERIFIED_ROLE_ID must be defined in environment variables. Exiting.");
      return false;
    }
  }

  if(typeof(process.env.OSU_CLIENT_ID) !== "string" || !process.env.OSU_CLIENT_ID) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(parseInt(process.env.OSU_CLIENT_ID, 10) === NaN) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_ID must be number. Exiting.");
    return false;
  }

  if(typeof(process.env.OSU_CLIENT_SECRET) !== "string" || !process.env.OSU_CLIENT_SECRET) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_SECRET must be defined in environment variables. Exiting.");
    return false;
  }

  log(LogSeverity.LOG, "validateEnvironmentVariables", "Environment variable checks completed.");
  return true;
}

module.exports = {
  validateEnvironmentVariables
};
