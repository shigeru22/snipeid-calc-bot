import DatabaseErrors from "./database-error";

class ConflictError extends DatabaseErrors {
  table: string | null = null;
  column: string | null = null;

  constructor(table?: string, column?: string) {
    super(`Duplicate data found in ${ table !== undefined ? `${ table } table${ column !== undefined ? ` at ${ column } column` : "" }` : "database" }.`);

    if(table !== undefined) {
      this.table = table;
      if(column !== undefined) {
        this.column = column;
      }
    }
  }
}

export default ConflictError;
