import { PoolClient } from "pg";

class PoolClientWrapper {
  #poolClient: PoolClient;
  #isUnderTransaction: boolean;

  constructor(client: PoolClient) {
    this.#poolClient = client;
    this.#isUnderTransaction = false;
  }

  getPoolClient(): PoolClient {
    return this.#poolClient;
  }

  async startTransaction(): Promise<void> {
    await this.#poolClient.query("BEGIN");
    this.#isUnderTransaction = true;
  }

  async releasePoolClient(commit = true): Promise<void> {
    if(this.#isUnderTransaction) {
      await this.#poolClient.query(commit ? "COMMIT" : "ROLLBACK");
      this.#isUnderTransaction = false;
    }
    this.#poolClient.release();
  }
}

export default PoolClientWrapper;
