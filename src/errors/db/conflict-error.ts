import DatabaseErrors from "./database-error";

class ConflictError extends DatabaseErrors {
  constructor(table?: string, column?: string) {
    super(`Duplicate data found in ${ table !== undefined ? `${ table } table${ column !== undefined ? ` at ${ column } column` : "" }` : "database" }.`);
  }
}

export default ConflictError;
