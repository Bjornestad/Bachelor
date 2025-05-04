# run.ps1
Write-Host "Running tests..." -ForegroundColor Cyan
dotnet test .\Test\Bachelor.Test\Bachelor.Test.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. App will not run." -ForegroundColor Red
    exit 1
}

Write-Host "Tests passed. Running app..." -ForegroundColor Green
dotnet run --project .\Bachelor.csproj