param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')

$ErrorActionPreference = 'Stop'
$settlementRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$settlementMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$settlementCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
if (-not (Test-Path -LiteralPath $settlementMono) -or -not (Test-Path -LiteralPath $settlementCompiler)) {
    throw 'Pass -UnityEditor with an installed Editor directory containing Mono and csc; Unity itself is not launched.'
}
$settlementSource = Join-Path $settlementRepository 'output_Unity/Assets/Scripts/Assembly-CSharp/LDNJKLHLHAH.cs'
$settlementStubs = Join-Path $PSScriptRoot 'SettlementRouteStubs.cs'
$settlementTests = Join-Path $PSScriptRoot 'SettlementRouteTests.cs'
$settlementOutput = Join-Path ([System.IO.Path]::GetTempPath()) ('Fruit-SettlementRouteTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
$settlementResult = 1
try {
    & $settlementMono $settlementCompiler /nologo /target:exe "/out:$settlementOutput" $settlementSource $settlementStubs $settlementTests
    $settlementResult = $LASTEXITCODE
    if ($settlementResult -eq 0) {
        & $settlementMono $settlementOutput
        $settlementResult = $LASTEXITCODE
    }
} finally {
    if (Test-Path -LiteralPath $settlementOutput) {
        Remove-Item -LiteralPath $settlementOutput -Force
    }
}
exit $settlementResult
