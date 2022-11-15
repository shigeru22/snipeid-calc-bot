import { PoolConfig } from "pg";
import DBAssignments from "./assignments";
import DBRoles from "./roles";
import DBUsers from "./users";
import DBServers from "./servers";
import { NoConfigError } from "../errors/db";

class DatabaseWrapper {
  // this is done to enable singleton patterns
  // eslint-disable-next-line no-use-before-define
  static #instance: DatabaseWrapper | undefined = undefined;

  static getInstance(): DatabaseWrapper {
    if(DatabaseWrapper.#instance === undefined) {
      DatabaseWrapper.#instance = new DatabaseWrapper();
    }

    return DatabaseWrapper.#instance;
  }

  #dbAssignments: DBAssignments | null = null;
  #dbRoles: DBRoles | null = null;
  #dbUsers: DBUsers | null = null;
  #dbServers: DBServers | null = null;

  setConfig(config: PoolConfig) {
    this.#dbAssignments = new DBAssignments(config);
    this.#dbRoles = new DBRoles(config);
    this.#dbUsers = new DBUsers(config);
    this.#dbServers = new DBServers(config);
  }

  getAssignmentsModule(): DBAssignments {
    if(this.#dbAssignments === null) {
      throw new NoConfigError();
    }

    return this.#dbAssignments;
  }

  getRolesModule(): DBRoles {
    if(this.#dbRoles === null) {
      throw new NoConfigError();
    }

    return this.#dbRoles;
  }

  getUsersModule(): DBUsers {
    if(this.#dbUsers === null) {
      throw new NoConfigError();
    }

    return this.#dbUsers;
  }

  getServersModule(): DBServers {
    if(this.#dbServers === null) {
      throw new NoConfigError();
    }

    return this.#dbServers;
  }
}

export { DatabaseWrapper, DBAssignments, DBRoles, DBUsers, DBServers };
