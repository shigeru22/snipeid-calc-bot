class InvalidRequiredElement extends Error {
  constructor(elements: number) {
    super(`Invalid number of elements. Required: ${ elements }`);
  }
}

export { InvalidRequiredElement };
