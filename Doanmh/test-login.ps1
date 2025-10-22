# Test login và lấy token
try {
    $loginData = @{
        username = "admin"
        password = "admin123"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "https://localhost:7218/api/auth/login" -Method Post -ContentType "application/json" -Body $loginData
    
    Write-Host "=== LOGIN SUCCESSFUL ==="
    Write-Host "Token: $($response.token)"
    Write-Host "User Role: $($response.user.role)"
    Write-Host "Username: $($response.user.username)"
    Write-Host ""
    Write-Host "Copy token này và paste vào Swagger UI Authorize button"
    Write-Host "Format: Bearer $($response.token)"
}
catch {
    Write-Host "Login failed: $($_.Exception.Message)"
}
