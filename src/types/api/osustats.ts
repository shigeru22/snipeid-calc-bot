import { OsuStatsErrorStatus, OsuStatsSuccessStatus } from "../../utils/common";

/**
 * osu!Stats API data interface.
 */
interface IOsuStatsUserData {
  userName: string;
  maxRank: number;
  count: number;
}

/**
 * osu!Stats API response data type.
 */
type OsuStatsApiResponseData = [ unknown, number, boolean, boolean ]; // first element is unused

/**
 * osu!Stats (respektive) API response data type.
 */
type OsuStatsRespektiveApiResponseData = {
  username: string | null,
  top1s: number | null,
  top8s: number | null,
  top25s: number | null,
  top50s: number | null
}

/**
 * osu!Stats API success response data generic interface.
 */
interface IOsuStatsSuccessResponseData<T> {
  status: OsuStatsSuccessStatus.OK;
  data: T;
}

/**
 * osu!Stats API error response data generic interface.
 */
interface IOsuStatsErrorResponseData<T extends OsuStatsErrorStatus> {
  status: T;
}

/**
 * Main osu!Stats API response data type.
 */
type OsuStatsResponseData<T> = T extends OsuStatsErrorStatus ? IOsuStatsErrorResponseData<T> : IOsuStatsSuccessResponseData<T>;

/**
 * Checks whether response's type is `IOsuStatsErrorResponseData`.
 *
 * @param { unknown } response - Response to be checked.
 *
 * @returns { response is IOsuStatsErrorResponseData<OsuStatsErrorStatus> } Returns `true` if response is an error, `false` otherwise.
 */
function isOsuStatsErrorResponse(response: unknown): response is IOsuStatsErrorResponseData<OsuStatsErrorStatus> {
  return (response as IOsuStatsErrorResponseData<OsuStatsErrorStatus>).status !== OsuStatsErrorStatus.OK;
}

export { IOsuStatsUserData, OsuStatsApiResponseData, OsuStatsRespektiveApiResponseData, OsuStatsResponseData, isOsuStatsErrorResponse };
