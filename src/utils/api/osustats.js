const axios = require("axios").default;
const { LogSeverity, log } = require("../log");
const { HTTPStatus, OsuStatsStatus } = require("../common");

const OSUSTATS_API_ENDPOINT = "https://osustats.ppy.sh/api";
const OSUSTATS_API_RESPEKTIVE_ENDPOINT = "https://osustats.respektive.pw";

/**
 * Retrieves user top leaderboard count.
 *
 * @param { string } userName - osu! username.
 * @param { number } maxRank - Maximum rank to retrieve.
 *
 * @returns { Promise<{ userName: string, maxRank: number, count: number } | number> } Promise object with user name, max rank, and count. Returns `OsuStatsStatus` constant in case of errors.
 */
async function getTopCounts(userName, maxRank) {
  try {
    const response = await axios.post(OSUSTATS_API_ENDPOINT + "/getScores", {
      accMin: 0.0,
      accMax: 100.0,
      rankMin: 1,
      rankMax: maxRank,
      sortBy: 2, // rank
      sortOrder: 0, // ascending
      page: 1,
      u1: userName
    });

    if(response.status !== HTTPStatus.OK) {
      log(LogSeverity.ERROR, "getTopCounts", "osu!Stats returned status code " + response.status + ":\n" + response.data);
      return OsuStatsStatus.CLIENT_ERROR;
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
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response.status === HTTPStatus.BAD_REQUEST) {
        return OsuStatsStatus.USER_NOT_FOUND;
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getTopCounts", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getTopCounts", "Unknown error occurred.");
    }

    return OsuStatsStatus.CLIENT_ERROR;
  }
}

/**
 * Retrieves user top leaderboard count from respektive's API.
 *
 * @param { number } osuId - osu! user ID.
 *
 * @returns { Promise<number[] | number> } Promise object with user top leaderboard count array. Assume `[ top 1, top 8, top 25, top 50 ]` for now. Returns `OsuStatsStatus` constant in case of errors.
 */
async function getTopCountsFromRespektive(osuId) {
  try {
    const response = await axios.get(OSUSTATS_API_RESPEKTIVE_ENDPOINT + "/counts/" + osuId);

    if(response.status !== HTTPStatus.OK) {
      log(LogSeverity.ERROR, "getTopCounts", "osu!Stats returned status code " + response.status + ":\n" + response.data);
      return OsuStatsStatus.CLIENT_ERROR;
    }

    if(response.data.username === null) {
      return OsuStatsStatus.USER_NOT_FOUND;
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

    const ret = [];
    ret.push(response.data.top1s !== null ? response.data.top1s : 0);
    ret.push(response.data.top8s !== null ? response.data.top8s : 0);
    ret.push(response.data.top25s !== null ? response.data.top25s : 0);
    ret.push(response.data.top50s !== null ? response.data.top50s : 0);

    return ret;
  }
  catch (e) {
    if(axios.isAxiosError(e)) {
      if(e.response.status === HTTPStatus.NOT_FOUND) {
        return OsuStatsStatus.USER_NOT_FOUND;
      }
      else {
        log(LogSeverity.ERROR, "getTopCounts", e.name + ": " + e.message);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getTopCounts", e.name + ": " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getTopCounts", "Unknown error occurred.");
    }

    return OsuStatsStatus.CLIENT_ERROR;
  }

}

module.exports = {
  getTopCounts,
  getTopCountsFromRespektive
};
