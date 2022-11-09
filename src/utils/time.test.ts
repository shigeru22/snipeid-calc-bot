import { TimeUtils } from "./time";

test("TimeUtils.deltaTimeToString (seconds)", () => {
  expect(TimeUtils.deltaTimeToString(1000)).toBe("1 second");
  expect(TimeUtils.deltaTimeToString(10000)).toBe("10 seconds");
});

test("TimeUtils.deltaTimeToString (minutes)", () => {
  expect(TimeUtils.deltaTimeToString(60000)).toBe("1 minute");
  expect(TimeUtils.deltaTimeToString(600000)).toBe("10 minutes");
});

test("TimeUtils.deltaTimeToString (hours)", () => {
  expect(TimeUtils.deltaTimeToString(3600000)).toBe("1 hour");
  expect(TimeUtils.deltaTimeToString(36000000)).toBe("10 hours");
});

test("TimeUtils.deltaTimeToString (days)", () => {
  expect(TimeUtils.deltaTimeToString(86400000)).toBe("1 day");
  expect(TimeUtils.deltaTimeToString(864000000)).toBe("10 days");
});

test("TimeUtils.deltaTimeToString (months)", () => {
  expect(TimeUtils.deltaTimeToString(2629800000)).toBe("1 month");
  expect(TimeUtils.deltaTimeToString(26298000000)).toBe("10 months");
});

test("TimeUtils.deltaTimeToString (years)", () => {
  expect(TimeUtils.deltaTimeToString(31557600000)).toBe("1 year");
  expect(TimeUtils.deltaTimeToString(315576000000)).toBe("10 years");
});
