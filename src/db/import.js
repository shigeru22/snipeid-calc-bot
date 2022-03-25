const { Client } = require("pg");

async function importRoles(db, roles) {
  console.log("Importing roles...");

  if(!(db instanceof Client)) {
    console.log("[ERROR] importRoles :: db must be a Client object instance.");
    return false;
  }

  const len = roles.length;
  let query = "INSERT INTO roles (discordId, roleName, minPoints) VALUES ";
  roles.forEach((role, index) => {
    query += "('" + role.discordId + "', '" + role.name + "', " + role.minPoints.toString() + ")";
    if(index < len - 1) {
      query += ", ";
    }
    else {
      query += ";";
    }
  });

  try {
    await db.query(query);

    console.log("[LOG] importRoles :: Role import completed.");
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
