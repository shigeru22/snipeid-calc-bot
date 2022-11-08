/**
 * Database server user table query interface.
 */
interface IDBServerUserQueryData {
  userid: number;
  discordid: string;
  osuid: number;
  country: string;
  points: number;
}

/**
 * Database server leaderboard query interface.
 */
interface IDBServerLeaderboardQueryData {
  userid: number;
  username: string;
  points: number;
}

/**
 * Database server user table data interface.
 */
interface IDBServerUserData {
  userId: number;
  discordId: string;
  osuId: number;
  country: string;
  points: number;
}

/**
 * Database server leaderboard data interface.
 */
interface IDBServerLeaderboardData {
  userId: number;
  userName: string;
  points: number;
}

export { IDBServerUserQueryData, IDBServerLeaderboardQueryData, IDBServerUserData, IDBServerLeaderboardData };
