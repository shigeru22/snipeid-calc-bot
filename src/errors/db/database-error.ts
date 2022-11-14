/**
 * Database errors base class.
 */
class DatabaseErrors extends Error {
  constructor(message: string) {
    super(message);
  }
}

export default DatabaseErrors;