import { Parser } from "./parser";
import { InvalidExpressionError, InvalidTypeError, InvalidNumberOfRanksError, InvalidTopRankError } from "../errors/utils/parser";

test("parseTopCountDescription", () => {
  const description = "```\nTop 1 :    52\nTop 8 : 1,892\nTop 15: 3,733\nTop 25: 5,404\nTop 50: 7,496\n```";
  expect(Parser.parseTopCountDescription(description)).toStrictEqual([ 52, 1892, 3733, 5404, 7496 ]);
});

test("parseUsername", () => {
  const title = "In how many top X map leaderboards is Akshiro?";
  expect(Parser.parseUsername(title)).toBe("Akshiro");
});

test("parseOsuIdFromLink (without mode routing)", () => {
  const link = "https://osu.ppy.sh/users/10557490";
  expect(Parser.parseOsuIdFromLink(link)).toBe("10557490");
});

test("parseOsuIdFromLink (with mode routing)", () => {
  const link = "https://osu.ppy.sh/users/10557490/osu";
  expect(Parser.parseOsuIdFromLink(link)).toBe("10557490");
});

test("parseWhatIfCount", () => {
  expect(Parser.parseWhatIfCount("1=5")).toStrictEqual([ 1, 5 ]);
});

test("parseWhatIfCount (invalid expression)", () => {
  expect(Parser.parseWhatIfCount("1=5=10")).toThrowError(InvalidExpressionError);
});

test("parseWhatIfCount (wrong type)", () => {
  expect(Parser.parseWhatIfCount("1=a")).toThrowError(InvalidTypeError);
  expect(Parser.parseWhatIfCount("a=5")).toThrowError(InvalidTypeError);
});

test("parseWhatIfCount (wrong top rank value)", () => {
  expect(Parser.parseWhatIfCount("0=5")).toThrowError(InvalidTopRankError);
});

test("parseWhatIfCount (wrong number of rank value)", () => {
  expect(Parser.parseWhatIfCount("1=-10")).toThrowError(InvalidNumberOfRanksError);
});
