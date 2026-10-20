<#
.SYNOPSIS
Repairs malformed native declarations in four recovered SDK placeholder DLLs.
.DESCRIPTION
Dry-run by default. This does not restore an SDK. A recovered declaration that
has no body and no native module/entry point becomes an explicit, platform-
independent NotSupportedException. All other methods and public signatures are
verified after writing. Original DLLs, hashes and a repair manifest are retained.
#>
[CmdletBinding()]
param(
    [switch]$Apply,
    [string]$ProjectPath = (Join-Path $PSScriptRoot '../output_Unity'),
    [string]$BackupDirectory,
    [string]$UnityEditorData = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data'
)

$ErrorActionPreference = 'Stop'
if ($Apply -and [string]::IsNullOrWhiteSpace($BackupDirectory)) {
    throw '-Apply requires -BackupDirectory.'
}
$projectRoot = (Resolve-Path -LiteralPath $ProjectPath).Path
$pluginRoot = Join-Path $projectRoot 'Assets/Plugins'
$cecil = Join-Path $UnityEditorData 'Managed/Unity.Cecil.dll'
Add-Type -Path $cecil

$allowedFiles = @('Lofelt.NiceVibrations.dll', 'Firebase.App.dll', 'Firebase.Analytics.dll', 'Firebase.Crashlytics.dll')
$nativeFiles = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets') -File -Recurse |
    Where-Object { $_.Extension -in @('.so', '.aar', '.jar', '.a', '.dylib') })
if ($nativeFiles.Count -gt 0) {
    throw 'Native libraries are present. Audit the original SDK before converting recovered imports.'
}

function Get-AllTypes($Types) {
    foreach ($type in $Types) {
        $type
        if ($type.HasNestedTypes) { Get-AllTypes $type.NestedTypes }
    }
}

function Read-Assembly([string]$Path) {
    $resolver = [Mono.Cecil.DefaultAssemblyResolver]::new()
    $resolver.AddSearchDirectory($pluginRoot)
    $resolver.AddSearchDirectory((Join-Path $UnityEditorData 'Managed/UnityEngine'))
    $resolver.AddSearchDirectory((Join-Path $UnityEditorData 'MonoBleedingEdge/lib/mono/unityaot-linux'))
    $resolver.AddSearchDirectory((Join-Path $UnityEditorData 'MonoBleedingEdge/lib/mono/unityaot-linux/Facades'))
    $parameters = [Mono.Cecil.ReaderParameters]::new()
    $parameters.AssemblyResolver = $resolver
    $parameters.InMemory = $true
    $parameters.ReadSymbols = $false
    return [Mono.Cecil.AssemblyDefinition]::ReadAssembly($Path, $parameters)
}

function Get-AttributeSignature($Attributes) {
    return (@($Attributes | ForEach-Object {
        $_.Constructor.FullName + ':' + [Convert]::ToBase64String($_.GetBlob())
    } | Sort-Object) -join '|')
}

function Get-Snapshot($Assembly, $TargetNames) {
    # Normalize precisely the changes that this tool is authorized to make.
    $rows = [System.Collections.Generic.List[string]]::new()
    $rows.Add('assembly:' + $Assembly.Name.FullName)
    $rows.Add('mvid:' + $Assembly.MainModule.Mvid)
    $rows.Add('kind:' + [string]$Assembly.MainModule.Kind)
    $rows.Add('references:' + (($Assembly.MainModule.AssemblyReferences.FullName | Sort-Object) -join '|'))
    $rows.Add('attributes:' + (Get-AttributeSignature $Assembly.CustomAttributes))
    foreach ($type in (Get-AllTypes $Assembly.MainModule.Types)) {
        $rows.Add('type:' + $type.FullName + ':' + [int]$type.Attributes + ':' + $type.BaseType.FullName)
        $rows.Add('type-attributes:' + (Get-AttributeSignature $type.CustomAttributes))
        foreach ($interface in $type.Interfaces) { $rows.Add('interface:' + $interface.InterfaceType.FullName) }
        foreach ($field in $type.Fields) {
            $rows.Add('field:' + $field.FullName + ':' + [int]$field.Attributes + ':' + [string]$field.Constant)
            $rows.Add('field-attributes:' + (Get-AttributeSignature $field.CustomAttributes))
        }
        foreach ($property in $type.Properties) { $rows.Add('property:' + $property.FullName + ':' + [int]$property.Attributes) }
        foreach ($event in $type.Events) { $rows.Add('event:' + $event.FullName + ':' + [int]$event.Attributes) }
        foreach ($method in $type.Methods) {
            $target = $TargetNames.Contains($method.FullName)
            $attributes = [int]$method.Attributes
            $impl = [int]$method.ImplAttributes
            if ($target) { $attributes = $attributes -band (-bnot 0x2000); $impl = 0 }
            $rows.Add('method:' + $method.FullName + ':' + $attributes + ':' + $impl)
            $rows.Add('method-attributes:' + (Get-AttributeSignature $method.CustomAttributes))
            foreach ($parameter in $method.Parameters) {
                $rows.Add('parameter:' + $parameter.Name + ':' + $parameter.ParameterType.FullName + ':' +
                    [int]$parameter.Attributes + ':' + [string]$parameter.Constant)
                $rows.Add('parameter-attributes:' + (Get-AttributeSignature $parameter.CustomAttributes))
            }
            foreach ($override in $method.Overrides) { $rows.Add('override:' + $override.FullName) }
            if ($target) { continue }
            $rows.Add('has-body:' + $method.HasBody)
            if ($method.HasBody) {
                $rows.Add('init-locals:' + $method.Body.InitLocals)
                foreach ($variable in $method.Body.Variables) { $rows.Add('local:' + $variable.VariableType.FullName) }
                foreach ($instruction in $method.Body.Instructions) { $rows.Add('il:' + $instruction.ToString()) }
                foreach ($handler in $method.Body.ExceptionHandlers) {
                    $rows.Add('handler:' + $handler.HandlerType + ':' + $handler.CatchType.FullName + ':' +
                        $handler.TryStart.Offset + ':' + $handler.TryEnd.Offset + ':' +
                        $handler.HandlerStart.Offset + ':' + $handler.HandlerEnd.Offset + ':' + $handler.FilterStart.Offset)
                }
            }
        }
    }
    return ($rows -join "`n")
}

