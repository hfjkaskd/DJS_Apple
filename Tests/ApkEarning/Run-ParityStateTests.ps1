param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
$parityRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$parityMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$parityCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
$parityOutput = Join-Path $env:TEMP ('Fruit-ParityState-' + [Guid]::NewGuid().ToString('N') + '.exe')
$paritySourceRoot = Join-Path $parityRepository 'output_Unity/Assets/Scripts/Assembly-CSharp'
$paritySources = @(
    'ApkEarning/ApkEarningDomain.cs',
    'ApkEarning/ApkEarningService.cs',
    'ApkEarning/ApkEarningConfig.cs',
    'Withdrawal/WithdrawalGameplayBridge.cs',
    'Withdrawal/WithdrawalStorage.cs',
    'Withdrawal/WithdrawalState.cs',
    'Withdrawal/WithdrawalRules.cs'
) | ForEach-Object { Join-Path $paritySourceRoot $_ }
& $parityMono $parityCompiler /nologo /target:exe /main:ApkParityStateTests "/out:$parityOutput" @paritySources `
    (Join-Path $PSScriptRoot 'ApkServiceTests.cs') (Join-Path $PSScriptRoot 'ApkParityStateTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $parityMono $parityOutput
exit $LASTEXITCODE
