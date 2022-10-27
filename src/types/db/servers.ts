/**
 * Database server user table query interface.
 */
interface IDBServerQueryData {
  serverid: number;
  discordid: string;
  country: string | null;
  verifychannelid: string | null;
  verifiedroleid: string | null;
  commandschannelid: string | null;
  leaderboardschannelid: string | null;
}

/**
 * Database server user table data interface.
 */
interface IDBServerData {
  serverId: number;
  discordId: string;
  country: string | null;
  verifyChannelId: string | null;
  verifiedRoleId: string | null;
  commandsChannelId: string | null;
  leaderboardsChannelId: string | null;
}

export { IDBServerQueryData, IDBServerData };
