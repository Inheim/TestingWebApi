# TestingWebApi
Just some WebApi application, that I made as a test task for job.

For it to work properly, u should have PostgreSQL server
thats what I used:
* PostreSQL 17
* .Net 8.0
* Swagger 9.0.3
* EntityFrameworkCore.PostgreSQL 9.0.4
* CsvHelper 33.1.0

Commands that I used to make Tables in PostgreSQL server:

---------for Results table--------------------

CREATE TABLE Results (
Name VARCHAR(50) PRIMARY KEY,
Delta INTEGER,
Date TIMESTAMP,
AverageExecutionTime REAL,
averagevalue REAL,
medianvalue REAL,
maxvalue REAL,
minvalue REAL
)

---------for Values table--------------------

CREATE TABLE Values (
Id SERIAL PRIMARY KEY,
Date TIMESTAMP,
ExecutionTime INTEGER,
Value REAL
)

Check if everything right in connection string in program.cs and in Data.cs
