import APIErrors from "./api-error";

/**
 * API unauthorized (403) error class.
 */
class UnauthorizedError extends APIErrors {
  constructor() {
    super("Unauthorized API access detected (403).");
  }
}

export default UnauthorizedError;
