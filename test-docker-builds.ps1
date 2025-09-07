# PowerShell ?????? ??? ?????????? ?????? ???? ?????????????
Write-Host "?? Testing Docker builds for all microservices..." -ForegroundColor Cyan

# ??????? ??? ?????? ?? ?????????? ???????
function Build-Service {
    param(
        [string]$ServiceName,
        [string]$DockerfilePath
    )
    
    Write-Host ""
    Write-Host "?? Building $ServiceName..." -ForegroundColor Yellow
    Write-Host "?? Dockerfile: $DockerfilePath" -ForegroundColor Gray
    
    # ??????
    $buildResult = docker build -f $DockerfilePath -t "marketplace-$ServiceName`:test" . 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? $ServiceName built successfully" -ForegroundColor Green
        
        # ???????? test image
        docker rmi "marketplace-$ServiceName`:test" > $null 2>&1
        
        return $true
    } else {
        Write-Host "? $ServiceName build failed" -ForegroundColor Red
        Write-Host "Error output:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        return $false
    }
}

# ?????? ??? ??????????
$totalServices = 0
$successfulBuilds = 0
$failedServices = @()

# ?????? ???????? ??? ??????????
$services = @(
    @{Name="users-service"; Path="src/Services/UsersService/Dockerfile"},
    @{Name="products-service"; Path="src/Services/ProductsService/Dockerfile"},
    @{Name="files-service"; Path="src/Services/FilesService/Dockerfile"},
    @{Name="chat-service"; Path="src/Services/ChatService/Dockerfile"},
    @{Name="orders-service"; Path="src/Services/OrdersServiceNet/Dockerfile"},
    @{Name="notifications-service"; Path="src/Services/NotificationsService/Dockerfile"},
    @{Name="api-gateway"; Path="src/ApiGateway/Dockerfile"}
)

# ????????? ????????? Docker
try {
    docker --version > $null
} catch {
    Write-Host "? Docker is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# ?????????? ??????? ???????
foreach ($service in $services) {
    $totalServices++
    
    if (Build-Service -ServiceName $service.Name -DockerfilePath $service.Path) {
        $successfulBuilds++
    } else {
        $failedServices += $service.Name
    }
}

# ????????
Write-Host ""
Write-Host "?? Build Summary:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan
Write-Host "? Successful builds: $successfulBuilds/$totalServices" -ForegroundColor Green

if ($failedServices.Count -gt 0) {
    Write-Host "? Failed services:" -ForegroundColor Red
    foreach ($service in $failedServices) {
        Write-Host "   - $service" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "?? Tips for fixing build issues:" -ForegroundColor Yellow
    Write-Host "  1. Check Dockerfile paths and syntax" -ForegroundColor Gray
    Write-Host "  2. Ensure all project references are correct" -ForegroundColor Gray
    Write-Host "  3. Verify Marketplace.Contracts project exists" -ForegroundColor Gray
    Write-Host "  4. Run 'dotnet restore' in solution root" -ForegroundColor Gray
    Write-Host "  5. Check if all required files exist" -ForegroundColor Gray
    exit 1
} else {
    Write-Host "?? All services built successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? You can now run: docker-compose up --build" -ForegroundColor Cyan
}