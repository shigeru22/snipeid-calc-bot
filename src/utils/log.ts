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
 * Custom logging class.
 */
class Log {
  /**
   * Outputs logs using custom severity and function name format.
   *
   * @param { LogSeverity } severity Logging severity.
   * @param { string } source Function name.
   * @param { string } message Log message.
   */
  static log(severity: LogSeverity, source: string, message: string) {
    console.log(`[${ severities[severity] }] ${ source } :: ${ message }`);
  }

  static debug(source: string, message: string) {
    this.log(LogSeverity.DEBUG, source, message);
  }

  static info(source: string, message: string) {
    this.log(LogSeverity.LOG, source, message);
  }

  static warn(source: string, message: string) {
    this.log(LogSeverity.WARN, source, message);
  }

  static error(source: string, message: string) {
    this.log(LogSeverity.ERROR, source, message);
  }
}

export { LogSeverity, Log };
