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

export { IOsuStatsUserData, OsuStatsApiResponseData, OsuStatsRespektiveApiResponseData };
