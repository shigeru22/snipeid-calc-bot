import ParserError from "./parser-error";

/**
 * Invalid number of ranks value error class.
 */
class InvalidNumberOfRanksError extends ParserError {
  constructor() {
    super("Number of ranks must be higher than or equal to 0.");
  }
}

export default InvalidNumberOfRanksError;
