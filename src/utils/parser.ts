enum WhatIfParserStatus {
  OK,
  INVALID_EXPRESSION,
  TYPE_ERROR,
  TOP_RANK_ERROR,
  NUMBER_OF_RANKS_ERROR
}

class Parser {
  /**
   * Parses Bathbot's top count embed into arrays.
   *
   * @param { string } desc Embed description.
   *
   * @returns { number[] } Array of top counts.
   */
  static parseTopCountDescription(desc: string): number[] {
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

    return topsArray;
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
   * @returns { number[] | number } Array of what-if top counts. Returns `WhatIfParserStatus` constant in case of errors.
   */
  static parseWhatIfCount(exp: string): number[] | number {
    const temp = exp.split("=");

    if(temp.length !== 2) {
      return WhatIfParserStatus.INVALID_EXPRESSION;
    }

    const testerArray = [ parseInt(temp[0], 10), parseInt(temp[1], 10) ];

    if(isNaN(testerArray[0]) || isNaN(testerArray[1])) {
      return WhatIfParserStatus.TYPE_ERROR;
    }

    if(testerArray[0] < 1) {
      return WhatIfParserStatus.TOP_RANK_ERROR;
    }

    if(testerArray[1] < 0) {
      return WhatIfParserStatus.NUMBER_OF_RANKS_ERROR;
    }

    return testerArray;
  }
}

export { WhatIfParserStatus, Parser };
