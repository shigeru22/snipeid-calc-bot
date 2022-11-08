import { LogSeverity, log } from "./log";

/**
 * Validates environment variables.
 *
 * @returns { boolean } `true` if environment variables are valid, `false` otherwise.
 */
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

  if(typeof(process.env.OSU_CLIENT_ID) !== "string" || !process.env.OSU_CLIENT_ID) {
    log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(isNaN(parseInt(process.env.OSU_CLIENT_ID, 10))) {
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

export { validateEnvironmentVariables };
