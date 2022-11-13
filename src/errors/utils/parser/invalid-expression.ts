import ParserError from "./parser-error";

class InvalidExpressionError extends ParserError {
  constructor() {
    super("Invalid expression.");
  }
}

export default InvalidExpressionError;
