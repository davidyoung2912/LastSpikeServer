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


$baseUrl = "http://localhost:5098/api/gameboards"

try {
    # 1. Create a gameboard
    Write-Host "Creating gameboard..."
    $newId = [Guid]::NewGuid()
    $body = @{
        Id          = $newId
        Description = "Test GameBoard"
        Data        = "Initial Data"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $body -ContentType "application/json"
    Write-Host "Created gameboard with ID: $($response.id)"

    # 2. Get the gameboard
    Write-Host "Getting gameboard..."
    $gameBoard = Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
    Write-Host "Retrieved gameboard: $($gameBoard.description) ($($gameBoard.data))"

    # 3. Update the gameboard
    Write-Host "Updating gameboard..."
    $gameBoard.data = "Updated Data"
    $bodyUpdate = $gameBoard | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Put -Body $bodyUpdate -ContentType "application/json"
    Write-Host "GameBoard updated."

    # 4. Verify update
    $updatedGameBoard = Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
    if ($updatedGameBoard.data -eq "Updated Data") {
        Write-Host "Update verified."
    }
    else {
        Write-Error "Update failed."
    }

    # 5. Delete the gameboard
    Write-Host "Deleting gameboard..."
    Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Delete
    Write-Host "GameBoard deleted."

    # 6. Verify delete
    try {
        Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Get
        Write-Error "Delete failed. GameBoard still exists."
    }
    catch {
        Write-Host "Delete verified. GameBoard not found."
    }
}
catch {
    Write-Error "An error occurred: $_"
}
finally {
    # Cleanup: Delete created test data if it still exists
    Write-Host "`nCleaning up test data..."
    try {
        if ($newId) {
            Invoke-RestMethod -Uri "$baseUrl/$newId" -Method Delete -ErrorAction SilentlyContinue
            Write-Host "Cleaned up GameBoard: $newId"
        }
    }
    catch {
        Write-Host "Cleanup error (non-critical): $_"
    }
    
    Stop-Process -Id $process.Id -Force
    Write-Host "Application stopped."
}
