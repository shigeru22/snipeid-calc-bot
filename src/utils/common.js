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

const OsuUserStatus = {
  USER: 1,
  BOT: 2,
  DELETED: 3,
  NOT_FOUND: 4
};

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

const AssignmentType = {
  INSERT: 0,
  UPDATE: 1
};

const AssignmentSort = {
  ID: 1,
  ROLE_ID: 2,
  POINTS: 3,
  LAST_UPDATED: 4
};

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
	DatabaseErrors,
	AssignmentType,
	AssignmentSort,
	isTypeEnumAvailable,
	isSortEnumAvailable
};
