> 当前项目只为了学习使用，搭建的一个微服务项目。项目中orm框架用的sqlsugar



#### nacos

> docker run -d -p 8848:8848  -p 9848:9848 -p 9849:9849 \
> -e MODE=standalone \
> -e PREFER_HOST_MODE=192.168.5.150 \
> -e SPRING_DATASOURCE_PLATFORM=mysql \
> -e MYSQL_SERVICE_HOST=192.168.5.150 \
> -e MYSQL_SERVICE_PORT=3306 \
> -e MYSQL_SERVICE_USER=root \
> -e MYSQL_SERVICE_PASSWORD=123456 \
> -e MYSQL_SERVICE_DB_NAME=nacos-config \
> --name nacos \



#### ELK

docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" -v /home/es/sql:/usr/share/elasticsearch/plugins/sql  -e ES_JAVA_OPTS="-Xms512m -Xmx512m"   docker.elastic.co/elasticsearch/elasticsearch:7.6.1

docker run -p 5601:5601 -d -e ELASTICSEARCH_URL=http://192.168.5.150:9200   -e ELASTICSEARCH_HOSTS=http://192.168.5.150:9200 kibana:7.6.1  

docker run --rm -it --privileged=true -p 9600:9600  -d -v /home/es/logstash/logstash.conf:/usr/share/logstash/pipeline/logstash.conf -v /home/es/logstash/log/:/home/public/  -v /home/es/logstash/logstash.yml:/usr/share/logstash/config/logstash.yml logstash:7.6.1