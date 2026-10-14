param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')

$apkRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$apkMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$apkCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
if (-not (Test-Path -LiteralPath $apkMono) -or -not (Test-Path -LiteralPath $apkCompiler)) {
    throw 'Pass -UnityEditor with an installed Editor directory containing Mono and csc; Unity itself is not launched.'
}
$apkOutput = Join-Path $env:TEMP ('Fruit-ApkServiceTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
$apkSourceRoot = Join-Path $apkRepository 'output_Unity/Assets/Scripts/Assembly-CSharp'
$apkSources = @(
    'ApkEarning/ApkEarningDomain.cs',
    'ApkEarning/ApkEarningService.cs',
    'ApkEarning/ApkEarningConfig.cs',
    'Withdrawal/WithdrawalGameplayBridge.cs',
    'Withdrawal/WithdrawalStorage.cs',
    'Withdrawal/WithdrawalState.cs',
    'Withdrawal/WithdrawalRules.cs'
) | ForEach-Object { Join-Path $apkSourceRoot $_ }
& $apkMono $apkCompiler /nologo /target:exe "/out:$apkOutput" @apkSources (Join-Path $PSScriptRoot 'ApkServiceTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $apkMono $apkOutput
exit $LASTEXITCODE
