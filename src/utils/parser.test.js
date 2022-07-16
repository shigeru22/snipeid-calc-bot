const { WhatIfParserStatus, parseTopCountDescription, parseUsername, parseOsuIdFromLink, parseWhatIfCount } = require("./parser");

test("parseTopCountDescription", () => {
  const description = "```\nTop 1 :    52\nTop 8 : 1,892\nTop 15: 3,733\nTop 25: 5,404\nTop 50: 7,496\n```";
  expect(parseTopCountDescription(description)).toStrictEqual([ 52, 1892, 3733, 5404, 7496 ]);
});

test("parseUsername", () => {
  const title = "In how many top X map leaderboards is Akshiro?";
  expect(parseUsername(title)).toBe("Akshiro");
});

test("parseOsuIdFromLink (without mode routing)", () => {
  const link = "https://osu.ppy.sh/users/10557490";
  expect(parseOsuIdFromLink(link)).toBe("10557490");
});

test("parseOsuIdFromLink (with mode routing)", () => {
  const link = "https://osu.ppy.sh/users/10557490/osu";
  expect(parseOsuIdFromLink(link)).toBe("10557490");
});

test("parseWhatIfCount", () => {
  expect(parseWhatIfCount("1=5")).toStrictEqual([ 1, 5 ]);
});

test("parseWhatIfCount (invalid expression)", () => {
  expect(parseWhatIfCount("1=5=10")).toBe(WhatIfParserStatus.INVALID_EXPRESSION);
});

test("parseWhatIfCount (wrong type)", () => {
  expect(parseWhatIfCount("1=a")).toBe(WhatIfParserStatus.TYPE_ERROR);
  expect(parseWhatIfCount("a=5")).toBe(WhatIfParserStatus.TYPE_ERROR);
});

test("parseWhatIfCount (wrong top rank value)", () => {
  expect(parseWhatIfCount("0=5")).toBe(WhatIfParserStatus.TOP_RANK_ERROR);
});

test("parseWhatIfCount (wrong number of rank value)", () => {
  expect(parseWhatIfCount("1=-10")).toBe(WhatIfParserStatus.NUMBER_OF_RANKS_ERROR);
});
