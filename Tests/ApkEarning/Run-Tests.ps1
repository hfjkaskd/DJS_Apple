param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
$apkRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$apkMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$apkCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
$apkOutput = Join-Path $env:TEMP 'Fruit-ApkEarningTests.exe'
$apkDomain = Join-Path $apkRepository 'output_Unity/Assets/Scripts/Assembly-CSharp/ApkEarning/ApkEarningDomain.cs'
& $apkMono $apkCompiler /nologo /target:exe "/out:$apkOutput" $apkDomain (Join-Path $PSScriptRoot 'ApkEarningTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $apkMono $apkOutput
exit $LASTEXITCODE
