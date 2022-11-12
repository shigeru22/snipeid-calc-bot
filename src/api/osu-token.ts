import { getAccessToken, revokeAccessToken } from "./osu";
import { Log } from "../utils/log";

/**
 * osu! API token class.
 */
class OsuToken {
  #clientId = 0;
  #clientSecret = "";
  #token: string | null = null;
  #expirationTime = new Date(0);

  /**
   * Instantiates `OsuToken` object. Also retrieves token if specified.
   *
   * @param { number } clientId osu! API client ID.
   * @param { string } clientSecret osu! API client secret.
   * @param { boolean } retrieve whether to retrieve token immediately after instantiating the object.
   */
  constructor(clientId: number, clientSecret: string, retrieve = false) {
    this.#clientId = clientId;
    this.#clientSecret = clientSecret;

    if(typeof(retrieve) === "boolean" && retrieve) {
      this.getToken();
    }
  }

  /**
   * Retrieves current token. If expired, new token will be retrieved.
   *
   * @returns { Promise<string | null> } Promise object with token response.
   */
  async getToken(): Promise<string | null> {
    const now = new Date();

    if(now.getTime() >= this.#expirationTime.getTime()) {
      if(this.#token !== "") {
        Log.info("getToken", "Access token expired. Requesting new osu! access token...");
      }
      else {
        Log.info("getToken", "Requesting new osu! access token...");
      }

      let response;

      try {
        response = await getAccessToken(this.#clientId, this.#clientSecret);
      }
      catch (e) {
        Log.error("getToken", "API request error occurred. See above log for details.");
        return null;
      }

      this.#token = response.token as string;
      this.#expirationTime = response.expire as Date;
    }

    return this.#token;
  }

  /**
   * Revokes current token.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  async revokeToken(): Promise<void> {
    Log.info("revokeToken", "Revoking osu! access token...");

    if(this.#token === null) {
      Log.warn("revokeToken", "No token was requested. Skipping process.");
      return;
    }

    try {
      await revokeAccessToken(this.#token);
    }
    catch {
      Log.error("getToken", "Client error occurred. See above log for details.");
      return;
    }

    this.#token = null;
  }
}

export { OsuToken };
