# PowerShell script to replace DTOs imports in UsersService

$files = Get-ChildItem -Path "p:\Norsen\back\src\Services\UsersService" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Replace old DTO names with new ones
    $replacements = @{
        "SalerInfoResponseDto" = "SellerInfoResponseDto"
        "CreateSalerInfoDto" = "CreateSellerInfoDto" 
        "UpdateSalerInfoDto" = "UpdateSellerInfoDto"
        "GetSalerInfoByIdsRequest" = "GetSellerInfoByIdsRequest"
    }
    
    $updated = $false
    foreach ($old in $replacements.Keys) {
        $new = $replacements[$old]
        if ($content -match $old) {
            $content = $content -replace $old, $new
            $updated = $true
        }
    }
    
    if ($updated) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated DTO names in: $($file.FullName)"
    }
}
