const axios = require("axios").default;
const { HTTPStatus, OsuUserStatus } = require("../common");

const OSU_API_ENDPOINT = "https://osu.ppy.sh/api/v2";

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

    if(response.status === HTTPStatus.OK) {
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

      if(response.status === HTTPStatus.UNAUTHORIZED) {
        exitOnUnauthorizedError();
      }
    }
    else {
      console.log("[ERROR] getAccessToken :: Unknown error occurred.");
    }

    process.exit(1);
  }
}

async function getUserByOsuId(token, id) {
  if(typeof(id) !== "number") {
    console.log("[ERROR] getUserByOsuId :: id argument passed is not number.");
    process.exit(1);
  }

  try {
    const response = await axios.get(OSU_API_ENDPOINT + "/users/" + id.toString(), {
      params: {
        key: "id"
      },
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Authorization": "Bearer " + token
      }
    });

    let res = {};

    if(response.status === HTTPStatus.OK) {
      if(response.data.is_bot) {
        res = {
          status: OsuUserStatus.BOT
        };
      }
      else if(response.data.is_deleted) {
        res = {
          status: OsuUserStatus.DELETED
        };
      }
      else {
        res = {
          status: OsuUserStatus.USER,
          username: response.data.username
        };
      }
    }

    return res;
  }
  catch (e) {
    let res = {};

    if(axios.isAxiosError(e)) {
      if(e.response.status === HTTPStatus.UNAUTHORIZED) {
        exitOnUnauthorizedError();
      }
      else if(e.response.status === HTTPStatus.NOT_FOUND) {
        res = {
          status: OsuUserStatus.NOT_FOUND
        }
      }
    }
    else if(e instanceof Error) {
      console.log("[ERROR] getUserByOsuId :: " + e.name + ": " + e.message);
    }
    else {
      console.log("[ERROR] getUserByOsuId :: Unknown error occurred.");
      process.exit(1);
    }

    return res;
  }
}

function exitOnUnauthorizedError() {
  console.log("[ERROR] getAccessToken :: Failed to authenticate client. Check OSU_CLIENT_ID and OSU_CLIENT_SECRET variables, and try again.");
  process.exit(1);
}

module.exports = {
  getAccessToken,
  getUserByOsuId
};
