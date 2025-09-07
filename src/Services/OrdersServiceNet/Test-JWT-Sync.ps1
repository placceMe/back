# JWT Sync Test Script
# ??? ?????? ?????? ????????????? JWT ??????? ??? UsersService ?? OrdersService

Write-Host "?? Testing JWT synchronization between services..." -ForegroundColor Green

# Test URLs
$usersServiceUrl = "http://localhost:5002"
$ordersServiceUrl = "http://localhost:5004"

# Test credentials
$loginData = @{
    email = "user@example.com"
    password = "password123"
} | ConvertTo-Json

Write-Host "`n1. Testing health endpoints..." -ForegroundColor Yellow

try {
    $usersHealth = Invoke-RestMethod -Uri "$usersServiceUrl/Health" -Method GET
    Write-Host "? UsersService: $($usersHealth.status)" -ForegroundColor Green
} catch {
    Write-Host "? UsersService health failed: $($_.Exception.Message)" -ForegroundColor Red
}

try {
    $ordersHealth = Invoke-RestMethod -Uri "$ordersServiceUrl/Health" -Method GET  
    Write-Host "? OrdersService: $($ordersHealth.status)" -ForegroundColor Green
} catch {
    Write-Host "? OrdersService health failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n2. Getting JWT token from UsersService..." -ForegroundColor Yellow

try {
    $loginResponse = Invoke-RestMethod -Uri "$usersServiceUrl/api/auth/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body $loginData
    
    if ($loginResponse.success -and $loginResponse.accessToken) {
        $token = $loginResponse.accessToken
        $userId = $loginResponse.user.id
        Write-Host "? Login successful!" -ForegroundColor Green
        Write-Host "   User ID: $userId" -ForegroundColor Cyan
        Write-Host "   Token (first 50 chars): $($token.Substring(0, [Math]::Min(50, $token.Length)))..." -ForegroundColor Cyan
    } else {
        Write-Host "? Login failed: $($loginResponse.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "? Login request failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n3. Testing token validation in UsersService..." -ForegroundColor Yellow

try {
    $authHeaders = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $meResponse = Invoke-RestMethod -Uri "$usersServiceUrl/api/auth/me" -Method GET -Headers $authHeaders
    Write-Host "? Token valid in UsersService!" -ForegroundColor Green
    Write-Host "   User: $($meResponse.name) ($($meResponse.email))" -ForegroundColor Cyan
} catch {
    Write-Host "? Token validation failed in UsersService: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n4. Testing token in OrdersService..." -ForegroundColor Yellow

try {
    $myOrdersResponse = Invoke-RestMethod -Uri "$ordersServiceUrl/api/orders/my" -Method GET -Headers $authHeaders
    Write-Host "? Token valid in OrdersService!" -ForegroundColor Green
    Write-Host "   Orders count: $($myOrdersResponse.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "? Token validation failed in OrdersService!" -ForegroundColor Red
    Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host "   Error: $($_.ErrorDetails.Message)" -ForegroundColor Red
    
    # Try to get more details
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "`n?? JWT Configuration Mismatch Detected!" -ForegroundColor Yellow
        Write-Host "   This means the JWT secret keys or configuration don't match between services." -ForegroundColor Yellow
        Write-Host "   Check appsettings.json in both services for:" -ForegroundColor Yellow
        Write-Host "   - Jwt:SecretKey (must be identical)" -ForegroundColor Yellow
        Write-Host "   - Jwt:Issuer (must be identical)" -ForegroundColor Yellow
        Write-Host "   - Jwt:Audience (must be identical)" -ForegroundColor Yellow
    }
}

Write-Host "`n5. Testing order creation..." -ForegroundColor Yellow

$testOrder = @{
    userId = $userId
    notes = "Test order from PowerShell script"
    deliveryAddress = "Test Address 123, Test City"
    items = @(
        @{
            productId = "123e4567-e89b-12d3-a456-426614174001"
            quantity = 1
            price = 99.99
        }
    )
} | ConvertTo-Json

try {
    $createOrderResponse = Invoke-RestMethod -Uri "$ordersServiceUrl/api/orders" -Method POST -Headers $authHeaders -Body $testOrder
    Write-Host "? Order created successfully!" -ForegroundColor Green
    Write-Host "   Order ID: $($createOrderResponse.id)" -ForegroundColor Cyan
    Write-Host "   Total: $($createOrderResponse.totalAmount)" -ForegroundColor Cyan
} catch {
    Write-Host "? Order creation failed!" -ForegroundColor Red
    Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host "   Error: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

Write-Host "`n?? JWT synchronization test completed!" -ForegroundColor Green
Write-Host "Check the results above to see if JWT tokens work between services." -ForegroundColor Yellow