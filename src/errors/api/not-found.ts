import APIErrors from "./api-error";

class NotFoundError extends APIErrors {
  constructor() {
    super("404 not found.");
  }
}

export default NotFoundError;
