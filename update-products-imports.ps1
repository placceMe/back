# PowerShell script to replace DTOs imports in ProductsService

$files = Get-ChildItem -Path "p:\Norsen\back\src\Services\ProductsService" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "using ProductsService\.DTOs;") {
        $newContent = $content -replace "using ProductsService\.DTOs;", "using Marketplace.Contracts.Products;`nusing Marketplace.Contracts.Files;`nusing Marketplace.Contracts.Common;"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated: $($file.FullName)"
    }
}
