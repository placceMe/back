docker-compose --env-file .env.dev up -d --build
docker-compose restart nginx-gateway
