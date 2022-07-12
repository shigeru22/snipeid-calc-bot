/**
 * Logging severity enum.
 */
const LogSeverity = {
  DEBUG: 1,
  LOG: 2,
  WARN: 3,
  ERROR: 4
};

const severities = [ "DEBUG", "LOG", "WARN", "ERROR" ];

/**
 * Checks whether severity type is available in enum.
 *
 * @param { number } value
 * 
 * @returns { boolean | undefined }
 */
function isSeverityEnumAvailable(value) {
  if(typeof(value) === "undefined") {
    return undefined;
  }

  for(const prop in LogSeverity) {
    if(LogSeverity[prop] === value) {
      return true;
    }
  }

  return false;
}

/**
 * Outputs logs using custom severity and function name format.
 *
 * @param { number } severity
 * @param { string } source
 * @param { string } message
 *
 * @returns { void }
 */
function log(severity, source, message) {
  if(!isSeverityEnumAvailable(severity)) {
    console.log("[" + severities[3] + "] " + "log" + " :: " + "severity value not available in LogSeverity enum.");
    return;
  }

  console.log("[" + severities[severity - 1] + "] " + source + " :: " + message);
}

module.exports = {
  LogSeverity,
  log
};
