const axios = require("axios").default;
const { LogSeverity, log } = require("../log");
const { HTTPStatus, OsuUserStatus } = require("../common");

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";
const OSU_TOKEN_ENDPOINT = "https://osu.ppy.sh/oauth/token";

/**
 * Gets access token using osu! client ID and secret.
 *
 * @param { string } clientId
 * @param { string } clientSecret
 *
 * @returns { Promise<string> }
 */
async function getAccessToken(clientId, clientSecret) {
  const id = parseInt(clientId, 10); // no need to validate since already validated in env module
  const secret = clientSecret;

  try {
    const response = await axios.post(OSU_TOKEN_ENDPOINT, {
      client_id: id,
      client_secret: secret,
      grant_type: "client_credentials",
      scope: "public"
    });

    let ret = {};

    if(response.status === HTTPStatus.OK) {
      ret = {
        token: response.data.access_token,
        expire: new Date((new Date()).getTime() + (response.data.expires_in * 1000))
      };
    }
    
    return ret;
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      log(LogSeverity.ERROR, "getAccessToken", e.name + ": " + e.message);

      if(response.status === HTTPStatus.UNAUTHORIZED) {
        log(LogSeverity.ERROR, "getAccessToken", "Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAccessToken", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getAccessToken", "Unknown error occurred.");
    }

    process.exit(1);
  }
}

/**
 * Gets user information for this bot by osu! ID.
 *
 * @param { string } token
 * @param { number } id
 *
 * @returns { Promise<{ status: number; username?: string; isCountryCodeAllowed?: boolean; }> }
 */
async function getUserByOsuId(token, id) {
  try {
    const response = await axios.get(OSU_API_ENDPOINT + "/users/" + id.toString(), {
      params: {
        key: "id"
      },
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Authorization": "Bearer " + token
      }
    });

    let res = {};

    if(response.status === HTTPStatus.OK) {
      if(response.data.is_bot) {
        res = {
          status: OsuUserStatus.BOT
        };
      }
      else if(response.data.is_deleted) {
        res = {
          status: OsuUserStatus.DELETED
        };
      }
      else {
        res = {
          status: OsuUserStatus.USER,
          username: response.data.username,
          isCountryCodeAllowed: response.data.country.code === process.env.COUNTRY_CODE
        };
      }
    }

    return res;
  }
  catch (e) {
    let res = {};

    if(axios.isAxiosError(e)) {
      if(e.response.status === HTTPStatus.UNAUTHORIZED) {
        exitOnUnauthorizedError();
      }
      else if(e.response.status === HTTPStatus.NOT_FOUND) {
        res = {
          status: OsuUserStatus.NOT_FOUND
        }
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getUserByOsuId", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getUserByOsuId", "Unknown error occurred.");
      process.exit(1);
    }

    return res;
  }
}

module.exports = {
  getAccessToken,
  getUserByOsuId
};
