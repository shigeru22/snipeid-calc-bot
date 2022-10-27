import { getAccessToken, revokeAccessToken } from "./osu";
import { OsuApiSuccessStatus, OsuApiErrorStatus } from "../utils/common";
import { LogSeverity, log } from "../utils/log";

/**
 * osu! API token class.
 */
class OsuToken {
  #clientId = "";
  #clientSecret = "";
  #token = "";
  #expirationTime = new Date(0);

  /**
   * Instantiates `OsuToken` object. Also retrieves token if specified.
   *
   * @param { string } clientId - osu! API client ID.
   * @param { string } clientSecret - osu! API client secret.
   * @param { boolean } retrieve - whether to retrieve token immediately after instantiating the object.
   */
  constructor(clientId: string, clientSecret: string, retrieve = false) {
    this.#clientId = clientId;
    this.#clientSecret = clientSecret;

    if(typeof(retrieve) === "boolean" && retrieve) {
      this.getToken();
    }
  }

  /**
   * Retrieves current token. If expired, new token will be retrieved.
   *
   * @returns { Promise<string> } Promise object with token response.
   */
  async getToken(): Promise<string> {
    const now = new Date();

    if(now.getTime() >= this.#expirationTime.getTime()) {
      if(this.#token !== "") {
        log(LogSeverity.LOG, "getToken", "Access token expired. Requesting new osu! access token...");
      }
      else {
        log(LogSeverity.LOG, "getToken", "Requesting new osu! access token...");
      }

      const response = await getAccessToken(this.#clientId, this.#clientSecret);

      if(Object.keys(response).length === 0) {
        log(LogSeverity.WARN, "getToken", "Unable to request access token. osu! API might be down?");
        return "";
      }

      if(response.status !== OsuApiSuccessStatus.OK) {
        switch(response.status) {
          case OsuApiErrorStatus.CLIENT_ERROR:
            log(LogSeverity.ERROR, "getToken", "Client error occurred. See above log for details.");
            break;
          default:
            log(LogSeverity.ERROR, "getToken", `osu! API returned status code ${ response.toString() }.`);
        }

        return "";
      }

      this.#token = response.data.token as string;
      this.#expirationTime = response.data.expire as Date;
    }

    return this.#token;
  }

  /**
   * Revokes current token.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  async revokeToken(): Promise<void> {
    log(LogSeverity.LOG, "revokeToken", "Revoking osu! access token...");

    if(this.#token === "") {
      log(LogSeverity.WARN, "revokeToken", "No token was requested. Skipping process.");
      return;
    }

    const response = await revokeAccessToken(this.#token);

    if(response.status !== OsuApiSuccessStatus.OK) {
      log(LogSeverity.ERROR, "revokeToken", "Unable to revoke access token. Check logs above.");
      return;
    }

    this.#token = "";
  }
}

export { OsuToken };
