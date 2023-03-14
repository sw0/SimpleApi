# SimpleApi

I am planning to use a YAML file to compose:
* webapi, which talks to redis
* redis

Reference: 
* https://kubernetes.io/docs/reference/kubectl/cheatsheet/
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
    "environmentVariables": {
    "Redis:Hosts:0:Host": "192.168.50.211"   
    }

# 192.168.50.211 here is my local IP address
```

Then we can click "Docker" in Debbuging mode and access: https://localhost:32783/swagger/index.html
* post/get  to get value from redis, like "k1"
* post/set  to set value to redis
* ping
* redis/connectionInfo

## Way 2: Docker Compose
Firstly, Right click "SimpleApi" project and set menu "Add..." to add "Container Orchestration Support...", hence new project "docker-compose" got created.

And we got the initial docker-compose.yml file like below:
```yaml
version: '3.4'

services:
  simpleapi:
    image: ${DOCKER_REGISTRY-}simpleapi
    build:
      context: .
      dockerfile: SimpleApi/Dockerfile
```

Secondly, we need to edit the docker compose file to include redis, and add network, port mapping, environment variable like below:
```yaml
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
* ping
* redis/connectionInfo

Or we can use `docker compose up -d` command to run the compose:
```
cd SimpleApi

docker componse up -d
```
Then you can access the service via following URLs (I've already set up the mapping in docker-compose YAML file):
* http://localhost:8080/swagger
* https://localhost:8443/swagger
* http://localhost:8080/ping
* http://localhost:8080/redis/connectionInfo

And then you can test the redis on swagger UI by:
* post/get
* post/set
* ping

The last step, we can use `docker componse down` command to destroy the containers.
docker run --hostname=542b948e942b --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env=ASPNETCORE_URLS=http://+:80 --env=DOTNET_RUNNING_IN_CONTAINER=true --env=DOTNET_VERSION=6.0.14 --env=ASPNET_VERSION=6.0.14 --workdir=/app --label='com.microsoft.created-by=visual-studio' --label='com.microsoft.visual-studio.project-name=SimpleApi' --runtime=runc -d simpleapi:dev

## Way 3: Using Azure Kubernetes Service
For this part, please refer to next section.

# Azure Kubernetes Service
## preparation on Azure
1. Azure Container Registry

Reference: https://learn.microsoft.com/en-us/azure/container-registry/container-registry-get-started-azure-cli
```bash
az group create --name myResourceGroup --location eastus

az acr create --resource-group myResourceGroup \
  --name mycontainerregistry --sku Basic
```
2. Azure Kubernetes Service

2.1. Login and create resource group
```bash
az login

$INSTANCE_ID="<your-initials>"
$AKS_RESOURCE_GROUP="azure-$INSTANCE_ID-rg"
$LOCATION="<region>"
$AKS_IDENTITY="identity-$INSTANCE_ID"


az vm list-sizes --location $LOCATION `
 --query "[?numberOfCores == ``2``].{Name:name}" -o table


 $VM_SKU="Standard_D2as_v5"


 az group create --location $LOCATION `
 --resource-group $AKS_RESOURCE_GROUP
```


2.2. Create kubernetes Cluster
```
$AKS_NAME="aks-$INSTANCE_ID"
Write-Host "AKS Cluster Name: $AKS_NAME"


az aks create --node-count 2 ` 
--generate-ssh-keys ` 
--node-vm-size $VM_SKU ` 
--name $AKS_NAME `
--resource-group $AKS_RESOURCE_GROUP


az aks get-credentials --name $AKS_NAME ` 
--resource-group $AKS_RESOURCE_GROUP

```

2.3.  Get nodes
```
kubectl get nodes 
```

## Preparation in local
1. build image
```
# NOTE: locate solution folder
cd SimpleApi

# build the image
docker build -f .\SimpleApi\Dockerfile . -t mydev/simpleapi:0.3

```

2. upload image to ACR

Here I use Azure CLI locally, and my ACR name is `dev230312` as demo purpose, please replace it with yours.
```
# login
azure acr login --name dev230312

# list repositories
az acr repository list --name dev230312 -o table

# tag the image
docker tag mydev/simpleapi:0.3 dev2303.azurecr.io/simpleapi:0.3

# push the image
docker push dev2303.azurecr.io/simpleapi:0.3
```

And here is an important step to attach ACR to AKS, so AKS can pull the images from ACR:
```
az aks update -n myAKSCluster -g myResourceGroup --attach-acr $MYACR
```

3. create YAML files for namespace, deployment and service

3.1. Namespace : simpleapi-ns.yml
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: simpleapi
spec:
  finalizers:
    - kubernetes
```

3.2. redis deployment: simpleapi-redis-dep.yml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: simpleapi-redis
  namespace: simpleapi
spec:
  replicas: 1
  selector:
    matchLabels:
      app: simpleapi-redis
  template:
    metadata:
      labels:
        app: simpleapi-redis
    spec:
      nodeSelector:
        kubernetes.io/os: linux
      containers:
        - name: redis
          image: redis
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
            limits:
              cpu: 250m
              memory: 256Mi
          ports:
            - containerPort: 6379
              name: redis
          env:
            - name: ALLOW_EMPTY_PASSWORD
              value: 'yes'
```

3.3. redis service: simpleapi-redis-svc.yml

```yaml
apiVersion: v1
kind: Service
metadata:
  name: simpleapi-redis
  namespace: simpleapi
spec:
  ports:
    - port: 6379
  selector:
    app: simpleapi-redis
```

3.4. API deployment: simpleapi-api-dep.yml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: simpleapi-api
  namespace: simpleapi
spec:
  replicas: 1
  selector:
    matchLabels:
      app: simpleapi
  template:
    metadata:
      labels:
        app: simpleapi
        kind: webapi
    spec:
      nodeSelector:
        kubernetes.io/os: linux
      containers:
        - name: simpleapi
          image: slindev.azurecr.io/simpleapi:0.2
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
            limits:
              cpu: 250m
              memory: 256Mi
          ports:
            - containerPort: 80
              protocol: TCP
          env:
            - name: REDIS__HOSTS__0__HOST
              value: simpleapi-redis
```

3.5. API service with type of `LoadBalancer` in `simpleapi-api-svc.yml`

```yaml
apiVersion: v1
kind: Service
metadata:
  name: simpleapi-api
  namespace: simpleapi
spec:
  ports:
    - port: 80
  selector:
    kind: webapi
  type: LoadBalancer
```

3.6. Execute deployment and create service
```bash

kubectl apply -f simpleapi-ns.yml

kubectl apply -f simpleapi-api-dep.yml

kubectl apply -f simpleapi-api-svc.yml

kubectl apply -f simpleapi-redis-dep.yml

kubectl apply -f simpleapi-redis-svc.yml
```
Now you can find the application got deployed into AKS. And you can find the public IP address after click "Services and ingresses" in left navigation blade.
Then you can open a browser and access "http://IP-ADDRESS/swagger" or "http://IP-ADDRESS/ping".

Alternatively, I've combine all these yml file content together, so you can just run `kubectl apply -f ./simpleapi.yml`, which will also make it.

From ~/ping, you can find the IP addresses, so if you get simpleapi-api-dep.yml with replicaset set to 2 or more, you can find the ip address will be changed if you refresh it or open a new browser to open it.

4. Clean up resources
```bash
# you can delete namespace, which will delete related resources for you
 kubectl delete namespace simpleapi
```
OR you can delete deployment and services separately:
```bash
kubectl delete deployment simpleapi-api -n simpleapi
kubectl delete service simpleapi-api -n simpleapi

kubectl delete deployment simpleapi-redis -n simpleapi
kubectl delete service simpleapi-redis -n simpleapi

# OR we can just use 
kubectl delete -f .
```

**NOTE**
  All these steps can be exected locally if you got Kubernetes enabled in docker desktop. And in locally, we need to use `kubectl port-forwrad POD 5000:80` to forward the port, otherwise, we cannot access the service deployed locally.

## Commands
For full command list for `kubectl`, you can check: https://kubernetes.io/docs/reference/kubectl/cheatsheet/

Here we can use `kubectl config set-context --current --namespace simpleapi` to set the default namespace.

5. troubleshootings
  
  5.1. use `kubectl logs POD -n NAMESPACE` to check the log

  5.2. use `kubectl exec -it POD -c CONTAINER -- COMMAND` in pod

  ```bash
  kubectl exec -it simpleapi-pod -c simpleapi -- bash
  ```

  5.3. Install ping
```bash
# update
apt-get update

# install curl and iputils-ping
apt-get install -y curl iputils-ping
```