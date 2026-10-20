param(
    [string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor',
    [string]$RecoveryCopy
)

$startupRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$startupMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$startupCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
if (-not (Test-Path -LiteralPath $startupMono) -or -not (Test-Path -LiteralPath $startupCompiler)) {
    throw 'Pass an installed Unity Editor directory containing Mono and csc; Unity itself is not launched.'
}
$startupRoot = Join-Path $env:TEMP ('Fruit-StartupRecovery-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $startupRoot | Out-Null
$startupOutput = Join-Path $startupRoot 'StartupRecoveryTests.exe'
$startupSourceRoot = Join-Path $startupRepository 'output_Unity/Assets/Scripts/Assembly-CSharp'
$startupSources = @(
    'ApkEarning/ApkEarningDomain.cs',
    'ApkEarning/ApkEarningService.cs',
    'ApkEarning/ApkEarningConfig.cs',
    'Withdrawal/WithdrawalGameplayBridge.cs',
    'Withdrawal/WithdrawalStorage.cs',
    'Withdrawal/WithdrawalState.cs',
    'Withdrawal/WithdrawalRules.cs'
) | ForEach-Object { Join-Path $startupSourceRoot $_ }
# Reuse the existing resource/path stubs. /main selects only this fixture's entry.
& $startupMono $startupCompiler /nologo /target:exe /main:ApkStartupRecoveryTests "/out:$startupOutput" @startupSources `
    (Join-Path $PSScriptRoot 'ApkServiceTests.cs') (Join-Path $PSScriptRoot 'ApkStartupRecoveryTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
foreach ($startupCase in @('unclaimed', 'claimed', 'unfinished', 'failed-save')) {
    $startupCasePath = Join-Path $startupRoot $startupCase
    $startupSeed = if ($startupCase -eq 'failed-save') { 'unclaimed' } else { $startupCase }
    & $startupMono $startupOutput "seed-$startupSeed" $startupCasePath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $startupResume = if ($startupCase -eq 'failed-save') { 'resume-failed-save' } else { 'resume' }
    & $startupMono $startupOutput $startupResume $startupCasePath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    # A second independent process exercises a genuine cold restart of the service.
    & $startupMono $startupOutput 'resume' $startupCasePath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
& $startupMono $startupOutput 'runtime-claim-retry' (Join-Path $startupRoot 'runtime-claim-retry')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if ($RecoveryCopy) {
    $startupCopyPath = Join-Path $startupRoot 'supplied-copy'
    & $startupMono $startupOutput 'seed-copy' $startupCopyPath $RecoveryCopy
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $startupMono $startupOutput 'resume' $startupCopyPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $startupMono $startupOutput 'resume' $startupCopyPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
Write-Output "APK_STARTUP_RECOVERY_SUITE_PASS isolatedRoot=$startupRoot"
