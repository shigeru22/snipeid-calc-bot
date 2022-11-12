import DatabaseErrors from "./database-error";

class DatabaseConnectionError extends DatabaseErrors {
  constructor() {
    super("Database connection error occurred.");
  }
}

export default DatabaseConnectionError;
