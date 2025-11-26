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

$sessionsUrl = "http://localhost:5098/api/sessions"
$playersUrl = "http://localhost:5098/api/players"
$gameBoardsUrl = "http://localhost:5098/api/gameboards"

try {
    # 0. Setup: Create Players and GameBoard
    Write-Host "Creating Setup Data..."
    
    $player1 = Invoke-RestMethod -Uri $playersUrl -Method Post -Body (@{ Name = "Player 1" } | ConvertTo-Json) -ContentType "application/json"
    $player2 = Invoke-RestMethod -Uri $playersUrl -Method Post -Body (@{ Name = "Player 2" } | ConvertTo-Json) -ContentType "application/json"
    $board = Invoke-RestMethod -Uri $gameBoardsUrl -Method Post -Body (@{ Description = "Timing Test Board"; Data = "Data" } | ConvertTo-Json) -ContentType "application/json"

    # 1. Create Session
    Write-Host "`nCreating Session..."
    $sessionBody = @{
        Description = "Timing Test Session"
        BoardId     = $board.id
        PlayerIds   = @()
    } | ConvertTo-Json
    $session = Invoke-RestMethod -Uri $sessionsUrl -Method Post -Body $sessionBody -ContentType "application/json"
    $sessionId = $session.id

    if ($null -eq $session.startTime) {
        Write-Host "Verified: StartTime is null initially."
    }
    else {
        Write-Error "Failed: StartTime should be null."
    }

    # 2. Add First Player
    Write-Host "`nAdding First Player..."
    Invoke-RestMethod -Uri "$sessionsUrl/$sessionId/players/$($player1.id)" -Method Post
    $session = Invoke-RestMethod -Uri "$sessionsUrl/$sessionId" -Method Get
    
    if ($null -ne $session.startTime) {
        Write-Host "Verified: StartTime set after first player added."
        $startTime = $session.startTime
    }
    else {
        Write-Error "Failed: StartTime should be set."
    }

    # 3. Add Second Player
    Write-Host "`nAdding Second Player..."
    Start-Sleep -Seconds 1 # Ensure time difference if any
    Invoke-RestMethod -Uri "$sessionsUrl/$sessionId/players/$($player2.id)" -Method Post
    $session = Invoke-RestMethod -Uri "$sessionsUrl/$sessionId" -Method Get
    
    if ($session.startTime -eq $startTime) {
        Write-Host "Verified: StartTime unchanged after second player added."
    }
    else {
        Write-Error "Failed: StartTime changed."
    }

    # 4. Remove First Player
    Write-Host "`nRemoving First Player..."
    Invoke-RestMethod -Uri "$sessionsUrl/$sessionId/players/$($player1.id)" -Method Delete
    $session = Invoke-RestMethod -Uri "$sessionsUrl/$sessionId" -Method Get
    
    if ($null -eq $session.endTime) {
        Write-Host "Verified: EndTime is null while players remain."
    }
    else {
        Write-Error "Failed: EndTime should be null."
    }

    # 5. Remove Last Player
    Write-Host "`nRemoving Last Player..."
    Invoke-RestMethod -Uri "$sessionsUrl/$sessionId/players/$($player2.id)" -Method Delete
    $session = Invoke-RestMethod -Uri "$sessionsUrl/$sessionId" -Method Get
    
    if ($null -ne $session.endTime) {
        Write-Host "Verified: EndTime set after last player removed."
    }
    else {
        Write-Error "Failed: EndTime should be set."
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
        if ($player1) {
            Invoke-RestMethod -Uri "$playersUrl/$($player1.id)" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted Player 1"
        }
        if ($player2) {
            Invoke-RestMethod -Uri "$playersUrl/$($player2.id)" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted Player 2"
        }
        if ($board) {
            Invoke-RestMethod -Uri "$gameBoardsUrl/$($board.id)" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted GameBoard"
        }
    }
    catch {
        Write-Host "Cleanup error (non-critical): $_"
    }
    
    Stop-Process -Id $process.Id -Force
    Write-Host "Application stopped."
}
