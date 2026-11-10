param([string]$Stages = 'all', [int]$TimeoutMinutes = 100, [switch]$KeepOpen)
$ErrorActionPreference = 'Stop'
# Launches ONE interactive Editor for this project only when none is running, runs the requested
# Extended Checks stages through AutomationRunner, and returns its exit code. Never use while you are
# editing the project in another Editor: Unity forbids two Editors on one project and so does this script.
$project = Split-Path $PSScriptRoot -Parent
$running = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object { $_.CommandLine -match [regex]::Escape($project) -or $_.CommandLine -notmatch '-projectPath' }
if ($running) { throw "A Unity Editor is already running (PID $($running.ProcessId -join ',')). Close it first; no second Editor is started." }
$version = ((Get-Content (Join-Path $project 'ProjectSettings/ProjectVersion.txt') | Select-String '^m_EditorVersion:').Line -split ': ')[1]
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
$log = Join-Path $project 'Logs/AutomationUnity.log'
$arguments = @('-projectPath', "`"$project`"", '-logFile', "`"$log`"", '-executeMethod', 'EvolvingRPG.Editor.AutomationRunner.Run', '-evolvingStages', $Stages)
if ($KeepOpen) { $arguments += '-evolvingKeepOpen' }
$process = Start-Process -FilePath $unity -ArgumentList $arguments -PassThru
if (!$process.WaitForExit($TimeoutMinutes * 60 * 1000)) { $process.Kill(); throw "Unity did not finish within $TimeoutMinutes minutes." }
Get-Content (Join-Path $project 'Logs/AutomationRunner.txt') -ErrorAction SilentlyContinue
Get-Content (Join-Path $project 'Logs/ExtendedChecks.txt') -ErrorAction SilentlyContinue
exit $process.ExitCode
