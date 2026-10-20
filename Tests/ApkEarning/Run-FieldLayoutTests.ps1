param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
$fieldRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$fieldMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$fieldCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
$fieldNewtonsoft = Join-Path $UnityEditor 'Data/Managed/Newtonsoft.Json.dll'
$fieldNetstandard = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/Facades/netstandard.dll'
$fieldOutput = Join-Path $env:TEMP 'Fruit-FieldLayoutTests.exe'
$fieldDomain = Join-Path $fieldRepository 'output_Unity/Assets/Scripts/Assembly-CSharp/CorePlay/FieldLayoutGeometry.cs'
& $fieldMono $fieldCompiler /nologo /target:exe "/out:$fieldOutput" "/r:$fieldNewtonsoft" "/r:$fieldNetstandard" $fieldDomain (Join-Path $PSScriptRoot 'FieldLayoutTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$fieldPreviousMonoPath = $env:MONO_PATH
try {
    $env:MONO_PATH = (Join-Path $UnityEditor 'Data/Managed')
    $fieldAssets = Join-Path $fieldRepository 'output_Unity/Assets'
    & $fieldMono $fieldOutput `
        (Join-Path $fieldAssets 'res_server/server_levelconfigs/mainlevel') `
        (Join-Path $fieldAssets 'res_server/server_configs/AllEleImageConfig.json') `
        (Join-Path $fieldAssets 'res/local/coreplay/CorePlayUI.prefab') `
        (Join-Path $fieldAssets 'Game.unity') `
        (Join-Path $fieldAssets 'res/local/coreplay/prefab/CollectItem.prefab') `
        (Join-Path $fieldAssets 'res/local/coreplay/effct/anim') `
        (Join-Path $fieldAssets 'res/local/withdrawal/ApkEarningTop.prefab')
    $fieldResult = $LASTEXITCODE
} finally {
    $env:MONO_PATH = $fieldPreviousMonoPath
}
exit $fieldResult
