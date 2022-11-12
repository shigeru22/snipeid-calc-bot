/**
 * Time operation enum.
 */
enum TimeOperation {
  DECREMENT,
  INCREMENT
}

class TimeUtils {
  /**
   * Converts delta time into its string representation.
   *
   * @param { number } ms Delta time in milliseconds.
   *
   * @returns { string } Delta time in string representation.
   */
  static deltaTimeToString(ms: number): string {
    {
      const years = Math.floor(ms / 31536000000);
      if(years >= 1) {
        return `${ years.toString() } year${ years !== 1 ? "s" : "" }`;
      }
    }

    {
      const months = Math.floor(ms / 2592000000);
      if(months >= 1) {
        return `${ months.toString() } month${ months !== 1 ? "s" : "" }`;
      }
    }

    {
      const days = Math.floor(ms / 86400000);
      if(days >= 1) {
        return `${ days.toString() } day${ days !== 1 ? "s" : "" }`;
      }
    }

    {
      const hours = Math.floor(ms / 3600000);
      if(hours >= 1) {
        return `${ hours.toString() } hour${ hours !== 1 ? "s" : "" }`;
      }
    }

    {
      const minutes = Math.floor(ms / 60000);
      if(minutes >= 1) {
        return `${ minutes.toString() } minute${ minutes !== 1 ? "s" : "" }`;
      }
    }

    const seconds = Math.floor(ms / 1000);
    return `${ seconds.toString() } second${ seconds !== 1 ? "s" : "" }`;
  }
}

export { TimeOperation, TimeUtils };
