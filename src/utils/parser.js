/**
 * Parses Bathbot's top count embed into arrays.
 *
 * @param { string } desc
 *
 * @returns { number[] }
 */
function parseTopCountDescription(desc) {
  const tops = desc.replace(/```(\n)*/g, "").split("\n");

  const len = tops.length;
  for(let i = 0; i < len; i++) {
    tops[i] = tops[i].replace(/Top /g, "").replace(/ /g, "").replace(/,/g, "");
  }

  const topsArray = [];
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
 * @param { string } title
 *
 * @returns { string }
 */
function parseUsername(title) {
  return title.replace("In how many top X map leaderboards is ", "").replace("?", "");
}

/**
 * Parses osu! ID from Bathbot's top count URL.
 *
 * @param { string } url
 *
 * @returns { string }
 */
function parseOsuIdFromLink(url) {
  return url.replace(/http(s)?:\/\/osu.ppy.sh\/u(sers)?\//g, "").split("/")[0];
}

/**
 * Parses `whatif` command query.
 *
 * @param { string } exp
 *
 * @returns { number[] }
 */
function parseWhatIfCount(exp) {
  const temp = exp.split("=");

  // TODO: create enum for each return code

  if(temp.length !== 2) {
    return -1; // invalid expression
  }

  temp[0] = parseInt(temp[0], 10);
  temp[1] = parseInt(temp[1], 10);

  if(typeof(temp[0]) !== "number" || typeof(temp[1]) !== "number") {
    return -2; // invalid type
  }

  if(temp[0] < 1) {
    return -3; // top rank should be higher than 0
  }

  if(temp[1] < 0) {
    return -4; // number of ranks should be higher or equal to 0
  }

  return temp;
}

module.exports = {
  parseTopCountDescription,
  parseUsername,
  parseOsuIdFromLink,
  parseWhatIfCount
};
