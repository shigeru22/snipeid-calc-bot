import DatabaseErrors from "./database-error";

class ServerNotFoundError extends DatabaseErrors {
  constructor() {
    super("Server not found in database.");
  }
}

export default ServerNotFoundError;
