import { Pool, DatabaseError } from "pg";
import { LogSeverity, log } from "../../src/utils/log";
import { DatabaseErrors, DatabaseSuccess } from "../../src/utils/common";
import { DBResponseBase } from "../../src/types/db/main";
import { ITableColumnNameQueryData, ITableColumnNameData } from "../types/tables";

/**
 * Retrieves column names for the specified table name.
 *
 * @param { Pool } db Database pool object.
 * @param { string } tableName Table to be queried.
 *
 * @returns { Promise<DBResponseBase<ITableColumnNameData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with table's column name data array.
 */
async function getColumnNames(db: Pool, tableName: string): Promise<DBResponseBase<ITableColumnNameData[]> | DBResponseBase<DatabaseErrors.NO_RECORD | DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const selectQuery = `
    SELECT
      table_name as tablename,
      column_name as columnname
    FROM
      information_schema.columns
    WHERE
      table_catalog = $1 AND table_name = $2
  `;
  const selectValues = [ process.env.DB_DATABASE, tableName ];

  try {
    const result = await db.query<ITableColumnNameQueryData>(selectQuery, selectValues);

    if(result.rows.length <= 0) {
      return {
        status: DatabaseErrors.NO_RECORD
      };
    }

    return {
      status: DatabaseSuccess.OK,
      data: result.rows.map(row => ({
        tableName: row.tablename,
        columnName: row.columnname
      }))
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "getColumnNames", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "getColumnNames", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "getColumnNames", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "getColumnNames", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Adds a column to the specified table.
 *
 * **Warning:** This function's altering constraint queries are not sanitized. Only run while migrating the database in trusted environment!
 *
 * @param { Pool } db Database pool object.
 * @param { string } tableName Table to be altered.
 * @param { string } columnName Column name to be added.
 * @param { string } columnType Column data type.
 * @param { string } foreignKeyTable Foreign key table. Leave `undefined` to disable constraint.
 * @param { string } foreignKeyColumn Foreign key column. Leave `undefined` to disable constraint.
 *
 * @returns { Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with `true` if altered successfully. Returns `DatabaseErrors` enum otherwise.
 */
async function addColumnToTable(db: Pool, tableName: string, columnName: string, columnType: string, foreignKeyTable?: string, foreignKeyColumn?: string): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  let addConstraint = false;

  if(foreignKeyTable !== undefined && foreignKeyColumn !== undefined) {
    addConstraint = true;
  }
  else if((foreignKeyTable === undefined && foreignKeyColumn !== undefined) || (foreignKeyTable !== undefined && foreignKeyColumn === undefined)) {
    log(LogSeverity.ERROR, "addColumnToTable", "Must specify either none or both foreignKeyTable and foreignKeyColumn values. Constraint won't be added.");
  }

  const alterColumnQuery = `
    ALTER TABLE ${ tableName }
      ADD COLUMN ${ columnName } ${ columnType }
  `;

  const alterConstraintQuery = `
    ALTER TABLE ${ tableName }
      ADD CONSTRAINT fk_${ columnName }
        FOREIGN KEY(${ columnName }) REFERENCES ${ foreignKeyTable }(${ foreignKeyColumn })
  `;

  try {
    const client = await db.connect();

    await client.query(alterColumnQuery);
    if(addConstraint) {
      await client.query(alterConstraintQuery);
    }

    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "addColumnToTable", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "addColumnToTable", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "addColumnToTable", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "addColumnToTable", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

/**
 * Alters a column of the specified table.
 *
 * **Warning:** This function's altering constraint queries are not sanitized. Only run while migrating the database in trusted environment!
 *
 * @param { Pool } db Database pool object.
 * @param { string } tableName Table to be altered.
 * @param { string } columnName Column name to be altered.
 * @param { string } expression Expression for altering the specified table.
 *
 * @returns { Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> } Promise object with `true` if altered successfully. Returns `DatabaseErrors` enum otherwise.
 */
async function alterTableColumn(db: Pool, tableName: string, columnName: string, expression: string): Promise<DBResponseBase<true> | DBResponseBase<DatabaseErrors.CONNECTION_ERROR | DatabaseErrors.CLIENT_ERROR>> {
  const alterColumnQuery = `
    ALTER TABLE ${ tableName }
      ALTER COLUMN ${ columnName } ${ expression }
  `;

  try {
    await db.query(alterColumnQuery);

    return {
      status: DatabaseSuccess.OK,
      data: true
    };
  }
  catch (e) {
    if(e instanceof DatabaseError) {
      switch(e.code) {
        case "ECONNREFUSED":
          log(LogSeverity.ERROR, "addColumnToTable", "Database connection failed.");
          return {
            status: DatabaseErrors.CONNECTION_ERROR
          };
        default:
          log(LogSeverity.ERROR, "addColumnToTable", "Database error occurred. Exception details below." + "\n" + `${ e.code }: ${ e.message }` + "\n" + e.stack);
      }
    }
    else if(e instanceof Error) {
      log(LogSeverity.ERROR, "addColumnToTable", "An error occurred while executing query. Exception details below." + "\n" + `${ e.name }: ${ e.message }` + "\n" + e.stack);
    }
    else {
      log(LogSeverity.ERROR, "addColumnToTable", "Unknown error occurred.");
    }

    return {
      status: DatabaseErrors.CLIENT_ERROR
    };
  }
}

export { getColumnNames, addColumnToTable, alterTableColumn };
