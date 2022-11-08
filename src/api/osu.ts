import axios from "axios";
import { LogSeverity, log } from "../utils/log";
import { HTTPStatus, OsuUserStatus, OsuApiErrorStatus, OsuApiSuccessStatus } from "../utils/common";
import { IOsuApiTokenData, IOsuApiUserData, IOsuApiTokenResponseData, IOsuApiUserResponseData, OsuApiResponseData } from "../types/api/osu";

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";
const OSU_TOKEN_ENDPOINT = "https://osu.ppy.sh/oauth/token";

/**
 * Gets access token using osu! client ID and secret.
 *
 * @param { string } clientId osu! client ID.
 * @param { string } clientSecret osu! client secret.
 *
 * @returns { Promise<OsuApiResponseData<IOsuApiTokenData> | OsuApiResponseData<OsuApiErrorStatus.NON_OK | OsuApiErrorStatus.CLIENT_ERROR>> } Promise object with access token and expiration date.
 */
async function getAccessToken(clientId: string, clientSecret: string): Promise<OsuApiResponseData<IOsuApiTokenData> | OsuApiResponseData<OsuApiErrorStatus.NON_OK | OsuApiErrorStatus.CLIENT_ERROR>> {
  const id = parseInt(clientId, 10); // no need to validate since already validated in env module
  const secret = clientSecret;

  try {
    const response = await axios.post<IOsuApiTokenResponseData>(OSU_TOKEN_ENDPOINT, {
      client_id: id,
      client_secret: secret,
      grant_type: "client_credentials",
      scope: "public"
    });

    if(response.status !== HTTPStatus.OK) {
      log(LogSeverity.ERROR, "getAccessToken", `osu! API returned status code ${ response.status.toString() }.`);
      return {
        status: OsuApiErrorStatus.NON_OK
      };
    }

    return {
      status: OsuApiSuccessStatus.OK,
      data: {
        token: response.data.access_token,
        expire: new Date((new Date()).getTime() + (response.data.expires_in * 1000))
      }
    };
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        if(e.response.status === HTTPStatus.UNAUTHORIZED) {
          log(LogSeverity.ERROR, "getAccessToken", "Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
          process.exit(1);
        }

        log(LogSeverity.ERROR, "getAccessToken", `osu! API returned status code ${ e.response.status.toString() }.`);
        return {
          status: OsuApiErrorStatus.NON_OK
        };
      }
      else {
        log(LogSeverity.ERROR, "getAccessToken", `${ e.name }: ${ e.message }`);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getAccessToken", `${ e.name }: ${ e.message }`);
    }
    else {
      log(LogSeverity.ERROR, "getAccessToken", "Unknown error occurred.");
    }

    return {
      status: OsuApiErrorStatus.CLIENT_ERROR
    };
  }
}

/**
 * Revokes access token specified in parameter.
 *
 * @param { string } token osu! access token.
 *
 * @returns { Promise<OsuApiResponseData<true> | OsuApiResponseData<OsuApiErrorStatus.NON_OK | OsuApiErrorStatus.CLIENT_ERROR>> } Promise object with `OsuApiStatus` constant.
 */
async function revokeAccessToken(token: string): Promise<OsuApiResponseData<true> | OsuApiResponseData<OsuApiErrorStatus.NON_OK | OsuApiErrorStatus.CLIENT_ERROR>> {
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
        status: OsuApiErrorStatus.NON_OK
      };
    }

    return {
      status: OsuApiSuccessStatus.OK,
      data: true
    };
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        log(LogSeverity.ERROR, "getAccessToken", "osu! API returned status code " + e.response.status.toString() + ".");
        return {
          status: OsuApiErrorStatus.NON_OK
        };
      }
      else {
        log(LogSeverity.ERROR, "revokeAccessToken", `${ e.name }: ${ e.message }`);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "revokeAccessToken", `${ e.name }: ${ e.message }`);
    }
    else {
      log(LogSeverity.ERROR, "revokeAccessToken", "Unknown error occurred.");
    }

    return {
      status: OsuApiErrorStatus.CLIENT_ERROR
    };
  }
}

/**
 * Gets user information for this bot by osu! ID.
 *
 * @param { string } token osu! access token.
 * @param { number } id osu! user ID.
 *
 * @returns { Promise<OsuApiResponseData<IOsuApiUserData> | OsuApiResponseData<OsuApiErrorStatus.NON_OK | OsuApiErrorStatus.CLIENT_ERROR>> } Promise object with user information.
 */
async function getUserByOsuId(token: string, id: number): Promise<OsuApiResponseData<IOsuApiUserData> | OsuApiResponseData<OsuApiErrorStatus.NON_OK | OsuApiErrorStatus.CLIENT_ERROR>> {
  try {
    const response = await axios.get<IOsuApiUserResponseData>(OSU_API_ENDPOINT + "/users/" + id.toString(), {
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
      log(LogSeverity.ERROR, "getUserByOsuId", `osu! API returned status code ${ response.status.toString() }.`);
      return {
        status: OsuApiErrorStatus.CLIENT_ERROR
      };
    }

    if(response.data.is_bot) {
      return {
        status: OsuApiSuccessStatus.OK,
        data: {
          status: OsuUserStatus.BOT
        }
      };
    }
    else if(response.data.is_deleted) {
      return {
        status: OsuApiSuccessStatus.OK,
        data: {
          status: OsuUserStatus.DELETED
        }
      };
    }

    return {
      status: OsuApiSuccessStatus.OK,
      data: {
        status: OsuUserStatus.USER,
        user: {
          userName: response.data.username,
          country: response.data.country_code
        }
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
              status: OsuApiErrorStatus.NON_OK
            };
        }

        if(exit) {
          process.exit(1);
        }
      }
      else {
        log(LogSeverity.ERROR, "getUserByOsuId", `${ e.name }: ${ e.message }`);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getUserByOsuId", `${ e.name }: ${ e.message }`);
    }
    else {
      log(LogSeverity.ERROR, "getUserByOsuId", "Unknown error occurred.");
    }

    return {
      status: OsuApiErrorStatus.CLIENT_ERROR
    };
  }
}

export { getAccessToken, revokeAccessToken, getUserByOsuId };
