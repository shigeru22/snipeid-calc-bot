/**
 * General parser error class.
 */
class ParserError extends Error {
  constructor(message: string) {
    super(message);
  }
}

export default ParserError;
