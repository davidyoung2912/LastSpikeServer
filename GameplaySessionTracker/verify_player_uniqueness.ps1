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

try {
    # 1. Create first player
    Write-Host "Creating first player..."
    $player1Id = [Guid]::NewGuid()
    $player1Body = @{
        Id    = $player1Id
        Name  = "John Doe"
        Alias = "JD"
    } | ConvertTo-Json
    $player1 = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $player1Body -ContentType "application/json"
    Write-Host "Created player: $($player1.name) ($($player1.alias))"

    # 2. Try to create player with duplicate Name (should fail)
    Write-Host "`nTesting duplicate Name (Expect Failure)..."
    $player2Id = [Guid]::NewGuid()
    $player2Body = @{
        Id    = $player2Id
        Name  = "John Doe"  # Same name
        Alias = "Different"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $player2Body -ContentType "application/json"
        Write-Error "Should have failed with duplicate Name"
    }
    catch {
        Write-Host "Correctly rejected duplicate Name"
    }

    # 3. Try to create player with duplicate Alias (should fail)
    Write-Host "`nTesting duplicate Alias (Expect Failure)..."
    $player3Id = [Guid]::NewGuid()
    $player3Body = @{
        Id    = $player3Id
        Name  = "Jane Smith"
        Alias = "JD"  # Same alias
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $player3Body -ContentType "application/json"
        Write-Error "Should have failed with duplicate Alias"
    }
    catch {
        Write-Host "Correctly rejected duplicate Alias"
    }

    # 4. Create second player with unique values (should succeed)
    Write-Host "`nCreating second player with unique values..."
    $player4Id = [Guid]::NewGuid()
    $player4Body = @{
        Id    = $player4Id
        Name  = "Jane Smith"
        Alias = "JS"
    } | ConvertTo-Json
    $player4 = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $player4Body -ContentType "application/json"
    Write-Host "Successfully created player: $($player4.name) ($($player4.alias))"

    # 5. Try to update player4 to have same Name as player1 (should fail)
    Write-Host "`nTesting update with duplicate Name (Expect Failure)..."
    $player4.name = "John Doe"
    $updateBody = $player4 | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "$baseUrl/$player4Id" -Method Put -Body $updateBody -ContentType "application/json"
        Write-Error "Should have failed with duplicate Name on update"
    }
    catch {
        Write-Host "Correctly rejected duplicate Name on update"
    }

    # 6. Update player4 with unique values (should succeed)
    Write-Host "`nUpdating player with unique values..."
    $player4Refresh = Invoke-RestMethod -Uri "$baseUrl/$player4Id" -Method Get
    $player4Refresh.name = "Janet Smith"
    $updateBody = $player4Refresh | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/$player4Id" -Method Put -Body $updateBody -ContentType "application/json"
    Write-Host "Successfully updated player"

}
catch {
    Write-Error "An error occurred: $_"
}
finally {
    # Cleanup: Delete created test data
    Write-Host "`nCleaning up test data..."
    try {
        if ($player1Id) {
            Invoke-RestMethod -Uri "$baseUrl/$player1Id" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted Player 1"
        }
        if ($player4Id) {
            Invoke-RestMethod -Uri "$baseUrl/$player4Id" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Deleted Player 2"
        }
    }
    catch {
        Write-Host "Cleanup error (non-critical): $_"
    }
    
    Stop-Process -Id $process.Id -Force
    Write-Host "Application stopped."
}
