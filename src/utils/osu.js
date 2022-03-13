const axios = require("axios").default;
const { HTTPStatus } = require("./common");

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v1";

async function getAccessToken() {
  const id = parseInt(process.env.OSU_CLIENT_ID, 10);
  const secret = process.env.OSU_CLIENT_SECRET;

  try {
    const response = await axios.post("https://osu.ppy.sh/oauth/token", {
      client_id: id,
      client_secret: secret,
      grant_type: "client_credentials",
      scope: "public"
    });

    let ret = {};

    if(response.status === HTTPStatus.UNAUTHORIZED) {
      exitOnUnauthorizedError();
    }
    else if(response.status === HTTPStatus.OK) {
      ret = {
        token: response.data.access_token,
        expire: new Date((new Date()).getTime() + (response.data.expires_in * 1000))
      };
    }

    return ret;
  }
  catch (e) {
    if(axios.isAxiosError(e) || e instanceof Error) {
      console.log("[ERROR] getAccessToken :: " + e.name + ": " + e.message);
    }
    else {
      console.log("[ERROR] getAccessToken :: Unknown error occurred.");
    }

    process.exit(1);
  }
}

function exitOnUnauthorizedError() {
  console.log("[ERROR] getAccessToken :: Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
  process.exit(1);
}

module.exports = {
  getAccessToken
};
