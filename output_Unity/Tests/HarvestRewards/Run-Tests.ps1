param(
    [string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$monoPath = Join-Path $UnityEditor 'Editor/Data/MonoBleedingEdge/bin/mono.exe'
$compilerPath = Join-Path $UnityEditor 'Editor/Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
if (!(Test-Path -LiteralPath $monoPath) -or !(Test-Path -LiteralPath $compilerPath)) {
    throw "Unity Mono compiler not found. Set -UnityEditor to an installed Unity editor directory."
}

# Keep build output and storage fault fixtures outside Assets and the repository.
$temporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$testDirectory = [System.IO.Path]::GetFullPath((Join-Path $temporaryRoot ('harvest-rewards-tests-' + [Guid]::NewGuid().ToString('N'))))
if (!$testDirectory.StartsWith($temporaryRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'Test output directory escaped the temporary directory.'
}
New-Item -ItemType Directory -Path $testDirectory | Out-Null
try {
    $executable = Join-Path $testDirectory 'HarvestRewards.Tests.exe'
    $sources = @(
        (Join-Path $projectRoot 'Assets/Scripts/Assembly-CSharp/HarvestRewards/HarvestRewardState.cs'),
        (Join-Path $projectRoot 'Assets/Scripts/Assembly-CSharp/HarvestRewards/HarvestRewardEngine.cs'),
        (Join-Path $projectRoot 'Assets/Scripts/Assembly-CSharp/HarvestRewards/HarvestRewardStorage.cs'),
        (Join-Path $PSScriptRoot 'HarvestRewardsTests.cs')
    )
    foreach ($source in $sources) {
        if (!(Test-Path -LiteralPath $source)) { throw "Required source not found: $source" }
    }
    & $monoPath $compilerPath /nologo /langversion:8.0 /warn:4 /warnaserror+ /target:exe "/out:$executable" @sources
    if ($LASTEXITCODE -ne 0) { throw "Test compilation failed (exit $LASTEXITCODE)." }
    & $monoPath $executable $testDirectory
    if ($LASTEXITCODE -ne 0) { throw "Harvest rewards regression tests failed (exit $LASTEXITCODE)." }
}
finally {
    # Re-check the resolved path immediately before recursive removal.
    $resolvedTestDirectory = [System.IO.Path]::GetFullPath($testDirectory)
    if ($resolvedTestDirectory.StartsWith($temporaryRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase) -and
        [System.IO.Path]::GetFileName($resolvedTestDirectory).StartsWith('harvest-rewards-tests-', [System.StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedTestDirectory -Recurse -Force
    }
}
