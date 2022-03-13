const { Client } = require("pg");

async function importRoles(db, roles) {
  console.log("Importing roles...");

  if(!(db instanceof Client)) {
    console.log("[ERROR] Invalid variable given: Not a node-postgres Client.");
    console.log("Exiting...");
    return false;
  }

  const len = roles.length;
  let query = "INSERT INTO roles (discordId, roleName) VALUES ";
  roles.forEach((role, index) => {
    query += "('" + role.discordId + "', '" + role.name + "')";
    if(index < len - 1) {
      query += ", ";
    }
    else {
      query += ";";
    }
  });

  try {
    await db.query(query);

    console.log("Role import completed.");
    return true;
  }
  catch (e) {
    if(e instanceof Error) {
      console.log("[ERROR] An error occurred while querying database: " + e.message);
    }
    else {
      console.log("[ERROR] An unknown error occurred.");
    }

    return false;
  }
}

module.exports = {
  importRoles
};