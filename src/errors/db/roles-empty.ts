import DatabaseErrors from "./database-error";

/**
 * Database no roles data error class.
 */
class RolesEmptyError extends DatabaseErrors {
  constructor() {
    super("Roles data for the specified server is empty.");
  }
}

export default RolesEmptyError;
