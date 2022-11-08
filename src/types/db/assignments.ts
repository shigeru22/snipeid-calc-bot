import { AssignmentType } from "../../utils/common";

/* database query types */

/**
 * Database assignment table data interface.
 */
interface IDBAssignmentData {
  assignmentid: number;
  username: string;
  rolename: string;
}

/**
 * Database server assignment table query interface.
 */
interface IDBServerAssignmentQueryData {
  assignmentid: number;
  username: string;
  rolename: string;
}

/**
 * Database server assignment table data interface.
 */
interface IDBServerAssignmentData {
  assignmentId: number;
  userName: string;
  roleName: string;
}

/**
 * Assignment result data interface.
 */
interface IDBAssignmentResultData {
  type: AssignmentType;
  discordId: string;
  role: {
    oldRoleId?: string;
    oldRoleName?: string;
    newRoleId: string;
    newRoleName: string;
  };
  delta: number;
  lastUpdate: Date | null;
}

export { IDBAssignmentData, IDBServerAssignmentQueryData, IDBServerAssignmentData, IDBAssignmentResultData };
