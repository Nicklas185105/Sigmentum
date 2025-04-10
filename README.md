# Sigmentum

## Deployment

1. Build the Docker image
```bash
docker build -f "Sigmentum/Dockerfile" -t sigmentum .
```

2. Push the Docker image to DigitalOcean Container Registry
```bash
doctl registry login
```

```bash
docker tag sigmentum registry.digitalocean.com/tfwo-container-registry/sigmentum
```

```bash
docker push registry.digitalocean.com/tfwo-container-registry/sigmentum
```

3. Clean up the old image
```bash
docker image prune -f
```

4. Run this script to redeploy the application on the server
```bash
./deploy.sh
```