FROM mysql:8
COPY my.cnf /etc/mysql/conf.d
COPY create_sync_user.sh /docker-entrypoint-initdb.d/
CMD ["mysqld"]