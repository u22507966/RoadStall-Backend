# PowerShell script to publish to Azure
# Run this if Visual Studio publish fails

Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Check the errors above." -ForegroundColor Red
    exit 1
}

Write-Host "Publishing project..." -ForegroundColor Yellow
dotnet publish --configuration Release --output ./publish

Write-Host "Build successful! Publish folder is ready at: ./publish" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Go to Azure Portal -> Your App Service -> Deployment Center" -ForegroundColor White
Write-Host "2. Use 'Local Git' or 'FTP' to upload the ./publish folder" -ForegroundColor White
Write-Host "OR use Azure CLI:" -ForegroundColor White
Write-Host "az webapp deployment source config-zip --resource-group YOUR-RG --name YOUR-APP-NAME --src ./publish.zip" -ForegroundColor Gray
