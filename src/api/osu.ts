import axios from "axios";
import { Log } from "../utils/log";
import { HTTPStatus, OsuUserStatus } from "../utils/common";
import { NonOKError, NotFoundError, APIClientError } from "../errors/api";
import { IOsuApiTokenData, OsuApiUserData, IOsuApiTokenResponseData, IOsuApiUserResponseData } from "../types/api/osu";

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";
const OSU_TOKEN_ENDPOINT = "https://osu.ppy.sh/oauth/token";

/**
 * Gets access token using osu! client ID and secret.
 *
 * @param { number } clientId osu! client ID.
 * @param { string } clientSecret osu! client secret.
 *
 * @returns { Promise<IOsuApiTokenData> } Promise object with access token and expiration date. Throws errors below if failed.
 *
 * @throws { NonOKError } API returned non-OK (200) status code.
 * @throws { APIClientError } Unhandled client error occurred.
 */
async function getAccessToken(clientId: number, clientSecret: string): Promise<IOsuApiTokenData> {
  let response;

  try {
    response = await axios.post<IOsuApiTokenResponseData>(OSU_TOKEN_ENDPOINT, {
      client_id: clientId,
      client_secret: clientSecret,
      grant_type: "client_credentials",
      scope: "public"
    });
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        if(e.response.status === HTTPStatus.UNAUTHORIZED) {
          Log.error("getAccessToken", "Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
          process.emit("SIGINT");
        }

        Log.error("getAccessToken", `osu! API returned status code ${ e.response.status }.`);
        throw new NonOKError(e.response.status);
      }
      else {
        Log.error("getAccessToken", `API request error occurred.\n${ e.stack }`);
      }
    }
    else if(e instanceof Error) {
      Log.error("getAccessToken", `Unhandled error occurred.\n${ e.stack }`);
    }
    else {
      Log.error("getAccessToken", "Unknown error occurred.");
    }

    throw new APIClientError();
  }

  if(response.status !== HTTPStatus.OK) {
    Log.error("getAccessToken", `osu! API returned status code ${ response.status.toString() }.`);
    throw new NonOKError(response.status);
  }

  return {
    token: response.data.access_token,
    expire: new Date((new Date()).getTime() + (response.data.expires_in * 1000))
  };
}

/**
 * Revokes access token specified in parameter.
 *
 * @param { string } token osu! access token.
 *
 * @returns { Promise<void> } Promise object with no return value. Throws errors below if failed.
 *
 * @throws { NonOKError } API returned non-OK (200) status code.
 * @throws { APIClientError } Unhandled client error occurred.
 */
async function revokeAccessToken(token: string): Promise<void> {
  let response;

  try {
    response = await axios.delete(`${ OSU_API_ENDPOINT }/oauth/tokens/current`, {
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Authorization": `Bearer ${ token }`
      }
    });
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        Log.error("revokeAccessToken", `osu! API returned status code ${ e.response.status }.`);
        throw new NonOKError(e.response.status);
      }
      else {
        Log.error("revokeAccessToken", `API request error occurred.\n${ e.stack }`);
      }
    }
    else if(e instanceof Error) {
      Log.error("revokeAccessToken", `Unhandled error occurred.\n${ e.stack }`);
    }
    else {
      Log.error("revokeAccessToken", "Unknown error occurred.");
    }

    throw new APIClientError();
  }

  if(response.status !== HTTPStatus.NO_CONTENT) {
    Log.error("revokeAccessToken", `osu! API returned status code ${ response.status }.`);
    throw new NonOKError(response.status);
  }
}

/**
 * Gets user information for this bot by osu! ID.
 *
 * @param { string } token osu! access token.
 * @param { number } id osu! user ID.
 *
 * @returns { Promise<IOsuApiUserData> } Promise object with user information. Throws errors below if failed.
 *
 * @throws { NotFoundError } osu! user with specified `id` not found.
 * @throws { NonOKError } API returned non-OK (200) status code.
 * @throws { APIClientError } Unhandled client error occurred.
 */
async function getUserByOsuId(token: string, id: number): Promise<OsuApiUserData<OsuUserStatus>> {
  let response;

  try {
    response = await axios.get<IOsuApiUserResponseData>(`${ OSU_API_ENDPOINT }/users/${ id }`, {
      params: {
        key: "id"
      },
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Authorization": `Bearer ${ token }`
      }
    });
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        switch(e.response.status) {
          case HTTPStatus.UNAUTHORIZED:
            Log.error("getUserByOsuId", "Failed to authenticate client. Check osu! client environment variables and token retrieval, and try again.");
            process.emit("SIGINT"); // should exit after this line is executed
            break;
          case HTTPStatus.NOT_FOUND:
            throw new NotFoundError();
        }
      }
      else {
        Log.error("getUserByOsuId", `API request error occurred.\n${ e.stack }`);
      }
    }
    else if(e instanceof Error) {
      Log.error("getUserByOsuId", `Unhandled error occurred.\n${ e.stack }`);
    }
    else {
      Log.error("getUserByOsuId", "Unknown error occurred.");
    }

    throw new APIClientError();
  }

  if(response.status !== HTTPStatus.OK) {
    Log.error("getUserByOsuId", `osu! API returned status code ${ response.status }.`);
    throw new NonOKError(response.status);
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
      userName: response.data.username,
      country: response.data.country_code
    }
  };
}

export { getAccessToken, revokeAccessToken, getUserByOsuId };
