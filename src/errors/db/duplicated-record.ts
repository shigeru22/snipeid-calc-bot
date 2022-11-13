import DatabaseErrors from "./database-error";

/**
 * Database duplicated data error class.
 */
class DuplicatedRecordError extends DatabaseErrors {
  constructor(table?: string, column?: string) {
    super(`Duplicated record found${ column !== undefined ? ` in ${ column } column` : "" }${ table !== undefined ? ` at ${ table } table` : "" }.`);
  }
}

export default DuplicatedRecordError;
