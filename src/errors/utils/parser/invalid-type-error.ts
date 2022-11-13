import ParserError from "./parser-error";

/**
 * Invalid parsed data typing error class.
 */
class InvalidTypeError extends ParserError {
  position: 0 | 1 | null = null;

  constructor(position?: number) {
    super(`Invalid typing${ position !== undefined ? ` at ${ position === 0 ? "left" : "right" } expression` : "" }.`);

    if(position !== undefined) {
      switch(position) {
        case 0: // fallthrough
        case 1:
          this.position = position;
          break;
      }
    }
  }
}

export default InvalidTypeError;
