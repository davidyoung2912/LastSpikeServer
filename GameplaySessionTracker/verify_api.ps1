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

$baseUrl = "http://localhost:5098/api/sessions"

# 1. Try Create without ID (Should Fail)
Write-Host "Testing Create without ID (Expect Failure)..."
$bodyNoId = @{
    Description = "Test Session No ID"
    PlayerIds   = @()
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri $baseUrl -Method Post -Body $bodyNoId -ContentType "application/json"
    Write-Error "Create without ID should have failed but succeeded."
}
catch {
    Write-Host "ID verified."
}
else {
    Write-Error "ID mismatch. Expected $newId but got $($response.id)"
}

# 3. Get the session
Write-Host "Getting session..."
$session = Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
Write-Host "Retrieved session: $($session.description) (BoardId: $($session.boardId))"

if ($session.boardId -eq $boardId.ToString()) {
    Write-Host "BoardId verified."
}
else {
    Write-Error "BoardId mismatch."
}

# 4. Update the session
Write-Host "Updating session..."
$session.description = "Updated Session"
$bodyUpdate = $session | ConvertTo-Json
Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Put -Body $bodyUpdate -ContentType "application/json"
Write-Host "Session updated."

# 5. Delete the session
Write-Host "Deleting session..."
Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Delete
Write-Host "Session deleted."

Stop-Process -Id $process.Id -Force
Write-Host "Application stopped."
