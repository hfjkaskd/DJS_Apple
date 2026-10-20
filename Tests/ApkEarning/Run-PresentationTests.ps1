param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
$presentationRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$presentationMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$presentationCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
$presentationOutput = Join-Path $env:TEMP ('Fruit-ApkPresentation-' + [Guid]::NewGuid().ToString('N') + '.exe')
$presentationSource = Join-Path $presentationRepository 'output_Unity/Assets/Scripts/Assembly-CSharp/ApkEarning/ApkEarningPresentation.cs'
& $presentationMono $presentationCompiler /nologo /target:exe "/out:$presentationOutput" $presentationSource `
    (Join-Path $PSScriptRoot 'ApkPresentationTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $presentationMono $presentationOutput
exit $LASTEXITCODE
