param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')

$withdrawalRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$withdrawalMono = Join-Path $UnityEditor 'Data/MonoBleedingEdge/bin/mono.exe'
$withdrawalCompiler = Join-Path $UnityEditor 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
if (-not (Test-Path -LiteralPath $withdrawalMono) -or -not (Test-Path -LiteralPath $withdrawalCompiler)) {
    throw 'Pass -UnityEditor with an installed Editor directory containing Mono and csc; Unity itself is not launched.'
}
$withdrawalAssembly = Join-Path $env:TEMP 'FruitWithdrawal-CoreTests.exe'
$withdrawalSources = @('WithdrawalRules.cs', 'WithdrawalState.cs', 'WithdrawalEngine.cs', 'WithdrawalStorage.cs') |
    ForEach-Object { Join-Path $withdrawalRepository ('output_Unity/Assets/Scripts/Assembly-CSharp/Withdrawal/' + $_) }
$withdrawalTest = Join-Path $PSScriptRoot 'WithdrawalCoreTests.cs'
& $withdrawalMono $withdrawalCompiler /nologo /target:exe "/out:$withdrawalAssembly" @withdrawalSources $withdrawalTest
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $withdrawalMono $withdrawalAssembly
exit $LASTEXITCODE
