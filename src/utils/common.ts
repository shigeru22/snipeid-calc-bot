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
  NOT_FOUND,
  API_ERROR,
  CLIENT_ERROR
}

/**
 * osu! API function response status.
 */
enum OsuApiStatus {
  OK,
  NON_OK,
  UNAUTHORIZED,
  CLIENT_ERROR
}

/**
 * osu!Stats response status.
 */
enum OsuStatsStatus {
  OK,
  USER_NOT_FOUND,
  API_ERROR,
  CLIENT_ERROR
}

/**
 * Database errors enum.
 */
enum DatabaseErrors {
  OK,
  CONNECTION_ERROR,
  DUPLICATED_DISCORD_ID,
  DUPLICATED_OSU_ID,
  USER_NOT_FOUND,
  NO_RECORD,
  ROLES_EMPTY,
  CLIENT_ERROR
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

export { HTTPStatus, OsuUserStatus, OsuApiStatus, OsuStatsStatus, DatabaseErrors, AssignmentType, AssignmentSort };
