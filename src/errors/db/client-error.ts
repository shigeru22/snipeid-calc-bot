import DatabaseErrors from "./database-error";

/**
 * General database client error class.
 */
class DatabaseClientError extends DatabaseErrors {
  constructor(message?: string) {
    super(`Database client error occurred${ message !== undefined ? `: ${ message }` : "." }`);
  }
}

export default DatabaseClientError;
