import ParserError from "./parser-error";

class InvalidTopRankError extends ParserError {
  constructor() {
    super("Top rank must be higher than 0.");
  }
}

export default InvalidTopRankError;
