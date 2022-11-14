import { Guild } from "discord.js";
import { Pool } from "pg";
import { DBServers } from "../db";
import { Log } from "../utils/log";
import { ConflictError, DuplicatedRecordError } from "../errors/db";

/**
 * Server-related actions class.
 */
class Servers {
  /**
   * Post-joined new server event function.
   *
   * @param db Database pool object.
   * @param guild Joined server object.
   */
  static async onJoinServer(db: Pool, guild: Guild) {
    try {
      await DBServers.insertServer(db, guild.id);
      Log.info("onJoinServer", `Joined server with ID ${ guild.id } (${ guild.name }).`);
    }
    catch (e) {
      if(e instanceof ConflictError) {
        if(e.column === "discordId") {
          Log.info("onJoinServer", `Rejoined server with ID ${ guild.id } (${ guild.name }).`);
        }
        else {
          Log.error("onJoinServer", `Unknown data conflict occurred while inserting server ID ${ guild.id } (${ guild.name })`);
        }
      }
      else if(e instanceof DuplicatedRecordError) {
        Log.error("onJoinServer", `Duplicated server data found with server ID ${ guild.id } (${ guild.name }).`);
      }
      else {
        Log.error("onJoinServer", `Failed to query database after joining server ID ${ guild.id } (${ guild.name }).`);
      }
    }
  }
}

export default Servers;
