解压，在plugins中新建sql文件夹，然后把文件复制进去，然后重新启动es

 
docker 启动需要挂载/plugins/sql  文件夹

docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" -v /home/es/sql:/usr/share/elasticsearch/plugins/sql  -e ES_JAVA_OPTS="-Xms100m -Xmx200m"   docker.elastic.co/elasticsearch/elasticsearch:7.6.1