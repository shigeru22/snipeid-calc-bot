import axios from "axios";
import { Log } from "../utils/log";
import { HTTPStatus } from "../utils/common";
import { NonOKError, NotFoundError, APIClientError } from "../errors/api";
import { IOsuStatsUserData, OsuStatsApiResponseData, OsuStatsRespektiveApiResponseData } from "../types/api/osustats";

const OSUSTATS_API_ENDPOINT = "https://osustats.ppy.sh/api";
const OSUSTATS_API_RESPEKTIVE_ENDPOINT = "https://osustats.respektive.pw";

/**
 * Retrieves user top leaderboard count.
 *
 * @param { string } userName osu! username.
 * @param { number } maxRank Maximum rank to retrieve.
 *
 * @returns { Promise<IOsuStatsUserData> } Promise object with status, user name, max rank, and count. Throws errors below if failed.
 *
 * @throws { NotFoundError } osu! user with specified `userName` not found.
 * @throws { NonOKError } API returned non-OK (200) status code.
 * @throws { APIClientError } Unhandled client error occurred.
 */
async function getTopCounts(userName: string, maxRank: number): Promise<IOsuStatsUserData> {
  let response;

  try {
    response = await axios.post<OsuStatsApiResponseData>(`${ OSUSTATS_API_ENDPOINT }/getScores`, {
      accMin: 0.0,
      accMax: 100.0,
      rankMin: 1,
      rankMax: maxRank,
      sortBy: 2, // rank
      sortOrder: 0, // ascending
      page: 1,
      u1: userName
    });
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        if(e.response.status === HTTPStatus.BAD_REQUEST) { // yeah, osu!Stats (Piotrekol) returns 400 for not-found users
          throw new NotFoundError();
        }
        else {
          Log.error("getTopCounts", `osu!Stats API returned status code ${ e.response.status }.`);
          throw new NonOKError(e.response.status);
        }
      }
      else {
        Log.error("getTopCounts", `API request error occurred.\n${ e.stack }`);
      }
    }
    else if(e instanceof Error) {
      Log.error("getTopCounts", `Unhandled error occurred.\n${ e.stack }`);
    }
    else {
      Log.error("getTopCounts", "Unknown error occurred.");
    }

    throw new APIClientError();
  }

  if(response.status !== HTTPStatus.OK) {
    Log.error("getTopCounts", `osu!Stats API returned status code ${ response.status }:\n ${ response.data }`);
    throw new NonOKError(response.status);
  }

  /*
   * osu!Stats API response format (200) is the following:
   * [
   *   RankDetails[],
   *   number, // this is the rank count
   *   boolean,
   *   boolean
   * ]
   */

  return {
    userName,
    maxRank,
    count: response.data[1]
  };
}

/**
 * Retrieves user top leaderboard count from respektive's API.
 *
 * @param { number } osuId osu! user ID.
 *
 * @returns { Promise<[ number, number, number, number ]> } Promise object with status and user top leaderboard count array (assume `[ top 1, top 8, top 25, top 50 ]` for now). Throws errors below if failed.
 *
 * @throws { NotFoundError } osu! user with specified `userName` not found.
 * @throws { NonOKError } API returned non-OK (200) status code.
 * @throws { APIClientError } Unhandled client error occurred.
 */
async function getTopCountsFromRespektive(osuId: number): Promise<[ number, number, number, number ]> {
  let response;

  try {
    response = await axios.get<OsuStatsRespektiveApiResponseData>(`${ OSUSTATS_API_RESPEKTIVE_ENDPOINT }/counts/${ osuId }`);
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response !== undefined) {
        if(e.response.status === HTTPStatus.NOT_FOUND) {
          throw new NotFoundError();
        }
        else {
          Log.error("getTopCountsFromRespektive", `osu!Stats API returned status code ${ e.response.status }.`);
        }
      }
      else {
        Log.error("getTopCountsFromRespektive", `API request error occurred.\n${ e.stack }`);
      }
    }
    else if(e instanceof Error) {
      Log.error("getTopCountsFromRespektive", `Unhandled error occurred.\n${ e.stack }`);
    }
    else {
      Log.error("getTopCountsFromRespektive", "Unknown error occurred.");
    }

    throw new APIClientError();
  }

  if(response.status !== HTTPStatus.OK) {
    Log.error("getTopCountsFromRespektive", `osu!Stats API returned status code ${ response.status }:\n ${ response.data }`);
    throw new NonOKError(response.status);
  }

  if(response.data.username === null) {
    throw new NotFoundError();
  }

  /*
   * osu!Stats (respektive) API response format (200) is the following:
   * {
   *   "user_id": number,
   *   "username": string,
   *   "country": string,
   *   "top50s": number,
   *   "top25s": number,
   *   "top8s": number,
   *   "top1s": number
   * }
   *
   * Response format might change in the future.
   */

  return [
    response.data.top1s !== null ? response.data.top1s : 0,
    response.data.top8s !== null ? response.data.top8s : 0,
    response.data.top25s !== null ? response.data.top25s : 0,
    response.data.top50s !== null ? response.data.top50s : 0
  ];
}

export { getTopCounts, getTopCountsFromRespektive };
