param(
    [string]$DllDirectory,
    [string]$OutputPath,
    [string]$CecilPath = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/Managed/Unity.Cecil.dll'
)
$ErrorActionPreference = 'Stop'
Add-Type -Path $CecilPath
$rows = foreach ($name in @('Newtonsoft.Json.dll', 'spine-csharp.dll', 'spine-unity.dll', 'DOTween.dll', 'DOTween.Modules.dll')) {
    $path = Join-Path $DllDirectory $name
    $assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($path)
    try {
        $methods = @($assembly.MainModule.Types | ForEach-Object { $_.Methods } | Where-Object { $_.HasBody })
        $samples = foreach ($type in $assembly.MainModule.Types) {
            foreach ($method in $type.Methods) {
                $wanted = ($type.FullName -eq 'Newtonsoft.Json.JsonConvert' -and $method.Name -in @('SerializeObject', 'DeserializeObject')) -or
                    ($type.FullName -in @('Spine.AnimationState', 'Spine.SkeletonJson', 'Spine.Unity.SkeletonGraphic', 'Spine.Unity.SkeletonDataAsset') -and $method.Name -in @('Apply', 'ReadSkeletonData', 'Initialize', 'GetSkeletonData')) -or
                    ($type.FullName -eq 'DG.Tweening.DOTween' -and $method.Name -in @('Sequence', 'Init')) -or
                    ($type.FullName -eq 'DG.Tweening.DOTweenModuleUI' -and $method.Name -eq 'DOAnchorPos')
                if ($wanted -and $method.HasBody) {
                    [pscustomobject]@{ method = $method.FullName; instructions = $method.Body.Instructions.Count;
                        leadingIL = @($method.Body.Instructions | Select-Object -First 8 | ForEach-Object { $_.ToString() }) }
                }
            }
        }
        [pscustomobject]@{ name = $name; identity = $assembly.Name.FullName; bytes = (Get-Item -LiteralPath $path).Length;
            sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant();
            methodsWithBody = $methods.Count; methodsWithAtMostThreeInstructions = @($methods | Where-Object { $_.Body.Instructions.Count -le 3 }).Count;
            references = @($assembly.MainModule.AssemblyReferences | ForEach-Object { $_.FullName }); samples = @($samples) }
    } finally { $assembly.Dispose() }
}
$json = ConvertTo-Json -InputObject @($rows) -Depth 8
if ($OutputPath) { [IO.File]::WriteAllText($OutputPath, $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false)) }
else { $json }
