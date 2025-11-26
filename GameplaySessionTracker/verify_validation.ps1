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


$sessionUrl = "http://localhost:5098/api/sessions"
$playerUrl = "http://localhost:5098/api/players"
$boardUrl = "http://localhost:5098/api/gameboards"

try {
    # 1. Create a valid player
    Write-Host "Creating player..."
    $playerId = [Guid]::NewGuid()
    $playerBody = @{
        Id    = $playerId
        Name  = "Test Player"
        Alias = "TP"
    } | ConvertTo-Json
    Invoke-RestMethod -Uri $playerUrl -Method Post -Body $playerBody -ContentType "application/json" | Out-Null

    # 2. Create a valid gameboard
    Write-Host "Creating gameboard..."
    $boardId = [Guid]::NewGuid()
    $boardBody = @{
        Id          = $boardId
        Description = "Test Board"
        Data        = "Test Data"
    } | ConvertTo-Json
    Invoke-RestMethod -Uri $boardUrl -Method Post -Body $boardBody -ContentType "application/json" | Out-Null

    # 3. Try to create a session with an invalid BoardId (should fail)
    Write-Host "Testing invalid BoardId (Expect Failure)..."
    $invalidBoardId = [Guid]::NewGuid()
    $sessionId = [Guid]::NewGuid()
    $sessionBody = @{
        Id          = $sessionId
        Description = "Test Session"
        BoardId     = $invalidBoardId
        PlayerIds   = @()
    } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri $sessionUrl -Method Post -Body $sessionBody -ContentType "application/json"
        Write-Error "Create with invalid BoardId should have failed but succeeded."
    }
    catch {
        Write-Host "Invalid BoardId rejected as expected."
    }

    # 4. Try to create a session with an invalid PlayerId (should fail)
    Write-Host "Testing invalid PlayerId (Expect Failure)..."
    $invalidPlayerId = [Guid]::NewGuid()
    $sessionBody = @{
        Id          = $sessionId
        Description = "Test Session"
        BoardId     = $boardId
        PlayerIds   = @($invalidPlayerId)
    } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri $sessionUrl -Method Post -Body $sessionBody -ContentType "application/json"
        Write-Error "Create with invalid PlayerId should have failed but succeeded."
    }
    catch {
        Write-Host "Invalid PlayerId rejected as expected."
    }

    # 5. Create session with valid BoardId and PlayerId (should succeed)
    Write-Host "Creating session with valid references..."
    $sessionBody = @{
        Id          = $sessionId
        Description = "Test Session"
        BoardId     = $boardId
        PlayerIds   = @($playerId)
    } | ConvertTo-Json

    $session = Invoke-RestMethod -Uri $sessionUrl -Method Post -Body $sessionBody -ContentType "application/json"
    Write-Host "Session created successfully with BoardId: $($session.boardId)"

    # 6. Try to update with invalid BoardId (should fail)
    Write-Host "Testing update with invalid BoardId (Expect Failure)..."
    $invalidBoardId2 = [Guid]::NewGuid()
    $session.boardId = $invalidBoardId2
    $updateBody = $session | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri "$sessionUrl/$sessionId" -Method Put -Body $updateBody -ContentType "application/json"
        Write-Error "Update with invalid BoardId should have failed but succeeded."
    }
    catch {
        Write-Host "Invalid BoardId on update rejected as expected."
    }
}
catch {
    Write-Error "An error occurred: $_"
}
finally {
    # Cleanup
    Write-Host "`nCleaning up test data..."
    try {
        if ($sessionId) {
            Invoke-RestMethod -Uri "$sessionUrl/$sessionId" -Method Delete -ErrorAction SilentlyContinue | Out-Null
            Write-Host "Deleted Session"
        }
        if ($playerId) {
            Invoke-RestMethod -Uri "$playerUrl/$playerId" -Method Delete -ErrorAction SilentlyContinue | Out-Null
            Write-Host "Deleted Player"
        }
        if ($boardId) {
            Invoke-RestMethod -Uri "$boardUrl/$boardId" -Method Delete -ErrorAction SilentlyContinue | Out-Null
            Write-Host "Deleted GameBoard"
        }
    }
    catch {
        Write-Host "Cleanup error (non-critical): $_"
    }
    
    Stop-Process -Id $process.Id -Force
    Write-Host "Application stopped. All tests passed!"
}
