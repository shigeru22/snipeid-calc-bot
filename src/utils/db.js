const { Pool } = require("pg");

const DatabaseErrors = {
  OK: 0,
  CONNECTION_ERROR: 1,
  TYPE_ERROR: 2,
  DUPLICATED: 3,
  CLIENT_ERROR: 4
};

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

  const query = "INSERT INTO users (discordId, osuId) VALUES ($1, $2);";
  const values = [ discordId, osuId ];

  try {  
    const client = await pool.connect();
    await client.query(query, values);
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

module.exports = {
  DatabaseErrors,
  insertUser
};
