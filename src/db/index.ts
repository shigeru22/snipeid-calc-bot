import { PoolConfig } from "pg";
import DBAssignments from "./assignments";
import DBRoles from "./roles";
import DBUsers from "./users";
import DBServers from "./servers";
import { NoConfigError } from "../errors/db";

/**
 * Wrapper class for all database classes.
 */
class DatabaseWrapper {
  // disabling no-use-before-define is done to enable singleton patterns

  /** Static wrapper instance. */
  // eslint-disable-next-line no-use-before-define
  static #instance: DatabaseWrapper | undefined = undefined;

  /**
   * Returns `DatabaseWrapper` instance.
   * Instantiates a new instance if previously no created in session.
   *
   * @returns `DatabaseWrapper` instance.
   */
  static getInstance(): DatabaseWrapper {
    if(DatabaseWrapper.#instance === undefined) {
      DatabaseWrapper.#instance = new DatabaseWrapper();
    }

    return DatabaseWrapper.#instance;
  }

  /** `DBAssignments` class instance. */
  #dbAssignments: DBAssignments | null = null;

  /** `DBRoles` class instance. */
  #dbRoles: DBRoles | null = null;

  /** `DBUsers` class instance. */
  #dbUsers: DBUsers | null = null;

  /** `DBServers` class instance. */
  #dbServers: DBServers | null = null;

  /**
   * Sets config and instantiates all database class instances with the specified configuration.
   *
   * @param { PoolConfig } config Database pool configuration object.
   */
  setConfig(config: PoolConfig) {
    this.#dbAssignments = new DBAssignments(config);
    this.#dbRoles = new DBRoles(config);
    this.#dbUsers = new DBUsers(config);
    this.#dbServers = new DBServers(config);
  }

  /**
   * Returns `DBAssignments` class object instance.
   *
   * @returns { DBAssignments } `DBAssignments` class instance.
   *
   * @throws { NoConfigError } Object not yet instantiated. Make sure to run `setConfig` once before returning any modules.
   */
  getAssignmentsModule(): DBAssignments {
    if(this.#dbAssignments === null) {
      throw new NoConfigError();
    }

    return this.#dbAssignments;
  }

  /**
   * Returns `DBRoles` class object instance.
   *
   * @returns { DBRoles } `DBRoles` class instance.
   *
   * @throws { NoConfigError } Object not yet instantiated. Make sure to run `setConfig` once before returning any modules.
   */
  getRolesModule(): DBRoles {
    if(this.#dbRoles === null) {
      throw new NoConfigError();
    }

    return this.#dbRoles;
  }

  /**
   * Returns `DBUsers` class object instance.
   *
   * @returns { DBUsers } `DBUsers` class instance.
   *
   * @throws { NoConfigError } Object not yet instantiated. Make sure to run `setConfig` once before returning any modules.
   */
  getUsersModule(): DBUsers {
    if(this.#dbUsers === null) {
      throw new NoConfigError();
    }

    return this.#dbUsers;
  }

  /**
   * Returns `DBServers` class object instance.
   *
   * @returns { DBServers } `DBServers` class instance.
   *
   * @throws { NoConfigError } Object not yet instantiated. Make sure to run `setConfig` once before returning any modules.
   */
  getServersModule(): DBServers {
    if(this.#dbServers === null) {
      throw new NoConfigError();
    }

    return this.#dbServers;
  }
}

export { DatabaseWrapper, DBAssignments, DBRoles, DBUsers, DBServers };
