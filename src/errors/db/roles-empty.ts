import DatabaseErrors from "./database-error";

class RolesEmptyError extends DatabaseErrors {
  constructor() {
    super("Roles data for the specified server is empty.");
  }
}

export default RolesEmptyError;
