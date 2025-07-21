# Add this location block to nginx.conf for OrdersServiceNet routing:

location /api/orders-net/ {
    proxy_pass http://orders-service-net:80/api/orders/;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}