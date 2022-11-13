import DatabaseErrors from "./database-error";

/**
 * Database no user found error class.
 */
class UserNotFoundError extends DatabaseErrors {
  constructor() {
    super("User not found in database.");
  }
}

export default UserNotFoundError;
