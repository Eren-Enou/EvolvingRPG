param([Parameter(Mandatory)][string]$PlayerPath)
$ErrorActionPreference = 'Stop'
$player = (Resolve-Path -LiteralPath $PlayerPath).Path
if ((Split-Path $player -Leaf) -ne 'EvolvingRPG.exe') { throw 'Select a packaged EvolvingRPG development player.' }
$token = [guid]::NewGuid().ToString('N')
$saveRoot = Join-Path ([System.IO.Path]::GetTempPath()) "EvolvingRPG-RelayRestart-$token"
$logRoot = Join-Path (Split-Path $PSScriptRoot -Parent) "Logs/RelayPlayerRestart/$token"
New-Item -ItemType Directory -Path $logRoot | Out-Null

function Invoke-RestartStage([string]$Stage) {
    $log = Join-Path $logRoot "$Stage.log"
    $arguments = @('-batchmode', '-evolvingRelayRestart', $Stage, $token, '-logFile', ('"' + $log + '"'))
    $process = Start-Process -FilePath $player -WorkingDirectory (Split-Path $player) -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (!$process.WaitForExit(45000)) {
        $process.Kill()
        throw "Packaged restart test timed out during $Stage. Log: $log"
    }
    $success = Select-String -LiteralPath $log -Pattern "^PASS Relay restart ${Stage}: ([a-f0-9]{32}) "
    if ($process.ExitCode -ne 0 -or !$success -or (Select-String -LiteralPath $log -Pattern '^FAIL Relay player smoke:')) {
        Get-Content -LiteralPath $log -Tail 22 | Write-Host
        throw "Packaged restart test failed during $Stage. Log: $log"
    }
    Write-Host $success.Line
    return $success.Matches[0].Groups[1].Value
}

$written = Invoke-RestartStage 'write'
$latest = Join-Path $saveRoot 'character-a.json'
$backup = "$latest.bak"
foreach ($path in @($saveRoot, $latest, $backup)) {
    if ((Get-Item -LiteralPath $path).Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Unexpected redirected fixture path: $path"
    }
}
$latestHash = (Get-FileHash -LiteralPath $latest).Hash
$backupHash = (Get-FileHash -LiteralPath $backup).Hash
if ($latestHash -eq $backupHash) { throw 'Fixture did not save distinct latest and backup records.' }
if ((Invoke-RestartStage 'reload') -ne $written -or (Get-FileHash -LiteralPath $latest).Hash -ne $latestHash -or
    (Get-FileHash -LiteralPath $backup).Hash -ne $backupHash) {
    throw 'Reload or replay changed the character identity or either saved file.'
}

[System.IO.File]::WriteAllText($latest, '{damaged restart fixture')
$damagedHash = (Get-FileHash -LiteralPath $latest).Hash
if ((Invoke-RestartStage 'recover') -ne $written -or (Get-FileHash -LiteralPath $latest).Hash -ne $damagedHash -or
    (Get-FileHash -LiteralPath $backup).Hash -ne $backupHash -or
    (Get-FileHash -LiteralPath (Join-Path $saveRoot 'character-b.json')).Hash -ne $backupHash) {
    throw 'Recovery changed the identity, rewrote slot A, or failed to preserve the recovered record in B.'
}
Write-Output "PASS Packaged restart: three separate processes, stable identity, repeat reward denied, recovery non-writing. Logs: $logRoot"
