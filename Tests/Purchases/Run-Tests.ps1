param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
$purchaseRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$purchaseData = Join-Path $UnityEditor 'Data'
$purchaseMono = Join-Path $purchaseData 'MonoBleedingEdge/bin/mono.exe'
$purchaseCompiler = Join-Path $purchaseData 'MonoBleedingEdge/lib/mono/4.5/csc.exe'
$purchaseOutput = Join-Path $env:TEMP 'Fruit-PurchaseDisabledTests.exe'
$purchaseSources = @('CorePlayItemBtn.cs','POJCEPBNNIP.cs','DBMNIMJGPGN.cs') |
    ForEach-Object { Join-Path $purchaseRepository ('output_Unity/Assets/Scripts/Assembly-CSharp/' + $_) }
& $purchaseMono $purchaseCompiler /nologo /nowarn:0649 /target:exe "/out:$purchaseOutput" @purchaseSources (Join-Path $PSScriptRoot 'PurchaseDisabledTests.cs')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $purchaseMono $purchaseOutput
exit $LASTEXITCODE
