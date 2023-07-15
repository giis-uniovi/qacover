# Execution of postgres commands at startup
# note the first line specifying the shell is removed to avoid problems with line breaks in windows
echo "-- Begin setup"
sqlplus system/${TEST_ORACLE_PWD}@XE  <<-EOSQL
  create user qacoverdb identified by "$TEST_ORACLE_PWD" ACCOUNT UNLOCK;
  grant connect to qacoverdb;
  grant create session to qacoverdb;
  grant resource to qacoverdb;
  grant create table to qacoverdb; 
  grant create procedure to qacoverdb; 
  grant create view to qacoverdb;
EOSQL
echo "-- End setup"
