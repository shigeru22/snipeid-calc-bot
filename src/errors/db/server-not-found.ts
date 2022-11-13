import DatabaseErrors from "./database-error";

/**
 * Database no server data error class.
 */
class ServerNotFoundError extends DatabaseErrors {
  constructor() {
    super("Server not found in database.");
  }
}

export default ServerNotFoundError;
