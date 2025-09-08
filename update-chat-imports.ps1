# PowerShell script to replace DTOs imports in ChatService

$files = Get-ChildItem -Path "p:\Norsen\back\src\Services\ChatService" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "using ChatService\.DTOs;") {
        $newContent = $content -replace "using ChatService\.DTOs;", "using Marketplace.Contracts.Chat;`nusing Marketplace.Contracts.Common;"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated: $($file.FullName)"
    }
}
