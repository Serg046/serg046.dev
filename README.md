# serg046.dev
A personal "NoJS" web page written in C#/Blazor deployed via werf/k3s

```code
kubectl create secret docker-registry registrysecret --docker-server=ghcr.io --docker-username=Serg046 --docker-password=GENERATED_PAT_TOKEN --docker-email=serg046@outlook.com -n serg046-app-production
# Use the latest release 
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.1/cert-manager.yaml
sudo ufw allow 80
sudo ufw allow 443
```
