# SimpleApi

I am planning to use a YAML file to compose:
* webapi, which talks to redis
* redis

Reference: 
* https://github.com/imperugo/StackExchange.Redis.Extensions
* https://taswar.zeytinsoft.com/redis-for-net-developers-redis-with-aspnetcore-webapi/

```
dotnet add package StackExchange.Redis.Extensions.AspNetCore --version 9.1.0

dotnet add package tackExchange.Redis.Extensions.System.Text.Json --version 9.1.0
```

# Get Started
## Way 1: Docker
Firtly, we launch a local redis service in docker:
```
$docker run --name redis-server -p6379:6379 -d redis
```

Secondly, to make it be able to connect to local redis service, we change the `Docker` section in file `SimpleApi\Properties\launchSettings.json` with:
```
    ,"environmentVariables": {
    "Redis:Hosts:0:Host": "192.168.50.211"   
    }

# 192.168.50.211 here is my local IP address
```

Then we can click "Docker" in Debbuging mode and access: https://localhost:32783/swagger/index.html
* post/get  to get value from redis, like "k1"
* post/set  to set value to redis
* redis/connectionInfo

## Way 2: Docker Compose
Firstly, Right click "SimpleApi" project and set menu "Add..." to add "Container Orchestration Support...", hence new project "docker-compose" got created.

And we got the initial docker-compose.yml file like below:
```
version: '3.4'

services:
  simpleapi:
    image: ${DOCKER_REGISTRY-}simpleapi
    build:
      context: .
      dockerfile: SimpleApi/Dockerfile
```

Secondly, we need to edit the docker compose file to include redis, and add network, port mapping, environment variable like below:
```
version: '3.4'

networks:
  net_compose_1:
    name: net_simpleapi_redis
    ipam:
      driver: default
      config:
        - subnet: "172.30.0.0/16"
services:
  simpleapi:
    image: ${DOCKER_REGISTRY-}simpleapi
    build:
      context: .
      dockerfile: SimpleApi/Dockerfile
    ports:
      - "8080:80"
      - "8443:443"
    depends_on:
      - redis
    environment:
      "Redis__Hosts__0__Host": "redis"
    networks:
      - net_compose_1
  redis:
    image: redis
    networks:
      - net_compose_1
```

Lastly, launch the "docker-compose" project, and we can access following API to test the basic operation of redis in docker compose.
* post/get
* post/set
* redis/connectionInfo

Or we can use `docker compose up -d` command to run the compose:
```
cd SimpleApi

docker componse up -d
```
Then you can access the service via following URLs (I've already set up the mapping in docker-compose YAML file):
* http://localhost:8080/swagger
* https://localhost:8443/swagger
* http://localhost:8080/redis/connectionInfo

And then you can test the redis on swagger UI by:
* post/get
* post/set

The last step, we can use `docker componse down` command to destroy the containers.
