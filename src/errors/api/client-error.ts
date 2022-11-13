import APIErrors from "./api-error";

/**
 * General API client error class.
 */
class APIClientError extends APIErrors {
  constructor(message?: string) {
    super(`API client error occurred${ message !== undefined ? `: ${ message }` : "." }`);
  }
}

export default APIClientError;
