import DatabaseErrors from "./database-error";

/**
 * Database connection error class.
 */
class DatabaseConnectionError extends DatabaseErrors {
  constructor() {
    super("Database connection error occurred.");
  }
}

export default DatabaseConnectionError;
