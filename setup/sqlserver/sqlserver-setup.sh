# Execution of sqlserver commands after startup
# A little bit flaky in GitHub, check first connection and wait if fails
# From windows, you may need this file to have LF endings instead of CRLF
echo "-- Try first connection"
if ! /opt/mssql-tools/bin/sqlcmd -S localhost,1433 -U sa -P $TEST_SQLSERVER_PWD -l 30 -Q "select name from sys.databases"; then
  echo "Failure to connect from sa, wait some time"
  sleep 4
  echo "End sleep, going to setup"
  #this seems to solve login failure, but sometimes: Error: Process completed with exit code 137.
fi
echo "-- Begin setup"
/opt/mssql-tools/bin/sqlcmd -S localhost,1433 -U sa -P $TEST_SQLSERVER_PWD -l 30  <<-EOSQL
  CREATE LOGIN qacoverdb WITH PASSWORD = '$TEST_SQLSERVER_PWD', CHECK_POLICY=OFF, CHECK_EXPIRATION=OFF, DEFAULT_LANGUAGE=spanish;
  GO
  CREATE DATABASE qacoverdb
  GO
  USE [qacoverdb]
  CREATE USER [qacoverdb] FOR LOGIN [qacoverdb] --WITH DEFAULT_SCHEMA=[dbo]
  EXEC sp_addrolemember 'db_owner', 'qacoverdb'
  GO
EOSQL
echo "-- END SETUP!"