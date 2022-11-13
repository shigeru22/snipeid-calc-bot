import APIErrors from "./api-error";

/**
 * API not found (404) error class.
 */
class NotFoundError extends APIErrors {
  constructor() {
    super("404 not found.");
  }
}

export default NotFoundError;
