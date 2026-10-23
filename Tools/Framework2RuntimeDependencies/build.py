"""Rebuild the pinned real Framework 2 libraries without starting Unity.

The same player-safe DLLs are used by the Editor and Android. No UNITY_EDITOR
code or platform-specific fallback is compiled into the dependency assemblies.
"""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
import subprocess
import tempfile

HERE = Path(__file__).resolve().parent
PROJECT = HERE.parents[1] / 'output_Unity'
NAMES = ('Newtonsoft.Json.dll', 'spine-csharp.dll', 'spine-unity.dll', 'DOTween.dll', 'DOTween.Modules.dll')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unity-editor', default=r'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor')
    parser.add_argument('--output', type=Path)
    parser.add_argument('--apply', action='store_true', help='Replace only the five dependency DLLs; preserve all meta files.')
    parser.add_argument('--smoke', action='store_true', help='Run real JSON and Spine decoding tests with the Unity Mono runtime.')
    args = parser.parse_args()
    editor = Path(args.unity_editor)
    data = editor / 'Data'
    output = args.output or Path(tempfile.mkdtemp(prefix='apple-f2-libraries-'))
    output.mkdir(parents=True, exist_ok=True)
    manifest = json.loads((HERE / 'source-manifest.json').read_text(encoding='utf-8'))
    for entry in manifest['files']:
        path = HERE / entry['path']
        if sha(path) != entry['sha256']:
            raise RuntimeError('Pinned dependency source changed: ' + str(path))
    references = list((data / 'NetStandard/ref/2.1.0').glob('*.dll'))
    references += list((data / 'NetStandard/compat/2.1.0/shims/netfx').glob('*.dll'))
    references += list((data / 'Managed/UnityEngine').glob('UnityEngine*.dll'))
    ui = PROJECT / 'Library/ScriptAssemblies/UnityEngine.UI.dll'
    if not ui.exists():
        raise RuntimeError('Import the project once to build its matching UnityEngine.UI.dll.')
    references.append(ui)
    # Generate Unity API feature defines only, never UNITY_EDITOR or a platform branch.
    defines = ['UNITY_2022', 'UNITY_2022_3', 'UNITY_2022_3_62', 'UNITY_5_3_OR_NEWER']
    for year in range(2017, 2023):
        defines += [f'UNITY_{year}_{minor}_OR_NEWER' for minor in range(1, 5) if year < 2022 or minor <= 3]
    defines += [f'UNITY_5_{minor}_OR_NEWER' for minor in range(3, 7)]
    compiler = [str(data / 'NetCoreRuntime/dotnet.exe'), str(data / 'DotNetSdkRoslyn/csc.dll'), '-noconfig']
    def compile_library(name, folder, extra=(), explicit_sources=None):
        sources = explicit_sources or sorted((HERE / 'Source' / folder).rglob('*.cs'))
        response = output / (name + '.rsp')
        options = ['-target:' + ('exe' if name.endswith('.exe') else 'library'), '-nostdlib+', '-nologo', '-langversion:9.0', '-unsafe', '-optimize+', '-deterministic+',
                   '-out:"' + str(output / name) + '"', '-pathmap:"' + str(HERE) + '=/_/Framework2RuntimeDependencies"',
                   '-define:' + ';'.join(defines)]
        options += ['-r:"' + str(p) + '"' for p in references + list(extra)]
        options += ['"' + str(p) + '"' for p in sources]
        response.write_text('\n'.join(options), encoding='utf-8')
        result = subprocess.run(compiler + ['@' + str(response)], text=True, capture_output=True, encoding='utf-8')
        (output / (name + '.compile.log')).write_text(result.stdout + result.stderr, encoding='utf-8')
        if result.returncode:
            print(result.stdout + result.stderr)
            raise RuntimeError('Compilation failed: ' + name)
        print(name, 'sources', len(sources), 'sha256', sha(output / name))
    shutil.copyfile(HERE / 'Source/Newtonsoft/Newtonsoft.Json.dll', output / 'Newtonsoft.Json.dll')
    shutil.copyfile(HERE / 'Source/DOTween/DOTween.dll', output / 'DOTween.dll')
    compile_library('spine-csharp.dll', 'Spine/spine-csharp')
    compile_library('spine-unity.dll', 'Spine/spine-unity', [output / 'spine-csharp.dll'])
    compile_library('DOTween.Modules.dll', 'DOTween/Modules', [output / 'DOTween.dll'])
    if args.smoke:
        compile_library('DependencySmoke.exe', None, [output / 'Newtonsoft.Json.dll', output / 'spine-csharp.dll'],
                        [HERE / 'DependencySmoke.cs'])
        result = subprocess.run([str(data / 'MonoBleedingEdge/bin/mono.exe'), str(output / 'DependencySmoke.exe'),
            str(PROJECT / 'Assets/Framework2Replica/Resources/Framework2Replica/Sources/TextAsset')],
            text=True, capture_output=True, encoding='utf-8')
        (output / 'smoke-result.txt').write_text(result.stdout + result.stderr, encoding='utf-8')
        print(result.stdout)
        if result.returncode:
            raise RuntimeError(result.stderr or 'Dependency smoke tests failed.')
    record = {'sourceCommit': manifest['sourceCommit'], 'unityEditor': str(editor), 'defines': defines,
              'libraries': [{'name': name, 'sha256': sha(output / name), 'bytes': (output / name).stat().st_size} for name in NAMES]}
    (output / 'build-result.json').write_text(json.dumps(record, indent=2) + '\n', encoding='utf-8')
    if args.apply:
        target = PROJECT / 'Assets/Plugins'
        meta_hashes = {name: sha(target / (name + '.meta')) for name in NAMES}
        for name in NAMES:
            shutil.copyfile(output / name, target / name)
        for name, digest in meta_hashes.items():
            if sha(target / (name + '.meta')) != digest:
                raise RuntimeError('Dependency meta changed: ' + name)
        (HERE / 'build-result.json').write_text(json.dumps(record, indent=2) + '\n', encoding='utf-8')
        print('Applied five genuine runtime DLLs; all five original meta files retained.')
    print('Build output:', output)

if __name__ == '__main__':
    main()
