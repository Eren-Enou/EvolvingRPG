param([string]$UnityEditorPath)
$ErrorActionPreference = 'Stop'
# Compile against the existing Editor's reference graph without launching Unity or writing Library.
# This is a source/API check, never a substitute for Unity import, serialization or Play mode tests.
$project = Split-Path $PSScriptRoot -Parent
Push-Location $project
try {
    if (!$UnityEditorPath) {
        $version = ((Get-Content ProjectSettings/ProjectVersion.txt | Select-String '^m_EditorVersion:').Line -split ': ')[1]
        $UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\$version\Editor"
    }
    $runtimeRsp = Get-ChildItem Library/Bee/artifacts -Recurse -Filter Assembly-CSharp.rsp |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (!$runtimeRsp) { throw 'No existing Unity compilation graph. Import the project in the configured Editor first.' }
    $editorRsp = Join-Path $runtimeRsp.DirectoryName 'Assembly-CSharp-Editor.rsp'
    if (!(Test-Path -LiteralPath $editorRsp)) { throw 'Missing matching Editor compilation graph.' }
    $sdk = Get-ChildItem (Join-Path $UnityEditorPath 'Data/DotNetSdk/sdk') -Directory |
        Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    $compiler = Join-Path $sdk.FullName 'Roslyn/bincore/csc.dll'
    $dotnet = Join-Path $UnityEditorPath 'Data/DotNetSdk/dotnet.exe'
    $output = Join-Path $project 'Logs/SourceCompile'
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    $report = Join-Path $output 'Report.txt'
    "Source/API check $(Get-Date -Format o); cached graph $($runtimeRsp.FullName). No Unity process launched." | Set-Content $report
    foreach ($assembly in @('Assembly-CSharp', 'Assembly-CSharp-Editor')) {
        $isEditor = $assembly.EndsWith('-Editor')
        $inputRsp = if ($isEditor) { $editorRsp } else { $runtimeRsp.FullName }
        $lines = foreach ($line in Get-Content -LiteralPath $inputRsp) {
            if ($line -match '^[-/]out:') { '-out:"' + (Join-Path $output "$assembly.dll") + '"' }
            elseif ($line -match '^[-/]refout:') { '-refout:"' + (Join-Path $output "$assembly.ref.dll") + '"' }
            elseif ($line -match '^"Assets/Game/.+\.cs"$') { continue }
            elseif ($isEditor -and $line -match '^[-/]r:".*[/\\]Assembly-CSharp\.ref\.dll"$') {
                '-r:"' + (Join-Path $output 'Assembly-CSharp.ref.dll') + '"'
            }
            else { $line }
        }
        $sources = Get-ChildItem Assets/Game -Recurse -Filter '*.cs' | Where-Object {
            ($_.FullName -match '[/\\]Editor[/\\]') -eq $isEditor
        } | ForEach-Object { '"' + $_.FullName + '"' }
        $copy = Join-Path $output "$assembly.rsp"
        @($lines) + @($sources) | Set-Content -LiteralPath $copy -Encoding utf8
        $result = & $dotnet exec $compiler /nostdlib /noconfig "@$copy" 2>&1
        $exit = $LASTEXITCODE
        $result | Tee-Object -FilePath $report -Append
        if ($exit -ne 0) { throw "$assembly source compile failed ($exit). See $report" }
        "PASS $assembly source compile" | Tee-Object -FilePath $report -Append
    }
}
finally { Pop-Location }
