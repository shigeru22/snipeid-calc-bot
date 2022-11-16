import { PoolClient } from "pg";

/**
 * Wrapper class for node-postgres' `PoolClient`.
 */
class PoolClientWrapper {
  /** Pool client object. */
  #poolClient: PoolClient;

  /** Whether this object's Pool client is under transaction. */
  #isUnderTransaction: boolean;

  /**
   * Wraps a `PoolClient` object.
   *
   * @param { PoolClient } client Instantiated Pool client object.
   */
  constructor(client: PoolClient) {
    this.#poolClient = client;
    this.#isUnderTransaction = false;
  }

  /**
   * Returns a `PoolClient` object.
   * Use `startTransaction()` and `releasePoolClient()` for proper transactions handling.
   *
   * @returns { PoolClient } Pool client object.
   */
  getPoolClient(): PoolClient {
    return this.#poolClient;
  }

  /**
   * Starts the transaction.
   * Further queries could be performed with `getPoolClient().query()` function and
   * finalization (or rolling back) could be performed using `releasePoolClient()` function.
   *
   * @returns { Promise<void> } Promise object with no return value.
   */
  async startTransaction(): Promise<void> {
    await this.#poolClient.query("BEGIN"); // TODO: implement try-catch for query functions
    this.#isUnderTransaction = true;
  }

  /**
   * Releases pool client resources and commits (or rollbacks) all changes.
   *
   * @param { boolean | undefined } commit Whether to commit all changes, defaults to `true`. Set to `false` to rollback.
   */
  async releasePoolClient(commit = true): Promise<void> {
    if(this.#isUnderTransaction) {
      await this.#poolClient.query(commit ? "COMMIT" : "ROLLBACK");
      this.#isUnderTransaction = false;
    }
    this.#poolClient.release();
  }
}

export default PoolClientWrapper;
