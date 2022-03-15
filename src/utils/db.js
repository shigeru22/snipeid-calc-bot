const { Pool } = require("pg");

const DatabaseErrors = {
  OK: 0,
  CONNECTION_ERROR: 1,
  TYPE_ERROR: 2,
  DUPLICATED_DISCORD_ID: 3,
  DUPLICATED_OSU_ID: 4,
  NOT_FOUND: 5,
  CLIENT_ERROR: 6
};

/* user operations */

async function insertUser(pool, discordId, osuId) {
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

  const insertQuery = "INSERT INTO users (discordId, osuId) VALUES ($1, $2);";
  const insertValues = [ discordId, osuId ];

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
      return DatabaseErrors.NOT_FOUND;
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
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

module.exports = {
  DatabaseErrors,
  insertUser,
  getDiscordUserByOsuId
};
