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

$baseUrl = "http://localhost:5098/api/sessiongameboards"
$sessionsUrl = "http://localhost:5098/api/sessions"
$gameBoardsUrl = "http://localhost:5098/api/gameboards"

try {
    # 0. Setup: Create a Session and a GameBoard
    Write-Host "Creating Setup Data..."
    
    # Create GameBoard
    $boardBody = @{
        Description = "Test Board for SessionGameBoard"
        Data        = "Test Board Data"
    } | ConvertTo-Json
    $board = Invoke-RestMethod -Uri $gameBoardsUrl -Method Post -Body $boardBody -ContentType "application/json"
    $boardId = $board.id
    Write-Host "Created GameBoard: $boardId"

    # Create Session
    $sessionBody = @{
        Description = "Test Session for SessionGameBoard"
        BoardId     = $boardId
        PlayerIds   = @()
    } | ConvertTo-Json
    $session = Invoke-RestMethod -Uri $sessionsUrl -Method Post -Body $sessionBody -ContentType "application/json"
    $sessionId = $session.id
    Write-Host "Created Session: $sessionId"


    # 1. Create SessionGameBoard
    Write-Host "`nTesting Create SessionGameBoard..."
    $sgbBody = @{
        SessionId = $sessionId
        BoardId   = $boardId
        Data      = "Initial Data"
    } | ConvertTo-Json
    
    $sgb = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $sgbBody -ContentType "application/json"
    $sgbId = $sgb.id
    Write-Host "Created SessionGameBoard: $sgbId"

    if ($sgb.sessionId -eq $sessionId -and $sgb.boardId -eq $boardId) {
        Write-Host "Create Verified."
    }
    else {
        Write-Error "Create Failed: ID mismatch."
    }

    # 2. Test Validation (Invalid SessionId)
    Write-Host "`nTesting Validation (Invalid SessionId)..."
    $invalidSessionBody = @{
        SessionId = [Guid]::NewGuid()
        BoardId   = $boardId
        Data      = "Invalid Session"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $invalidSessionBody -ContentType "application/json"
        Write-Error "Should have failed with invalid SessionId"
    }
    catch {
        Write-Host "Correctly failed with invalid SessionId"
    }

    # 3. Test Validation (Invalid BoardId)
    Write-Host "`nTesting Validation (Invalid BoardId)..."
    $invalidBoardBody = @{
        SessionId = $sessionId
        BoardId   = [Guid]::NewGuid()
        Data      = "Invalid Board"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $invalidBoardBody -ContentType "application/json"
        Write-Error "Should have failed with invalid BoardId"
    }
    catch {
        Write-Host "Correctly failed with invalid BoardId"
    }

    # 4. Get By Id
    Write-Host "`nTesting GetById..."
    $retrieved = Invoke-RestMethod -Uri "$baseUrl/$sgbId" -Method Get
    if ($retrieved.id -eq $sgbId) {
        Write-Host "GetById Verified."
    }
    else {
        Write-Error "GetById Failed."
    }

    # 5. Update
    Write-Host "`nTesting Update..."
    $retrieved.data = "Updated Data"
    $updateBody = $retrieved | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/$sgbId" -Method Put -Body $updateBody -ContentType "application/json"
    
    $updated = Invoke-RestMethod -Uri "$baseUrl/$sgbId" -Method Get
    if ($updated.data -eq "Updated Data") {
        Write-Host "Update Verified."
    }
    else {
        Write-Error "Update Failed."
    }

    # 6. Delete
    Write-Host "`nTesting Delete..."
    Invoke-RestMethod -Uri "$baseUrl/$sgbId" -Method Delete
    
    try {
        Invoke-RestMethod -Uri "$baseUrl/$sgbId" -Method Get
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
