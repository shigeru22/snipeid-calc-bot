import DatabaseErrors from "./database-error";

class DuplicatedRecordError extends DatabaseErrors {
  constructor(table?: string, column?: string) {
    super(`Duplicated record found${ column !== undefined ? ` in ${ column } column` : "" }${ table !== undefined ? ` at ${ table } table` : "" }.`);
  }
}

export default DuplicatedRecordError;
