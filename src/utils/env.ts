import { Log } from "./log";

class Environment {
  static #botName = "";
  static #botToken = "";
  static #osuClientId = 0;
  static #osuClientSecret = "";

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

    this.#botName = process.env.BOT_NAME;

    if(typeof(process.env.BOT_TOKEN) !== "string" || !process.env.BOT_TOKEN) {
      Log.error("validateEnvironmentVariables", "BOT_TOKEN must be defined in environment variables. Exiting.");
      return false;
    }

    this.#botToken = process.env.BOT_TOKEN;

    if(typeof(process.env.OSU_CLIENT_ID) !== "string" || !process.env.OSU_CLIENT_ID) {
      Log.error("validateEnvironmentVariables", "OSU_CLIENT_ID must be defined in environment variables. Exiting.");
      return false;
    }

    if(isNaN(parseInt(process.env.OSU_CLIENT_ID, 10))) {
      Log.error("validateEnvironmentVariables", "OSU_CLIENT_ID must be number. Exiting.");
      return false;
    }

    this.#osuClientId = parseInt(process.env.OSU_CLIENT_ID, 10);

    if(typeof(process.env.OSU_CLIENT_SECRET) !== "string" || !process.env.OSU_CLIENT_SECRET) {
      Log.error("validateEnvironmentVariables", "OSU_CLIENT_SECRET must be defined in environment variables. Exiting.");
      return false;
    }

    this.#osuClientSecret = process.env.OSU_CLIENT_SECRET;

    Log.info("validateEnvironmentVariables", "Environment variable checks completed.");
    return true;
  }

  static getBotName(): string {
    return this.#botName;
  }

  static getBotToken(): string {
    return this.#botToken;
  }

  static getOsuClientId(): number {
    return this.#osuClientId;
  }

  static getOsuClientSecret(): string {
    return this.#osuClientSecret;
  }
}

export default Environment;
