# kill-backend.ps1
# Kills all StackFlow backend processes (StackFlow.Api.exe + dotnet.exe on port 5000).
# Run from any terminal: .\kill-backend.ps1

Write-Host "Stopping StackFlow backend processes..."

# Kill the compiled exe (VS debug mode)
$api = Get-Process -Name "StackFlow.Api" -ErrorAction SilentlyContinue
if ($api) {
    $api | Stop-Process -Force
    Write-Host "  Killed StackFlow.Api.exe (PID $($api.Id))"
} else {
    Write-Host "  StackFlow.Api.exe not running"
}

# Kill any dotnet process holding port 5000
$port5000 = netstat -ano | Select-String ":5000 " | ForEach-Object {
    ($_ -split '\s+')[-1]
} | Select-Object -Unique | Where-Object { $_ -match '^\d+$' }

foreach ($pid in $port5000) {
    $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
    if ($proc) {
        $proc | Stop-Process -Force
        Write-Host "  Killed $($proc.Name) (PID $pid) holding port 5000"
    }
}

# Also kill loose dotnet.exe processes (from dotnet run)
$dotnet = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnet) {
    $dotnet | Stop-Process -Force
    Write-Host "  Killed $($dotnet.Count) dotnet.exe process(es)"
} else {
    Write-Host "  No dotnet.exe processes running"
}

Write-Host "Done."
