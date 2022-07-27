const axios = require("axios").default;
const { LogSeverity, log } = require("../log");
const { HTTPStatus, OsuUserStatus, OsuApiStatus } = require("../common");

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";
const OSU_TOKEN_ENDPOINT = "https://osu.ppy.sh/oauth/token";

/**
 * Gets access token using osu! client ID and secret.
 *
 * @param { string } clientId - osu! client ID.
 * @param { string } clientSecret - osu! client secret.
 *
 * @returns { Promise<{ token: string; expire: Date; } | number> } Promise object with access token and expiration date. Returns `OsuApiStatus` constant in case of errors.
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

    if(response.status !== HTTPStatus.OK) {
      log(LogSeverity.ERROR, "getAccessToken", "osu! API returned status code " + response.status.toString() + ".");
      return OsuApiStatus.NON_OK;
    }

    return {
      token: response.data.access_token,
      expire: new Date((new Date()).getTime() + (response.data.expires_in * 1000))
    };
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response.status === HTTPStatus.UNAUTHORIZED) {
        log(LogSeverity.ERROR, "getAccessToken", "Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
        process.exit(1);
      }

      log(LogSeverity.ERROR, "getAccessToken", "osu! API returned status code " + e.response.status.toString() + ".");
      return OsuApiStatus.NON_OK;
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAccessToken", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getAccessToken", "Unknown error occurred.");
    }

    return OsuApiStatus.CLIENT_ERROR;
  }
}

/**
 * Revokes access token specified in parameter.
 *
 * @param { string } token - osu! access token.
 *
 * @returns { Promise<number> } Promise object with `OsuApiStatus` constant.
 */
async function revokeAccessToken(token) {
  try {
    const response = await axios.delete(OSU_API_ENDPOINT + "/oauth/tokens/current", {
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Authorization": "Bearer " + token
      }
    });

    if(response.status !== HTTPStatus.NO_CONTENT) {
      log(LogSeverity.ERROR, "revokeAccessToken", "osu! API returned status code " + response.status.toString() + ".");
      return OsuApiStatus.NON_OK;
    }

    return OsuApiStatus.OK;
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      log(LogSeverity.ERROR, "revokeAccessToken", "osu! API returned status code " + e.response.status.toString() + ".");
      return OsuApiStatus.NON_OK;
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "revokeAccessToken", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "revokeAccessToken", "Unknown error occurred.");
    }

    return OsuApiStatus.CLIENT_ERROR;
  }
}

/**
 * Gets user information for this bot by osu! ID.
 *
 * @param { string } token - osu! access token.
 * @param { number } id - osu! user ID.
 *
 * @returns { Promise<{ status: number; username?: string; isCountryCodeAllowed?: boolean; } | number> } Promise object with user information. Returns `OsuApiStatus` constant in case of errors.
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

    if(response.status !== HTTPStatus.OK) {
      log(LogSeverity.ERROR, "getUserByOsuId", "osu! API returned status code " + response.status.toString() + ".");
      return OsuApiStatus.NON_OK;
    }

    let res = {
      status: OsuUserStatus.NOT_FOUND
    };

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

    return res;
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      let exit = false;

      switch(e.response.status) {
        case HTTPStatus.UNAUTHORIZED:
          log(LogSeverity.ERROR, "getAccessToken", "Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
          exit = true;
          break;
        case HTTPStatus.NOT_FOUND:
          return {
            status: OsuUserStatus.NOT_FOUND
          };
      }

      if(exit) {
        process.exit(1);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getUserByOsuId", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getUserByOsuId", "Unknown error occurred.");
    }

    return OsuApiStatus.CLIENT_ERROR;
  }
}

module.exports = {
  getAccessToken,
  revokeAccessToken,
  getUserByOsuId
};
