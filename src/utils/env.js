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

  if(typeof(process.env.LEADERBOARD_CHANNEL_ID) !== "string" || !process.env.LEADERBOARD_CHANNEL_ID) {
    console.log("[ERROR] LEADERBOARD_CHANNEL_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(typeof(process.env.SERVER_ID) === "string" && process.env.SERVER_ID !== "") {
    // verify role ID if server ID is defined
    if(typeof(process.env.VERIFIED_ROLE_ID) !== "string" || !process.env.VERIFIED_ROLE_ID) {
      console.log("[ERROR] If SERVER_ID is defined, VERIFIED_ROLE_ID must be defined in environment variables. Exiting.");
      return false;
    }
  }

  if(typeof(process.env.OSU_CLIENT_ID) !== "string" || !process.env.OSU_CLIENT_ID) {
    console.log("[ERROR] OSU_CLIENT_ID must be defined in environment variables. Exiting.");
    return false;
  }

  if(parseInt(process.env.OSU_CLIENT_ID, 10) === NaN) {
    console.log("[ERROR] OSU_CLIENT_ID must be number. Exiting.");
    return false;
  }

  if(typeof(process.env.OSU_CLIENT_SECRET) !== "string" || !process.env.OSU_CLIENT_SECRET) {
    console.log("[ERROR] OSU_CLIENT_SECRET must be defined in environment variables. Exiting.");
    return false;
  }

  console.log("Environment variable checks completed.");
  return true;
}

module.exports = {
  validateEnvironmentVariables
};
