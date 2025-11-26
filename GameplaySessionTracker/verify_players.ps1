# Kill any process listening on port 5098
$port = 5098
$tcpConnection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
if ($tcpConnection) {
    Write-Host "Killing process $($tcpConnection.OwningProcess) listening on port $port..."
    Stop-Process -Id $tcpConnection.OwningProcess -Force
    Start-Sleep -Seconds 2
}

$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "c:\AI_Games\LastSpikeServer\GameplaySessionTracker" -PassThru -NoNewWindow
Write-Host "Starting application..."
Start-Sleep -Seconds 10

$baseUrl = "http://localhost:5098/api/players"

# 1. Create a player
Write-Host "Creating player..."
$newId = [Guid]::NewGuid()
$body = @{
    Id    = $newId
    Name  = "Test Player"
    Alias = "TP"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $body -ContentType "application/json"
Write-Host "Created player with ID: $($response.id)"

# 2. Get the player
Write-Host "Getting player..."
$player = Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
Write-Host "Retrieved player: $($player.name) ($($player.alias))"

# 3. Update the player
Write-Host "Updating player..."
$player.name = "Updated Player"
$bodyUpdate = $player | ConvertTo-Json
Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Put -Body $bodyUpdate -ContentType "application/json"
Write-Host "Player updated."

# 4. Verify update
$updatedPlayer = Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
if ($updatedPlayer.name -eq "Updated Player") {
    Write-Host "Update verified."
}
else {
    Write-Error "Update failed."
}

# 5. Delete the player
Write-Host "Deleting player..."
Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Delete
Write-Host "Player deleted."

# 6. Verify delete
try {
    Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
    Write-Error "Delete failed. Player still exists."
}
catch {
    Write-Host "Delete verified. Player not found."
}

Stop-Process -Id $process.Id -Force
Write-Host "Application stopped."
