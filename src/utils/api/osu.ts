import axios from "axios";
import { LogSeverity, log } from "../log";
import { HTTPStatus, OsuUserStatus, OsuApiStatus } from "../common";

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";
const OSU_TOKEN_ENDPOINT = "https://osu.ppy.sh/oauth/token";

// TODO: convert compound object return types into interfaces

/**
 * Gets access token using osu! client ID and secret.
 *
 * @param { string } clientId - osu! client ID.
 * @param { string } clientSecret - osu! client secret.
 *
 * @returns { Promise<{ status: OsuApiStatus.OK | OsuApiStatus.NON_OK | OsuApiStatus.CLIENT_ERROR; token?: string; expire?: Date; }> } Promise object with access token and expiration date.
 */
async function getAccessToken(clientId: string, clientSecret: string): Promise<{ status: OsuApiStatus.OK | OsuApiStatus.NON_OK | OsuApiStatus.CLIENT_ERROR; token?: string; expire?: Date; }> {
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
      return {
        status: OsuApiStatus.NON_OK
      };
    }

    return {
      status: OsuApiStatus.OK,
      token: response.data.access_token,
      expire: new Date((new Date()).getTime() + (response.data.expires_in * 1000))
    };
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        if(e.response.status === HTTPStatus.UNAUTHORIZED) {
          log(LogSeverity.ERROR, "getAccessToken", "Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
          process.exit(1);
        }

        log(LogSeverity.ERROR, "getAccessToken", "osu! API returned status code " + e.response.status.toString() + ".");
        return {
          status: OsuApiStatus.NON_OK
        };
      }
      else {
        log(LogSeverity.ERROR, "getAccessToken", e.name + ": " + e.message);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAccessToken", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getAccessToken", "Unknown error occurred.");
    }

    return {
      status: OsuApiStatus.CLIENT_ERROR
    };
  }
}

/**
 * Revokes access token specified in parameter.
 *
 * @param { string } token - osu! access token.
 *
 * @returns { Promise<{ status: OsuApiStatus.OK | OsuApiStatus.NON_OK | OsuApiStatus.CLIENT_ERROR }> } Promise object with `OsuApiStatus` constant.
 */
async function revokeAccessToken(token: string): Promise<{ status: OsuApiStatus.OK | OsuApiStatus.NON_OK | OsuApiStatus.CLIENT_ERROR }> {
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
      return {
        status: OsuApiStatus.NON_OK
      };
    }

    return {
      status: OsuApiStatus.OK
    };
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        log(LogSeverity.ERROR, "getAccessToken", "osu! API returned status code " + e.response.status.toString() + ".");
        return {
          status: OsuApiStatus.NON_OK
        };
      }
      else {
        log(LogSeverity.ERROR, "revokeAccessToken", e.name + ": " + e.message);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "revokeAccessToken", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "revokeAccessToken", "Unknown error occurred.");
    }

    return {
      status: OsuApiStatus.CLIENT_ERROR
    };
  }
}

/**
 * Gets user information for this bot by osu! ID.
 *
 * @param { string } token - osu! access token.
 * @param { number } id - osu! user ID.
 *
 * @returns { Promise<{ status: OsuUserStatus; user?: { username: string; isCountryCodeAllowed: boolean; } }> } Promise object with user information.
 */
async function getUserByOsuId(token: string, id: number): Promise<{ status: OsuUserStatus.USER | OsuUserStatus.BOT | OsuUserStatus.DELETED | OsuUserStatus.NOT_FOUND | OsuUserStatus.API_ERROR | OsuUserStatus.CLIENT_ERROR; user?: { username: string; isCountryCodeAllowed: boolean; }; }> {
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
      return {
        status: OsuUserStatus.API_ERROR
      };
    }

    if(response.data.is_bot) {
      return {
        status: OsuUserStatus.BOT
      };
    }
    else if(response.data.is_deleted) {
      return {
        status: OsuUserStatus.DELETED
      };
    }

    return {
      status: OsuUserStatus.USER,
      user: {
        username: response.data.username,
        isCountryCodeAllowed: response.data.country.code === process.env.COUNTRY_CODE
      }
    };
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        let exit = false;

        switch(e.response.status) {
          case HTTPStatus.UNAUTHORIZED:
            log(LogSeverity.ERROR, "getUserByOsuId", "Failed to authenticate client. Check osu! client environment variables and token retrieval, and try again.");
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
      else {
        log(LogSeverity.ERROR, "getUserByOsuId", e.name + ": " + e.message);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getUserByOsuId", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getUserByOsuId", "Unknown error occurred.");
    }

    return {
      status: OsuUserStatus.CLIENT_ERROR
    };
  }
}

export { getAccessToken, revokeAccessToken, getUserByOsuId };
