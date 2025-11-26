#!/usr/bin/env pwsh
# PowerShell script to run unit tests with code coverage

Write-Host "Running unit tests with code coverage..." -ForegroundColor Cyan

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Find the most recent coverage file
$coverageFile = Get-ChildItem -Path "./TestResults" -Recurse -Filter "coverage.cobertura.xml" | 
Sort-Object LastWriteTime -Descending | 
Select-Object -First 1

if ($null -eq $coverageFile) {
    Write-Host "No coverage file found" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nCoverage file: $($coverageFile.FullName)" -ForegroundColor Green

# Parse and display coverage summary
[xml]$coverage = Get-Content $coverageFile.FullName

$lineRate = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2)
$branchRate = [math]::Round([double]$coverage.coverage.'branch-rate' * 100, 2)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "CODE COVERAGE SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Line Coverage:   $lineRate%" -ForegroundColor $(if ($lineRate -ge 80) { "Green" } elseif ($lineRate -ge 60) { "Yellow" } else { "Red" })
Write-Host "Branch Coverage: $branchRate%" -ForegroundColor $(if ($branchRate -ge 80) { "Green" } elseif ($branchRate -ge 60) { "Yellow" } else { "Red" })
Write-Host "========================================`n" -ForegroundColor Cyan

# Optional: Generate HTML report using ReportGenerator (if installed)
$reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue
if ($reportGenerator) {
    Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan
    reportgenerator "-reports:$($coverageFile.FullName)" "-targetdir:./CoverageReport" "-reporttypes:Html"
    Write-Host "HTML report generated at: ./CoverageReport/index.html" -ForegroundColor Green
}
else {
    Write-Host "Tip: Install ReportGenerator for HTML reports: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
}

Write-Host "`nAll tests passed!" -ForegroundColor Green
