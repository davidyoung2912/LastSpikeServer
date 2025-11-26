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

$baseUrl = "http://localhost:5098/api/sessionplayers"
$sessionsUrl = "http://localhost:5098/api/sessions"
$playersUrl = "http://localhost:5098/api/players"
$gameBoardsUrl = "http://localhost:5098/api/gameboards"

try {
    # 0. Setup: Create a Player, GameBoard, and Session
    Write-Host "Creating Setup Data..."
    
    # Create Player
    $playerBody = @{
        Name = "Test Player"
    } | ConvertTo-Json
    $player = Invoke-RestMethod -Uri $playersUrl -Method Post -Body $playerBody -ContentType "application/json"
    $playerId = $player.id
    Write-Host "Created Player: $playerId"

    # Create GameBoard (Needed for Session)
    $boardBody = @{
        Description = "Test Board for SessionPlayer"
        Data        = "Test Board Data"
    } | ConvertTo-Json
    $board = Invoke-RestMethod -Uri $gameBoardsUrl -Method Post -Body $boardBody -ContentType "application/json"
    $boardId = $board.id
    Write-Host "Created GameBoard: $boardId"

    # Create Session
    $sessionBody = @{
        Description = "Test Session for SessionPlayer"
        BoardId     = $boardId
        PlayerIds   = @()
    } | ConvertTo-Json
    $session = Invoke-RestMethod -Uri $sessionsUrl -Method Post -Body $sessionBody -ContentType "application/json"
    $sessionId = $session.id
    Write-Host "Created Session: $sessionId"


    # 1. Create SessionPlayer
    Write-Host "`nTesting Create SessionPlayer..."
    $spBody = @{
        SessionId = $sessionId
        PlayerId  = $playerId
        Data      = "Initial Player Data"
    } | ConvertTo-Json
    
    $sp = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $spBody -ContentType "application/json"
    $spId = $sp.id
    Write-Host "Created SessionPlayer: $spId"

    if ($sp.sessionId -eq $sessionId -and $sp.playerId -eq $playerId) {
        Write-Host "Create Verified."
    }
    else {
        Write-Error "Create Failed: ID mismatch."
    }

    # 2. Test Validation (Invalid SessionId)
    Write-Host "`nTesting Validation (Invalid SessionId)..."
    $invalidSessionBody = @{
        SessionId = [Guid]::NewGuid()
        PlayerId  = $playerId
        Data      = "Invalid Session"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $invalidSessionBody -ContentType "application/json"
        Write-Error "Should have failed with invalid SessionId"
    }
    catch {
        Write-Host "Correctly failed with invalid SessionId"
    }

    # 3. Test Validation (Invalid PlayerId)
    Write-Host "`nTesting Validation (Invalid PlayerId)..."
    $invalidPlayerBody = @{
        SessionId = $sessionId
        PlayerId  = [Guid]::NewGuid()
        Data      = "Invalid Player"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $invalidPlayerBody -ContentType "application/json"
        Write-Error "Should have failed with invalid PlayerId"
    }
    catch {
        Write-Host "Correctly failed with invalid PlayerId"
    }

    # 4. Get By Id
    Write-Host "`nTesting GetById..."
    $retrieved = Invoke-RestMethod -Uri "$baseUrl/$spId" -Method Get
    if ($retrieved.id -eq $spId) {
        Write-Host "GetById Verified."
    }
    else {
        Write-Error "GetById Failed."
    }

    # 5. Update
    Write-Host "`nTesting Update..."
    $retrieved.data = "Updated Player Data"
    $updateBody = $retrieved | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/$spId" -Method Put -Body $updateBody -ContentType "application/json"
    
    $updated = Invoke-RestMethod -Uri "$baseUrl/$spId" -Method Get
    if ($updated.data -eq "Updated Player Data") {
        Write-Host "Update Verified."
    }
    else {
        Write-Error "Update Failed."
    }

    # 6. Delete
    Write-Host "`nTesting Delete..."
    Invoke-RestMethod -Uri "$baseUrl/$spId" -Method Delete
    
    try {
        Invoke-RestMethod -Uri "$baseUrl/$spId" -Method Get
        Write-Error "Delete Failed: Item still exists."
    }
    catch {
        Write-Host "Delete Verified."
    }

}
catch {
    Write-Error "An error occurred: $_"
}
finally {
    # Cleanup: Delete created test data
    Write-Host "`nCleaning up test data..."
    try {
        if ($sessionId) {
            Invoke-RestMethod -Uri "$sessionsUrl/$sessionId" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted Session: $sessionId"
        }
        if ($playerId) {
            Invoke-RestMethod -Uri "$playersUrl/$playerId" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted Player: $playerId"
        }
        if ($boardId) {
            Invoke-RestMethod -Uri "$gameBoardsUrl/$boardId" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted GameBoard: $boardId"
        }
    }
    catch {
        Write-Host "Cleanup error (non-critical): $_"
    }
    
    Stop-Process -Id $process.Id -Force
    Write-Host "Application stopped."
}
