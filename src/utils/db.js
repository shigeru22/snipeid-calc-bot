const { Pool } = require("pg");

async function insertUser(pool, discordId, osuId) {
  if(!(pool instanceof Pool)) {
    console.log("[ERROR] insertUser :: pool must be a Pool object instance.");
    return false;
  }

  if(typeof(discordId) !== "string") {
    console.log("[ERROR] insertUser :: discordId must be string.");
    return false;
  }

  if(typeof(osuId) !== "number") {
    console.log("[ERROR] insertUser :: osuId must be number.");
    return false;
  }

  const query = "INSERT INTO users (discordId, osuId) VALUES ($1, $2);";
  const values = [ discordId, osuId ];

  try {  
    console.log("[INFO] Test");
    const client = await pool.connect();
    await client.query(query, values);
    client.release();

    console.log("[LOG] insertUser :: users: Inserted 1 row.");
    return true;
  }
  catch (e) {
    if(e instanceof Error) {
      console.log("[ERROR] insertUser :: An error occurred while inserting user: " + e.message);
    }
    else {
      console.log("[ERROR] insertUser :: Unknown error occurred.");
    }

    return false;
  }
}

module.exports = {
  insertUser
};
