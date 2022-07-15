const { DatabaseError } = require("pg");
const { LogSeverity, log } = require("../log");
const { DatabaseErrors } = require("../common");

/**
 * Gets Discord user by osu! ID from the database.
 *
 * @param { import("pg").Pool } db
 * @param { number } osuId
 *
 * @returns { Promise<{ userId: number; discordId: string; osuId: number; } | number> } Promise object with user data. Returns non-zero `DatabaseErrors` constant in case of errors.
 */
async function getDiscordUserByOsuId(db, osuId) {
  const selectQuery = "SELECT * FROM users WHERE osuId=$1";
  const selectValues = [ osuId ];

  try {
    const discordUserResult = await db.query(selectQuery, selectValues);

    if(typeof(discordUserResult.rows[0]) === "undefined") {
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
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Database connection failed.");
          return DatabaseErrors.CONNECTION_ERROR;
        default:
          log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserByOsuId", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

/**
 * Gets Discord user by Discord ID from the database.
 *
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } discordId - Discord ID of the user.
 *
 * @returns { Promise<{ userId: number; discordId: string; osuId: number; } | number> } Promise object with user data. Returns non-zero `DatabaseErrors` constant in case of errors.
 */
async function getDiscordUserByDiscordId(db, discordId) {
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
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Database connection failed.");
          return DatabaseErrors.CONNECTION_ERROR;
        default:
          log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "getDiscordUserByDiscordId", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

/**
 * Inserts user to the database.
 *
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { string } discordId - Discord ID of the user.
 * @param { number } osuId - osu! user ID.
 * @param { string } userName - osu! username.
 *
 * @returns { Promise<number> } Promise object with `DatabaseErrors.OK` after successful insertion. Returns non-zero `DatabaseErrors` constant in case of errors.
 */
async function insertUser(db, discordId, osuId, userName) {
  const selectDiscordIdQuery = "SELECT * FROM users WHERE discordId=$1";
  const selectDiscordIdValues = [ discordId ];

  const selectOsuIdQuery = "SELECT * FROM users WHERE osuId=$1;";
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
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "insertUser", "Database connection failed.");
          return DatabaseErrors.CONNECTION_ERROR;
        default:
          log(LogSeverity.ERROR, "insertUser", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "insertUser", "An error occurred while querying assignment: " + e.message);
    }
    else {
      log(LogSeverity.ERROR, "insertUser", "Unknown error occurred.");
    }

    return DatabaseErrors.CLIENT_ERROR;
  }
}

/**
 * Updates user in the database (username only).
 *
 * @param { import("pg").Pool } db - Database connection pool.
 * @param { number } osuId - osu! user ID.
 * @param { string } userName - osu! username.
 *
 * @returns { Promise<number> }
 */
async function updateUser(db, osuId, userName) { // only username should be updateable, since that changes are from osu! API
  try {
    await db.query("UPDATE users SET username=$1 WHERE osuid=$2", [ userName, osuId ]);

    log(LogSeverity.LOG, "updateUser", "users: Updated 1 row.");
    return DatabaseErrors.OK;
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "updateUser", "Database connection failed.");
          return DatabaseErrors.CONNECTION_ERROR;
        default:
          log(LogSeverity.ERROR, "updateUser", "Database error occurred:\n" + e.code + ": " + e.message + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "updateUser", "An error occurred while querying assignment: " + e.message);
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
