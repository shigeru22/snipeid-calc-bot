import DatabaseErrors from "./database-error";

class NoRecordError extends DatabaseErrors {
  constructor() {
    super("Specified query returns no records.");
  }
}

export default NoRecordError;
