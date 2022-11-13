import DatabaseErrors from "./database-error";

/**
 * Database no record returned error class.
 */
class NoRecordError extends DatabaseErrors {
  constructor() {
    super("Specified query returns no records.");
  }
}

export default NoRecordError;
