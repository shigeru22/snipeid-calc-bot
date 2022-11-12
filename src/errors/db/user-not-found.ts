import DatabaseErrors from "./database-error";

class UserNotFoundError extends DatabaseErrors {
  constructor() {
    super("User not found in database.");
  }
}

export default UserNotFoundError;
