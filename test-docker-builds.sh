#!/bin/bash

# ?????? ??? ?????????? ?????? ???? ?????????????
echo "?? Testing Docker builds for all microservices..."

# ??????? ??? ?????? ?? ?????????? ???????
build_service() {
    local service_name=$1
    local dockerfile_path=$2
    
    echo ""
    echo "?? Building $service_name..."
    echo "?? Dockerfile: $dockerfile_path"
    
    # ??????
    if docker build -f "$dockerfile_path" -t "marketplace-$service_name:test" .; then
        echo "? $service_name built successfully"
        
        # ???????? test image
        docker rmi "marketplace-$service_name:test" > /dev/null 2>&1
        
        return 0
    else
        echo "? $service_name build failed"
        return 1
    fi
}

# ?????? ??? ??????????
total_services=0
successful_builds=0
failed_services=()

# ?????? ???????? ??? ??????????
services=(
    "users-service:src/Services/UsersService/Dockerfile"
    "products-service:src/Services/ProductsService/Dockerfile"
    "files-service:src/Services/FilesService/Dockerfile"
    "chat-service:src/Services/ChatService/Dockerfile"
    "orders-service:src/Services/OrdersServiceNet/Dockerfile"
    "notifications-service:src/Services/NotificationsService/Dockerfile"
    "api-gateway:src/ApiGateway/Dockerfile"
)

# ?????????? ??????? ???????
for service_config in "${services[@]}"; do
    IFS=':' read -r service_name dockerfile_path <<< "$service_config"
    
    total_services=$((total_services + 1))
    
    if build_service "$service_name" "$dockerfile_path"; then
        successful_builds=$((successful_builds + 1))
    else
        failed_services+=("$service_name")
    fi
done

# ????????
echo ""
echo "?? Build Summary:"
echo "=================="
echo "? Successful builds: $successful_builds/$total_services"

if [ ${#failed_services[@]} -gt 0 ]; then
    echo "? Failed services:"
    for service in "${failed_services[@]}"; do
        echo "   - $service"
    done
    echo ""
    echo "?? Tips for fixing build issues:"
    echo "  1. Check Dockerfile paths and syntax"
    echo "  2. Ensure all project references are correct"
    echo "  3. Verify Marketplace.Contracts project exists"
    echo "  4. Run 'dotnet restore' in solution root"
    exit 1
else
    echo "?? All services built successfully!"
    echo ""
    echo "?? You can now run: docker-compose up --build"
fi