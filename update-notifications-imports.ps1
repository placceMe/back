# PowerShell script to replace DTOs imports in NotificationsService

$files = Get-ChildItem -Path "p:\Norsen\back\src\Services\NotificationsService" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "using NotificationsService\.Contracts;") {
        $newContent = $content -replace "using NotificationsService\.Contracts;", "using Marketplace.Contracts.Notifications;"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated: $($file.FullName)"
    }
}
