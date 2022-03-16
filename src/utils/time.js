function deltaTimeToString(ms) {
  if(typeof(ms) !== "number") {
    console.log("[ERROR] deltaTimeToString :: ms is not number.");
    return 0;
  }

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

module.exports = {
  deltaTimeToString
};