$planned = [System.Collections.Generic.List[object]]::new()
foreach ($fileName in $allowedFiles) {
    $path = Join-Path $pluginRoot $fileName
    $assembly = Read-Assembly $path
    try {
        if ($assembly.MainModule.Kind -ne [Mono.Cecil.ModuleKind]::Dll) {
            throw "$fileName must first have a valid DLL header."
        }
        $targets = @((Get-AllTypes $assembly.MainModule.Types) | ForEach-Object { $_.Methods } |
            Where-Object {
                $_.IsPInvokeImpl -and -not $_.HasBody -and
                ($null -eq $_.PInvokeInfo -or $null -eq $_.PInvokeInfo.Module -or
                    [string]::IsNullOrWhiteSpace($_.PInvokeInfo.EntryPoint))
            })
        if ($targets.Count -eq 0) { continue }
        $names = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($method in $targets) { [void]$names.Add($method.FullName) }
        $beforeSnapshot = Get-Snapshot $assembly $names
        $beforeHash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        $record = [ordered]@{file=$fileName; count=$targets.Count; beforeSha256=$beforeHash; methods=@($names | Sort-Object)}
        if (-not $Apply) { $planned.Add($record); continue }

        [void][IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($BackupDirectory))
        $backupPath = Join-Path $BackupDirectory $fileName
        if (Test-Path -LiteralPath $backupPath) {
            if ((Get-FileHash -LiteralPath $backupPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $beforeHash) {
                throw "Refusing to overwrite a different original backup: $backupPath"
            }
        } else { Copy-Item -LiteralPath $path -Destination $backupPath }

        $module = $assembly.MainModule
        # Use the recovered assembly's own core library, never the host .NET runtime.
        $exceptionType = [Mono.Cecil.TypeReference]::new('System', 'NotSupportedException', $module, $module.TypeSystem.CoreLibrary)
        $exceptionCtor = [Mono.Cecil.MethodReference]::new('.ctor', $module.TypeSystem.Void, $exceptionType)
        $exceptionCtor.HasThis = $true
        $exceptionCtor.Parameters.Add([Mono.Cecil.ParameterDefinition]::new($module.TypeSystem.String))
        foreach ($method in $targets) {
            $method.PInvokeInfo = $null
            $method.IsPInvokeImpl = $false
            $method.ImplAttributes = [Mono.Cecil.MethodImplAttributes]::IL -bor [Mono.Cecil.MethodImplAttributes]::Managed
            $method.Body = [Mono.Cecil.Cil.MethodBody]::new($method)
            $processor = $method.Body.GetILProcessor()
            $message = 'Recovered SDK native binding unavailable: ' + $method.FullName
            $processor.Append([Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldstr, $message))
            $processor.Append([Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Newobj, $exceptionCtor))
            $processor.Append([Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Throw))
        }

        $stagedPath = Join-Path $BackupDirectory ($fileName + '.repaired')
        $assembly.Write($stagedPath)
        $verified = Read-Assembly $stagedPath
        try {
            if ((Get-Snapshot $verified $names) -cne $beforeSnapshot) {
                throw "Unrelated metadata or IL changed in $fileName. The project DLL has not been overwritten."
            }
            $verifiedCount = 0
            foreach ($type in (Get-AllTypes $verified.MainModule.Types)) {
                foreach ($method in $type.Methods) {
                    if (-not $names.Contains($method.FullName)) { continue }
                    $instructions = $method.Body.Instructions
                    if ($method.IsPInvokeImpl -or $instructions.Count -ne 3 -or
                        $instructions[0].OpCode.Name -ne 'ldstr' -or
                        $instructions[1].OpCode.Name -ne 'newobj' -or
                        $instructions[1].Operand.FullName -ne 'System.Void System.NotSupportedException::.ctor(System.String)' -or
                        $instructions[2].OpCode.Name -ne 'throw') {
                        throw "Unsupported-binding validation failed: $($method.FullName)"
                    }
                    $verifiedCount++
                }
            }
            if ($verifiedCount -ne $targets.Count) { throw 'Repaired method count mismatch.' }
        } finally { $verified.Dispose() }
        Copy-Item -LiteralPath $stagedPath -Destination $path -Force
        $record.afterSha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        $record.validation = 'Signatures, attribute values and unrelated method IL unchanged; all repaired methods explicitly throw.'
        $planned.Add($record)
    } finally { $assembly.Dispose() }
}

$report = [ordered]@{applied=[bool]$Apply; files=$planned.Count; methods=0; repairs=@($planned)}
foreach ($repair in $planned) { $report.methods += $repair.count }
if ($Apply -and $planned.Count -gt 0) {
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $BackupDirectory 'manifest.json') -Encoding utf8
}
[ordered]@{applied=[bool]$Apply; files=$report.files; methods=$report.methods} | ConvertTo-Json
