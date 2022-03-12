function validateEnvironmentVariables() {
  console.log("Checking for environment variables...");

  if(typeof(process.env.BOT_NAME) !== "string" || !process.env.BOT_NAME) {
    console.log("[ERROR] BOT_NAME must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.BOT_TOKEN) !== "string" || !process.env.BOT_TOKEN) {
    console.log("[ERROR] BOT_TOKEN must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.CHANNEL_ID) !== "string" || !process.env.CHANNEL_ID) {
    console.log("[ERROR] CHANNEL_ID must be defined in environment variables. Exiting.");
    return false;
  }

  console.log("Environment variable checks completed.");
  return true;
}

module.exports = {
  validateEnvironmentVariables
};
