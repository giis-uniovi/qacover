# Execution of postgres commands at startup
# note the first line specifying the shell is removed to avoid problems with line breaks in windows
echo "-- Begin setup"
psql -v ON_ERROR_STOP=1   <<-EOSQL
  CREATE USER qacoverdb with encrypted password '$TEST_POSTGRES_PWD';
  CREATE DATABASE qacoverdb;
EOSQL
echo "-- END SETUP!"