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
 * Assignment type enum.
 */
enum AssignmentType {
  INSERT,
  UPDATE
}

export { HTTPStatus, OsuUserStatus, AssignmentType };
