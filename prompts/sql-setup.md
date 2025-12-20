There are several TSQL scripts located in the @/database folder.

### SQL Scripts Overview

The `@/database` folder contains several TSQL scripts that are essential for setting up and maintaining the database schema for the Volleyball Rally Manager application. These scripts include:

1. **setup.sql**: This script is responsible for creating the initial database schema, including tables, stored procedures, and other database objects. It should be run first to establish the foundation of the database.

2. ***.sql**: Additional SQL scripts that may include specific database operations, such as data migrations, seed data insertion, or complex queries used by the application. THe other scripts may also include migrations for schema changes since the intial setup. Each script is named according to its purpose.



### Synchronization with Entity Models

To ensure that the SQL setup files are in sync with the project's database schema, follow these steps:

1. **Review Entity Models**: Examine the entity models defined in the lib project to understand the expected database schema. These models should reflect the current state of the application's data requirements.

2. **Compare with SQL Scripts**: Compare the entity models with the SQL scripts in the `@/database` folder. Ensure that all tables, columns, relationships, and constraints defined in the entity models are accurately represented in the SQL scripts.

3. **Update SQL Scripts**: If there are discrepancies between the entity models and the SQL scripts, update the SQL scripts to match the entity models. This may involve adding new tables, modifying existing tables, or adding constraints.

4. **Review SQL Scripts**: Review the SQL scripts in the `@/database` folder to ensure they are up-to-date and correctly reflect the current state of the database schema. This includes checking for any missing tables, columns, or relationships that are defined in the entity models but not in the SQL scripts.


By following these steps, you can ensure that the SQL setup files are in sync with the project's database schema, maintaining consistency between the application's data models and the underlying database.

Check the setup.sql and update file, look at the other *.sql files and the entity models and ensure the sql setup files can update and in sync with the project databse schema.

Create a single SQL script that combines the setup.sql and any other .sql files, ensuring the database schema is up-to-date and synchronized with the entity models.

Update cleanup.sql so that it can be used to reset teh database.

GIve me a list of files that can be removed because there are no longer relevant.


okay, update the README.md and SCHEMA_SYNC_REPORT.md files in the database folder.