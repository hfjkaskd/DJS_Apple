param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
$presentationRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$presentationData = Join-Path $UnityEditor 'Data'
$presentationMono = Join-Path $presentationData 'MonoBleedingEdge/bin/mono.exe'
$presentationCompiler = Join-Path $presentationData 'MonoBleedingEdge/lib/mono/4.5/csc.exe'
$presentationOutput = Join-Path $env:TEMP 'FruitWithdrawal-PresentationTests.exe'
$presentationSources = @('WithdrawalRules.cs','WithdrawalState.cs','WithdrawalEngine.cs','WithdrawalStorage.cs','WithdrawalPresentation.cs','WithdrawalHost.cs','WithdrawalPageView.cs','WithdrawalHud.cs') |
    ForEach-Object { Join-Path $presentationRepository ('output_Unity/Assets/Scripts/Assembly-CSharp/Withdrawal/' + $_) }
& $presentationMono $presentationCompiler /nologo /target:exe "/out:$presentationOutput" @presentationSources (Join-Path $PSScriptRoot 'PresentationUnityStubs.cs') (Join-Path $PSScriptRoot 'WithdrawalPresentationTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $presentationMono $presentationOutput
exit $LASTEXITCODE
