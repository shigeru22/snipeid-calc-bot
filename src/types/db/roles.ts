/**
 * Database server role table query interface.
 */
interface IDBServerRoleQueryData {
  roleid: number;
  discordid: string;
  rolename: string;
  minpoints: number;
}

/**
 * Database server role table data interface.
 */
interface IDBServerRoleData {
  roleId: number;
  discordId: string;
  roleName: string;
  minPoints: number;
}

export { IDBServerRoleQueryData, IDBServerRoleData };
