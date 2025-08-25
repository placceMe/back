docker-compose --env-file .env.prod up -d --build
docker-compose restart nginx-gateway
