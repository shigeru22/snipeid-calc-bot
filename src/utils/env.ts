import { LogSeverity, log } from "./log";

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
    log(LogSeverity.LOG, "validateEnvironmentVariables", "Checking for environment variables...");

    if(typeof(process.env.BOT_NAME) !== "string" || !process.env.BOT_NAME) {
      log(LogSeverity.ERROR, "validateEnvironmentVariables", "BOT_NAME must be defined in environment variables. Exiting.");
      return false;
    }

    this.#botName = process.env.BOT_NAME;

    if(typeof(process.env.BOT_TOKEN) !== "string" || !process.env.BOT_TOKEN) {
      log(LogSeverity.ERROR, "validateEnvironmentVariables", "BOT_TOKEN must be defined in environment variables. Exiting.");
      return false;
    }

    this.#botToken = process.env.BOT_TOKEN;

    if(typeof(process.env.OSU_CLIENT_ID) !== "string" || !process.env.OSU_CLIENT_ID) {
      log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_ID must be defined in environment variables. Exiting.");
      return false;
    }

    if(isNaN(parseInt(process.env.OSU_CLIENT_ID, 10))) {
      log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_ID must be number. Exiting.");
      return false;
    }

    this.#osuClientId = parseInt(process.env.OSU_CLIENT_ID, 10);

    if(typeof(process.env.OSU_CLIENT_SECRET) !== "string" || !process.env.OSU_CLIENT_SECRET) {
      log(LogSeverity.ERROR, "validateEnvironmentVariables", "OSU_CLIENT_SECRET must be defined in environment variables. Exiting.");
      return false;
    }

    this.#osuClientSecret = process.env.OSU_CLIENT_SECRET;

    log(LogSeverity.LOG, "validateEnvironmentVariables", "Environment variable checks completed.");
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
