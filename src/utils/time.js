const { LogSeverity, log } = require("../utils/log");

/**
 * Time operation enum.
 */
const TimeOperation = {
  DECREMENT: 0,
  INCREMENT: 1
};

/**
 * Checks whether time operation type is available in enum.
 *
 * @param { number | undefined } value - Time operation type to be checked.
 *
 * @returns { boolean | undefined } Whether time operation type is available in enum. Returns `undefined` if `value` is `undefined`.
 */
function isTimeOperationEnumAvailable(value) {
  if(typeof(value) === "undefined") {
    return undefined;
  }

  for(const prop in TimeOperation) {
    if(TimeOperation[prop] === value) {
      return true;
    }
  }

  return false;
}

/**
 * Converts delta time into its string representation.
 *
 * @param { number } ms - Delta time in milliseconds.
 *
 * @returns { string } Delta time in string representation.
 */
function deltaTimeToString(ms) {
  const years = Math.floor(ms / 31536000000);
  if(years >= 1) {
    return years.toString() + (years === 1 ? " year" : " years");
  }

  const months = Math.floor(ms / 2592000000);
  if(months >= 1) {
    return months.toString() + (months === 1 ? " month" : " months");
  }

  const days = Math.floor(ms / 86400000);
  if(days >= 1) {
    return days.toString() + (days === 1 ? " day" : " days");
  }

  const hours = Math.floor(ms / 3600000);
  if(hours >= 1) {
    return hours.toString() + (hours === 1 ? " hour" : " hours");
  }

  const minutes = Math.floor(ms / 60000);
  if(minutes >= 1) {
    return minutes.toString() + (minutes === 1 ? " minute" : " minutes");
  }

  const seconds = Math.floor(ms / 1000);
  return seconds.toString() + (seconds === 1 ? " second" : " seconds");
}

/**
 * Parses time offset from string.
 *
 * @param { string } value - Time offset string.
 *
 * @returns { { operation: number; hours: number; minutes: number; } | undefined } Time offset object. Returns `undefined` in case of errors.
 */
function getTimeOffsetFromString(value) {
  const temp = value.split(":");
  if(temp.length !== 2) {
    log(LogSeverity.ERROR, "getTimeOffsetFromString", "value must be in time format. Check .env-template for details.");
    return undefined;
  }

  const inc = temp[0].slice(0, 1);
  if(inc !== "+" && inc !== "-") {
    log(LogSeverity.ERROR, "getTimeOffsetFromString", "First value character must be '+' or '-'. Check .env-template for details.");
    return undefined;
  }

  const hours = parseInt(temp[0].slice(0 - (temp[0].length - 1)), 10);
  if(isNaN(hours) || (hours < 0 || hours > 14)) { // well, earliest timezone is 14, but who use that?
    log(LogSeverity.ERROR, "getTimeOffsetFromString", "Invalid hours value (0 to 14). Check .env-template for format details.");
    return undefined;
  }

  const minutes = parseInt(temp[1], 10);
  if(isNaN(minutes) || (minutes < 0 || minutes > 59)) {
    log(LogSeverity.ERROR, "getTimeOffsetFromString", "Invalid minutes value (0 to 59). Check .env-template for format details.");
    return undefined;
  }

  return {
    operation: inc === "+" ? TimeOperation.INCREMENT : TimeOperation.DECREMENT,
    hours,
    minutes
  };
}

module.exports = {
  TimeOperation,
  isTimeOperationEnumAvailable,
  deltaTimeToString,
  getTimeOffsetFromString
};
