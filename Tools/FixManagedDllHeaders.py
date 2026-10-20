"""Restore IMAGE_FILE_DLL on recovered managed libraries without rewriting their IL.

Dry-run by default. Use --apply --backup-dir <directory> to repair the headers;
original files and a SHA-256 manifest are kept outside the Unity asset tree.
"""
import argparse
import hashlib
import json
from pathlib import Path
import struct


def inspect(data):
    if data[:2] != b'MZ':
        raise ValueError('Not a PE file')
    pe = struct.unpack_from('<I', data, 0x3c)[0]
    if data[pe:pe + 4] != b'PE\0\0':
        raise ValueError('Invalid PE signature')
    characteristics_offset = pe + 22
    characteristics = struct.unpack_from('<H', data, characteristics_offset)[0]
    opt = pe + 24
    magic = struct.unpack_from('<H', data, opt)[0]
    directories = opt + {0x10b: 96, 0x20b: 112}[magic]
    cli_rva, cli_size = struct.unpack_from('<II', data, directories + 14 * 8)
    if not cli_rva or cli_size < 24:
        raise ValueError('Not a managed library')
    section_count = struct.unpack_from('<H', data, pe + 6)[0]
    opt_size = struct.unpack_from('<H', data, pe + 20)[0]
    sections = opt + opt_size
    for index in range(section_count):
        section = sections + 40 * index
        virtual_size, virtual_address, raw_size, raw_pointer = struct.unpack_from('<IIII', data, section + 8)
        if virtual_address <= cli_rva < virtual_address + max(virtual_size, raw_size):
            cli = raw_pointer + cli_rva - virtual_address
            flags, entry_point = struct.unpack_from('<II', data, cli + 16)
            if entry_point or flags & 0x10:
                raise ValueError('Executable entry point must not be reclassified as a library')
            return characteristics_offset, characteristics
    raise ValueError('CLI header section not found')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--backup-dir', type=Path)
    args = parser.parse_args()
    if args.apply and not args.backup_dir:
        parser.error('--apply requires --backup-dir')
    plugins = Path(__file__).resolve().parents[1] / 'output_Unity/Assets/Plugins'
    repairs = []
    for path in sorted(plugins.glob('*.dll')):
        original = path.read_bytes()
        offset, flags = inspect(original)
        if flags & 0x2000:
            continue
        fixed = bytearray(original)
        struct.pack_into('<H', fixed, offset, flags | 0x2000)
        repair = {'file': path.name, 'offset': offset, 'beforeFlags': flags,
                  'afterFlags': flags | 0x2000, 'beforeSha256': hashlib.sha256(original).hexdigest(),
                  'afterSha256': hashlib.sha256(fixed).hexdigest()}
        if args.apply:
            args.backup_dir.mkdir(parents=True, exist_ok=True)
            backup = args.backup_dir / path.name
            if backup.exists() and backup.read_bytes() != original:
                raise ValueError('Refusing to overwrite a different backup: ' + str(backup))
            backup.write_bytes(original)
            path.write_bytes(fixed)
        repairs.append(repair)
    report = {'applied': args.apply, 'count': len(repairs), 'repairs': repairs}
    if args.apply:
        (args.backup_dir / 'manifest.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'applied': args.apply, 'count': len(repairs)}, indent=2))


if __name__ == '__main__':
    main()
