import { InvalidRequiredElement } from "../errors/utils/invalid-element";
import { InvalidExpressionError, InvalidTypeError, InvalidNumberOfRanksError, InvalidTopRankError } from "../errors/utils/parser";

class Parser {
  /**
   * Parses Bathbot's top count embed into arrays.
   *
   * @param { string } desc Embed description.
   *
   * @returns { [ number, number, number, number, number ] } Array of top counts.
   *
   * @throws { InvalidRequiredElement } Invalid number of elements parsed.
   */
  static parseTopCountDescription(desc: string): [ number, number, number, number, number ] {
    const tops = desc.replace(/```(\n)*/g, "").split("\n");

    const len = tops.length;
    for(let i = 0; i < len; i++) {
      tops[i] = tops[i].replace(/Top /g, "").replace(/ /g, "").replace(/,/g, "");
    }

    const topsArray: number[] = [];
    tops.forEach(item => {
      if(item !== "") {
        const temp = item.split(":");
        topsArray.push(parseInt(temp[1], 10));
      }
    });

    if(topsArray.length !== 5) {
      throw new InvalidRequiredElement(5);
    }

    return topsArray as [ number, number, number, number, number ];
  }

  /**
   * Parses username from Bathbot's top count embed.
   *
   * @param { string } title Embed title.
   *
   * @returns { string } Parsed username.
   */
  static parseUsername(title: string): string {
    return title.replace("In how many top X map leaderboards is ", "").replace("?", "");
  }

  /**
   * Parses osu! ID from Bathbot's top count URL.
   *
   * @param { string } url Embed URL.
   *
   * @returns { string } Parsed osu! ID.
   */
  static parseOsuIdFromLink(url: string): string {
    return url.replace(/http(s)?:\/\/osu.ppy.sh\/u(sers)?\//g, "").split("/")[0];
  }

  /**
   * Parses `whatif` command query.
   *
   * @param { string } exp Query expression.
   *
   * @returns { number[] } Array of what-if top counts. Throws below errors if failed.
   *
   * @throws { InvalidExpressionError } Invalid expression found in `exp`.
   * @throws { InvalidTypeError } Invalid type either in left or right side of `=` in `exp`.
   * @throws { InvalidTopRankError } Invalid top rank value (must be higher than 0).
   * @throws { InvalidNumberOfRanksError } Invalid number of top ranks value (must be higher than or equal to 0).
   */
  static parseWhatIfCount(exp: string): number[] {
    const temp = exp.split("=");

    if(temp.length !== 2) {
      throw new InvalidExpressionError();
    }

    const testerArray = [ parseInt(temp[0], 10), parseInt(temp[1], 10) ];

    if(isNaN(testerArray[0])) {
      throw new InvalidTypeError(0);
    }

    if(isNaN(testerArray[1])) {
      throw new InvalidTypeError(1);
    }

    if(testerArray[0] < 1) {
      throw new InvalidTopRankError();
    }

    if(testerArray[1] < 0) {
      throw new InvalidNumberOfRanksError();
    }

    return testerArray;
  }
}

export { Parser };
