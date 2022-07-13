# NetDemo

#### 介绍
net学习资料

#### 软件架构
主要是个人学习和一些知识点的笔记使用

#### 项目层级介绍：



 1.Bussiness:业务逻辑，主要存放业务操作，数据库访问

      1.1 MicroService.Interface：抽象业务逻辑层

      1.2 MicroService.Service: 主要存放业务逻辑操作

      1.3 MicroService.Model：实体层（主要存放数据库实体，业务实体dto）

 
 2.Client:客户端，主要是存放前端页面

      2.1 MicroService.Client 主要存放前端请求和前端页面
 
 3.Common：存放公共类库

      3.1 MicroService.Core：存放项目的一些公共类，和一些三方插件的扩展
 
 4.Microservice：微服务层

    4.1 MicroService.GrpcAPI grpc接口api
  
    4.2 MicroService.MinimalAPI 后端接口api层

#### 使用说明

1.  本地启动consul：consul agent - dev

2.  启动后端接口api命令：dotnet run --urls=http://localhost:4399 --ConsulRegisterOptions:Port=4399 --ConsulRegisterOptions:HealthCheckUrl=http://localhost:4399/Api/Health/Index

3.  启动后端grp api命令：dotnet run --urls=http://localhost:5018 --ConsulRegisterOptions:Port=5018 --ConsulRegisterOptions:HealthCheckUrl=localhost:5018
