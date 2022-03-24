const { Pool } = require("pg");
const { DatabaseErrors } = require("../common");

async function getDiscordUserByOsuId(pool, osuId) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] getDiscordUserByOsuId :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] getDiscordUserByOsuId :: osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM users WHERE osuId=$1";
  const selectValues = [ osuId ];

  try {
    const client = await pool.connect();

    const discordUserResult = await client.query(selectQuery, selectValues);
    client.release();

    if(typeof(discordUserResult.rows[0]) === "undefined")  {
      return DatabaseErrors.USER_NOT_FOUND;
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      console.log("[ERROR] getDiscordUserByOsuId :: Invalid row returned.");
      return DatabaseErrors.CLIENT_ERROR;
    }

    return {
      userId: discordUserResult.rows[0].userid,
      discordId: discordUserResult.rows[0].discordid,
      osuId: discordUserResult.rows[0].osuid
    };
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        console.log("[ERROR] getDiscordUserByOsuId :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] getDiscordUserByOsuId :: An error occurred while inserting user: " + e.message);
      }
    }
    else {
      console.log("[ERROR] getDiscordUserByOsuId :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function getDiscordUserByDiscordId(pool, discordId) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] getDiscordUserByDiscordId :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(discordId) !== "string") {
    console.log("[ERROR] getDiscordUserByDiscordId :: discordId must be string.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM users WHERE discordid=$1";
  const selectValues = [ discordId ];

  try {
    const client = await pool.connect();

    const result = await client.query(selectQuery, selectValues);
    if(typeof(result.rows[0]) === "undefined") {
      client.release();
      return DatabaseErrors.USER_NOT_FOUND;
    }

    client.release();

    return {
      userId: result.rows[0].userid,
      discordId: result.rows[0].discordid,
      osuId: result.rows[0].osuid
    };
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        console.log("[ERROR] getDiscordUserByDiscordId :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] getDiscordUserByDiscordId :: An error occurred while inserting user: " + e.message);
      }
    }
    else {
      console.log("[ERROR] getDiscordUserByDiscordId :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function insertUser(pool, discordId, osuId, userName) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] insertUser :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(discordId) !== "string") {
    console.log("[ERROR] insertUser :: discordId must be string.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] insertUser :: osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectDiscordIdQuery = "SELECT * FROM users WHERE discordId=$1";
  const selectDiscordIdValues = [ discordId ];

  const selectOsuIdQuery = "SELECT * FROM users WHERE osuId=$1;"
  const selectOsuIdValues = [ osuId ];

  const insertQuery = "INSERT INTO users (discordId, osuId, userName) VALUES ($1, $2, $3)";
  const insertValues = [ discordId, osuId, userName ];

  try {  
    const client = await pool.connect();

    const discordIdResult = await client.query(selectDiscordIdQuery, selectDiscordIdValues);
    if(typeof(discordIdResult.rows[0]) !== "undefined") {
      if(discordIdResult.rows[0].discordid === discordId) {
        client.release();
        return DatabaseErrors.DUPLICATED_DISCORD_ID;
      }
    }
    
    const osuIdResult = await client.query(selectOsuIdQuery, selectOsuIdValues);
    if(typeof(osuIdResult.rows[0]) !== "undefined") {
      if(osuIdResult.rows[0].osuid === osuId) {
        client.release();
        return DatabaseErrors.DUPLICATED_OSU_ID;
      }
    }

    await client.query(insertQuery, insertValues);

    client.release();

    console.log("[LOG] insertUser :: users: Inserted 1 row.");
    return DatabaseErrors.OK;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        console.log("[ERROR] insertUser :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] insertUser :: An error occurred while inserting user: " + e.message);
      }
    }
    else {
      console.log("[ERROR] insertUser :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function updateUser(pool, osuId, userName) { // only username should be updateable, even that changes are from osu! API
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] updateUser :: pool must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] updateUser :: osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  try {
    const client = await pool.connect();
    await client.query("UPDATE users SET username=$1 WHERE osuid=$2", [ userName, osuId ]);

    client.release();
    console.log("[LOG] updateUser :: users: Updated 1 row.");
    return DatabaseErrors.OK;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        console.log("[ERROR] updateUser :: Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        console.log("[ERROR] updateUser :: An error occurred while " + (insert ? "inserting" : "updating") + " assignment: " + e.message + "\n" + e.stack);
      }
    }
    else {
      console.log("[ERROR] updateUser :: Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

module.exports = {
  getDiscordUserByOsuId,
  getDiscordUserByDiscordId,
  insertUser,
  updateUser
};
