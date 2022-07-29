#!/bin/bash
SLAVE_SYNC_USER="${SLAVE_SYNC_USER:-sync_admin}"
SLAVE_SYNC_PASSWORD="${SLAVE_SYNC_PASSWORD:-sync_admin}"
ADMIN_USER="${ADMIN_USER:-root}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-qwer1234}"
MASTER_HOST="${MASTER_HOST:-%}"
MASTER_PORT="${MASTER_PORT:-%}"
sleep 10

RESULT=`mysql -u"$SLAVE_SYNC_USER" -h$MASTER_HOST -P$MASTER_PORT  -p"$SLAVE_SYNC_PASSWORD" -e "SHOW MASTER STATUS;" | grep -v grep |tail -n +2| awk '{print $1,$2}'`
LOG_FILE_NAME=`echo $RESULT | grep -v grep | awk '{print $1}'`
LOG_FILE_POS=`echo $RESULT | grep -v grep | awk '{print $2}'`
SYNC_SQL="change master to master_host='$MASTER_HOST', master_port=$MASTER_PORT, master_user='$SLAVE_SYNC_USER',master_password='$SLAVE_SYNC_PASSWORD',master_log_file='$LOG_FILE_NAME',master_log_pos=$LOG_FILE_POS;"
START_SYNC_SQL="start slave;"
STATUS_SQL="show slave status\G;"
mysql -u"$ADMIN_USER" -p"$ADMIN_PASSWORD" -e "$SYNC_SQL $START_SYNC_SQL $STATUS_SQL"