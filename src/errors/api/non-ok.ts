import APIErrors from "./api-error";

class NonOKError extends APIErrors {
  constructor(status?: string | number) {
    super(`API returned ${ status !== undefined ? `status code ${ status }` : "non-OK status code" }.`);
  }
}

export default NonOKError;
