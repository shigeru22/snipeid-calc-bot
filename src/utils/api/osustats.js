const axios = require("axios").default;
const { LogSeverity, log } = require("../log");
const { HTTPStatus, OsuStatsStatus } = require("../common");

const OSUSTATS_API_ENDPOINT = "https://osustats.ppy.sh/api";

async function getTopCounts(userName, maxRank) {
  if(typeof(userName) !== "string") {
    log(LogSeverity.ERROR, "getTopCounts", "userName must be string.");
    return OsuStatsStatus.TYPE_ERROR;
  }

  if(typeof(maxRank) !== "number") {
    log(LogSeverity.ERROR, "getTopCounts", "maxRank must be number.");
    return OsuStatsStatus.TYPE_ERROR;
  }

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
  catch {
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

module.exports = {
  getTopCounts
};
