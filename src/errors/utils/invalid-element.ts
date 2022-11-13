/**
 * Invalid number of elements reuired error class.
 */
class InvalidRequiredElement extends Error {
  constructor(elements: number) {
    super(`Invalid number of elements. Required: ${ elements }`);
  }
}

export { InvalidRequiredElement };
