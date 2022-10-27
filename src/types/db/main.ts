import { DatabaseErrors, DatabaseSuccess } from "../../utils/common";

/**
 * Database error response data generic interface.
 */
interface IDBErrorResponseData<T extends DatabaseErrors> {
  status: T;
}

/**
 * Database response data generic interface.
 */
interface IDBResponseData<T> {
  status: DatabaseSuccess.OK;
  data: T;
}

/**
 * Main database response data type.
 */
type DBResponseBase<T> = T extends DatabaseErrors ? IDBErrorResponseData<T> : IDBResponseData<T>;

/**
 * Checks whether response's type is `IDBErrorResponseData`.
 *
 * @param { unknown } response Response to be checked.
 *
 * @returns { response is IDBErrorResponseData<DatabaseErrors> } Returns `true` if response is an error, `false` otherwise.
 */
function isDatabaseErrorResponse(response: unknown): response is IDBErrorResponseData<DatabaseErrors> {
  return (response as IDBErrorResponseData<DatabaseErrors>).status !== DatabaseErrors.OK;
}

export { DBResponseBase, isDatabaseErrorResponse };
