const { getAccessToken } = require("./api/osu");
const { LogSeverity, log} = require("./log");

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
   * @param { string } clientId
   * @param { string } clientSecret
   * @param { boolean | undefined } retrieve
   *
   * @returns { OsuToken }
   */
  constructor(clientId, clientSecret, retrieve) {
    this.#clientId = clientId;
    this.#clientSecret = clientSecret;

    if(typeof(retrieve) === "boolean" && retrieve) {
      this.getToken();
    }
  }

  /**
   * Retrieves current token. If expired, new token will be retrieved.
   *
   * @returns { string }
   */
  async getToken() {
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
        return 0;
      }
      else {
        this.#token = response.token;
        this.#expirationTime = response.expire;
      }
    }

    return this.#token;
  }
}

module.exports = {
  OsuToken
};

