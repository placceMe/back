# PowerShell script to replace DTOs imports in OrdersServiceNet

$files = Get-ChildItem -Path "p:\Norsen\back\src\Services\OrdersServiceNet" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "using OrdersServiceNet\.DTOs;") {
        $newContent = $content -replace "using OrdersServiceNet\.DTOs;", "using Marketplace.Contracts.Orders;`nusing Marketplace.Contracts.Common;"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated: $($file.FullName)"
    }
}
