import ParserError from "./parser-error";

/**
 * Invalid expression error class.
 */
class InvalidExpressionError extends ParserError {
  constructor() {
    super("Invalid expression.");
  }
}

export default InvalidExpressionError;
