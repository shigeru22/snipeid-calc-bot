import { Log } from "./log";

/**
 * Environment variables class.
 */
class Environment {
  static #botName = "";
  static #botToken = "";
  static #osuClientId = 0;
  static #osuClientSecret = "";
  static #useRespektive = false;

  /**
   * Validates environment variables.
   *
   * @returns { boolean } `true` if environment variables are valid, `false` otherwise.
   */
  static validateEnvironmentVariables(): boolean {
    Log.info("validateEnvironmentVariables", "Checking for environment variables...");

    if(typeof(process.env.BOT_NAME) !== "string" || !process.env.BOT_NAME) {
      Log.error("validateEnvironmentVariables", "BOT_NAME must be defined in environment variables. Exiting.");
      return false;
    }

    Environment.#botName = process.env.BOT_NAME;

    if(typeof(process.env.BOT_TOKEN) !== "string" || !process.env.BOT_TOKEN) {
      Log.error("validateEnvironmentVariables", "BOT_TOKEN must be defined in environment variables. Exiting.");
      return false;
    }

    Environment.#botToken = process.env.BOT_TOKEN;

    if(typeof(process.env.OSU_CLIENT_ID) !== "string" || !process.env.OSU_CLIENT_ID) {
      Log.error("validateEnvironmentVariables", "OSU_CLIENT_ID must be defined in environment variables. Exiting.");
      return false;
    }

    if(isNaN(parseInt(process.env.OSU_CLIENT_ID, 10))) {
      Log.error("validateEnvironmentVariables", "OSU_CLIENT_ID must be number. Exiting.");
      return false;
    }

    Environment.#osuClientId = parseInt(process.env.OSU_CLIENT_ID, 10);

    if(typeof(process.env.OSU_CLIENT_SECRET) !== "string" || !process.env.OSU_CLIENT_SECRET) {
      Log.error("validateEnvironmentVariables", "OSU_CLIENT_SECRET must be defined in environment variables. Exiting.");
      return false;
    }

    Environment.#osuClientSecret = process.env.OSU_CLIENT_SECRET;

    Environment.#useRespektive = process.env.USE_RESPEKTIVE !== undefined && process.env.USE_RESPEKTIVE === "1";

    Log.info("validateEnvironmentVariables", "Environment variable checks completed.");
    return true;
  }

  static getBotName(): string {
    return Environment.#botName;
  }

  static getBotToken(): string {
    return Environment.#botToken;
  }

  static getOsuClientId(): number {
    return Environment.#osuClientId;
  }

  static getOsuClientSecret(): string {
    return Environment.#osuClientSecret;
  }

  static useRespektive(): boolean {
    return Environment.#useRespektive;
  }
}

export default Environment;
