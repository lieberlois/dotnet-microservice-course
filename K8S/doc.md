# Ingress

## Deploy Ingress Nginx

```sh
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.0.0/deploy/static/provider/cloud/deploy.yaml
```

## List services

```sh
kubectl get namespace
kubectl get service --namespace ingress-nginx
```

## Documentation

[Getting started](https://kubernetes.github.io/ingress-nginx/deploy/) or [GitHub Repo](https://github.com/kubernetes/ingress-nginx)

# Secret

Creates a key (SA_PASSWORD) - value (Pa55w0rd!) pair as a secret/mssql.

```sh
kubectl create secret generic mssql --from-literal=SA_PASSWORD='Pa55w0rd!'
```