const { Pool } = require("pg");
const { LogSeverity, log } = require("../log");
const { DatabaseErrors } = require("../common");

async function getDiscordUserByOsuId(db, osuId) {
  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "getDiscordUserByOsuId", "db must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    log(LogSeverity.ERROR, "getDiscordUserByOsuId", "osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM users WHERE osuId=$1";
  const selectValues = [ osuId ];

  try {
    const discordUserResult = await db.query(selectQuery, selectValues);

    if(typeof(discordUserResult.rows[0]) === "undefined")  {
      return DatabaseErrors.USER_NOT_FOUND;
    }

    if(discordUserResult.rows[0].osuid !== osuId) {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Invalid row returned.");
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
        log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "getDiscordUserByOsuId", "An error occurred while inserting user: " + e.message);
      }
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function getDiscordUserByDiscordId(db, discordId) {
  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "db must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(discordId) !== "string") {
    log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "discordId must be string.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectQuery = "SELECT * FROM users WHERE discordid=$1";
  const selectValues = [ discordId ];

  try {
    const result = await db.query(selectQuery, selectValues);
    if(typeof(result.rows[0]) === "undefined") {
      return DatabaseErrors.USER_NOT_FOUND;
    }

    return {
      userId: result.rows[0].userid,
      discordId: result.rows[0].discordid,
      osuId: result.rows[0].osuid
    };
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "An error occurred while inserting user: " + e.message);
      }
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function insertUser(db, discordId, osuId, userName) {
  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "insertUser", "db must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(discordId) !== "string") {
    log(LogSeverity.ERROR, "insertUser", "discordId must be string.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    log(LogSeverity.ERROR, "insertUser", "osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  const selectDiscordIdQuery = "SELECT * FROM users WHERE discordId=$1";
  const selectDiscordIdValues = [ discordId ];

  const selectOsuIdQuery = "SELECT * FROM users WHERE osuId=$1;"
  const selectOsuIdValues = [ osuId ];

  const insertQuery = "INSERT INTO users (discordId, osuId, userName) VALUES ($1, $2, $3)";
  const insertValues = [ discordId, osuId, userName ];

  try {  
    const client = await db.connect();

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

    log(LogSeverity.LOG, "insertUser", "users: Inserted 1 row.");
    return DatabaseErrors.OK;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "insertUser", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "insertUser", "An error occurred while inserting user: " + e.message);
      }
    }
    else {
      log(LogSeverity.ERROR, "insertUser", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

async function updateUser(db, osuId, userName) { // only username should be updateable, even that changes are from osu! API
  if(!(db instanceof Pool)) {
    log(LogSeverity.ERROR, "updateUser", "db must be a Pool object instance.");
    return DatabaseErrors.TYPE_ERROR;
  }

  if(typeof(osuId) !== "number") {
    log(LogSeverity.ERROR, "updateUser", "osuId must be number.");
    return DatabaseErrors.TYPE_ERROR;
  }

  try {
    await db.query("UPDATE users SET username=$1 WHERE osuid=$2", [ userName, osuId ]);

    log(LogSeverity.LOG, "updateUser", "users: Updated 1 row.");
    return DatabaseErrors.OK;
  }
  catch (e) {
    if(e instanceof Error) {
      if(e.code === "ECONNREFUSED") {
        log(LogSeverity.ERROR, "updateUser", "Database connection failed.");
        return DatabaseErrors.CONNECTION_ERROR;
      }
      else {
        log(LogSeverity.ERROR, "updateUser", "An error occurred while updating user: " + e.message + "\n" + e.stack);
      }
    }
    else {
      log(LogSeverity.ERROR, "updateUser", "Unknown error occurred.");
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
