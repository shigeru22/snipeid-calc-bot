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
const OsuUserStatus = {
  USER: 1,
  BOT: 2,
  DELETED: 3,
  NOT_FOUND: 4
};

/**
 * osu! API response status.
 */
const OsuStatsStatus = {
  TYPE_ERROR: 1,
  USER_NOT_FOUND: 2,
  CLIENT_ERROR: 3
};

/**
 * Database errors enum.
 */
const DatabaseErrors = {
  OK: 0,
  CONNECTION_ERROR: 1,
  TYPE_ERROR: 2,
  DUPLICATED_DISCORD_ID: 3,
  DUPLICATED_OSU_ID: 4,
  USER_NOT_FOUND: 5,
  NO_RECORD: 6,
  ROLES_EMPTY: 7,
  CLIENT_ERROR: 8
};

/**
 * Assignment type enum.
 */
const AssignmentType = {
  INSERT: 0,
  UPDATE: 1
};

/**
 * Assignment sorting enum.
 */
const AssignmentSort = {
  ID: 1,
  ROLE_ID: 2,
  POINTS: 3,
  LAST_UPDATED: 4
};

/**
 * Checks whether assignment type is available in enum.
 *
 * @param { number | undefined } value
 *
 * @returns { boolean | undefined }
 */
function isTypeEnumAvailable(value) {
	if(typeof(value) === "undefined") {
    return undefined;
  }

  for(const prop in AssignmentType) {
    if(AssignmentType[prop] === value) {
      return true;
    }
  }

  return false;
}

/**
 * Checks whether sorting type is available in enum.
 *
 * @param { number | undefined } value
 *
 * @returns { boolean | undefined }
 */
function isSortEnumAvailable(value) {
	if(typeof(value) === "undefined") {
    return undefined;
  }

  for(const prop in AssignmentSort) {
    if(AssignmentSort[prop] === value) {
      return true;
    }
  }

  return false;
}

module.exports = {
  HTTPStatus,
  OsuUserStatus,
  OsuStatsStatus,
	DatabaseErrors,
	AssignmentType,
	AssignmentSort,
	isTypeEnumAvailable,
	isSortEnumAvailable
};
