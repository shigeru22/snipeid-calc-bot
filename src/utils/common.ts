/**
 * HTTP status codes enum.
 */
const HTTPStatus = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  METHOD_NOT_ALLOWED: 405,
  CONFLICT: 409,
  INTERNAL_SERVER_ERROR: 500
};

/**
 * osu! user status enum.
 */
enum OsuUserStatus {
  USER,
  BOT,
  DELETED,
  NOT_FOUND
}

/**
 * osu! API function success response status.
 */
enum OsuApiSuccessStatus {
  OK
}

/**
 * osu! API function error response status.
 */
enum OsuApiErrorStatus {
  OK, // should not be used
  NON_OK,
  UNAUTHORIZED,
  CLIENT_ERROR
}

/**
 * osu!Stats success response status.
 */
enum OsuStatsSuccessStatus {
  OK
}

/**
 * osu!Stats response status.
 */
enum OsuStatsErrorStatus {
  OK, // should not be used
  USER_NOT_FOUND,
  API_ERROR,
  CLIENT_ERROR
}

/**
 * Database errors enum.
 */
enum DatabaseErrors {
  OK, // should not be used
  CONNECTION_ERROR,
  DUPLICATED_DISCORD_ID,
  DUPLICATED_OSU_ID,
  DUPLICATED_RECORD,
  USER_NOT_FOUND,
  NO_RECORD,
  ROLES_EMPTY,
  CLIENT_ERROR
}

/**
 * Database non-error status enum.
 */
enum DatabaseSuccess { // this is created to differentiate type checking
  OK
}

/**
 * Assignment type enum.
 */
enum AssignmentType {
  INSERT,
  UPDATE
}

/**
 * Assignment sorting enum.
 */
enum AssignmentSort {
  ID,
  ROLE_ID,
  POINTS,
  LAST_UPDATED
}

/**
 * Returns SQL string representation of assignment sorting enum.
 *
 * @param { AssignmentSort } sort Sorting enum value.
 *
 * @returns { string } SQL string representation of assignment sorting value.
 */
function assignmentSortToString(sort: AssignmentSort): string {
  switch(sort) {
    case AssignmentSort.ID:
      return "assignments.\"assignmentid\"";
    case AssignmentSort.ROLE_ID:
      return "assignments.\"roleid\"";
    case AssignmentSort.POINTS:
      return "assignments.\"points\"";
    case AssignmentSort.LAST_UPDATED:
      return "assignments.\"lastupdate\"";
    default:
      return "";
  }
}

const Commons = {
  HTTPStatus,
  OsuUserStatus,
  OsuApiSuccessStatus,
  OsuApiErrorStatus,
  OsuStatsSuccessStatus,
  OsuStatsErrorStatus,
  DatabaseErrors,
  DatabaseSuccess,
  AssignmentType,
  AssignmentSort,
  assignmentSortToString
};

export { HTTPStatus, OsuUserStatus, OsuApiSuccessStatus, OsuApiErrorStatus, OsuStatsSuccessStatus, OsuStatsErrorStatus, DatabaseErrors, DatabaseSuccess, AssignmentType, AssignmentSort, assignmentSortToString };
export default Commons;
