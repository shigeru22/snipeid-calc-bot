import { Pool, PoolConfig } from "pg";
import PoolClientWrapper from "./pool-client";

class DBConnectorBase {
  #db: Pool;

  constructor(config: PoolConfig) {
    this.#db = new Pool(config);
  }

  getPool(): Pool {
    return this.#db;
  }

  async getPoolClient(): Promise<PoolClientWrapper> {
    const client = await this.#db.connect();
    const ret = new PoolClientWrapper(client);
    return ret;
  }
}

export default DBConnectorBase;
