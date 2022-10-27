/**
 * Database server user table query interface.
 */
interface IDBServerUserQueryData {
  userid: number;
  discordid: string;
  osuid: number;
}

/**
 * Database server user table data interface.
 */
interface IDBServerUserData {
  userId: number;
  discordId: string;
  osuId: number;
}

export { IDBServerUserQueryData, IDBServerUserData };
