import DatabaseErrors from "./database-error";

class NoConfigError extends DatabaseErrors {
  constructor() {
    super("No configuration set to this instance.");
  }
}

export default NoConfigError;
