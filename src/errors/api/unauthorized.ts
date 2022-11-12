import APIErrors from "./api-error";

class UnauthorizedError extends APIErrors {
  constructor() {
    super("Unauthorized API access detected (403).");
  }
}

export default UnauthorizedError;
