/**
 * API errors base class.
 */
class APIErrors extends Error {
  constructor(message: string) {
    super(message);
  }
}

export default APIErrors;
