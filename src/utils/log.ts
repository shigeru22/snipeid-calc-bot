/**
 * Logging severity enum.
 */
enum LogSeverity {
  DEBUG,
  LOG,
  WARN,
  ERROR
}

const severities = [ "DEBUG", "LOG", "WARN", "ERROR" ];

/**
 * Outputs logs using custom severity and function name format.
 *
 * @param { LogSeverity } severity Logging severity.
 * @param { string } source Function name.
 * @param { string } message Log message.
 */
function log(severity: LogSeverity, source: string, message: string) {
  console.log(`[${ severities[severity] }] ${ source } :: ${ message }`);
}

export { LogSeverity, log };
