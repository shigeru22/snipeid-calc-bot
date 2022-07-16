const { deltaTimeToString } = require("./time");

test("deltaTimeToString (seconds)", () => {
  expect(deltaTimeToString(1000)).toBe("1 second");
  expect(deltaTimeToString(10000)).toBe("10 seconds");
});

test("deltaTimeToString (minutes)", () => {
  expect(deltaTimeToString(60000)).toBe("1 minute");
  expect(deltaTimeToString(600000)).toBe("10 minutes");
});

test("deltaTimeToString (hours)", () => {
  expect(deltaTimeToString(3600000)).toBe("1 hour");
  expect(deltaTimeToString(36000000)).toBe("10 hours");
});

test("deltaTimeToString (days)", () => {
  expect(deltaTimeToString(86400000)).toBe("1 day");
  expect(deltaTimeToString(864000000)).toBe("10 days");
});

test("deltaTimeToString (months)", () => {
  expect(deltaTimeToString(2629800000)).toBe("1 month");
  expect(deltaTimeToString(26298000000)).toBe("10 months");
});

test("deltaTimeToString (years)", () => {
  expect(deltaTimeToString(31557600000)).toBe("1 year");
  expect(deltaTimeToString(315576000000)).toBe("10 years");
});
