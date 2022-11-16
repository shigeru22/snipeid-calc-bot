import { Pool, PoolConfig } from "pg";
import PoolClientWrapper from "./pool-client";

/**
 * Base class for database connectors.
 */
class DBConnectorBase {
  /** Database pool object. */
  #db: Pool;

  /**
   * Instantiates a `DBConnectorBase` object.
   *
   * @param { PoolConfig } config Database pool configuration.
   */
  constructor(config: PoolConfig) {
    this.#db = new Pool(config);
  }

  /**
   * Gets this instance's database pool object.
   *
   * @returns { Pool } Database pool object.
   */
  getPool(): Pool {
    return this.#db;
  }

  /**
   * Creates a pool client for transactions.
   *
   * @returns { Promise<PoolClientWrapper> } Promise object with `PoolClientWrapper` object.
   */
  async getPoolClient(): Promise<PoolClientWrapper> {
    const client = await this.#db.connect();
    const ret = new PoolClientWrapper(client);
    return ret;
  }
}

export default DBConnectorBase;
