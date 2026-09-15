# Harvest UI fonts

These fonts replace the angular 802-CAI978 lettering in the Harvest UI only.

- **Title and large buttons:** Lilita One, baked as `HarvestTitleRound SDF.asset`. Titles use `HarvestTitleRound WhiteOutline.mat` with a brown outline; white labels on green buttons use `HarvestTitleRound GreenOutline.mat` with a green outline (`#1c741f`, width `0.20`). Dark labels use `HarvestTitleRound Plain.mat`.
- **Body and amounts:** Nunito fixed at weight 800 (ExtraBold), renamed to Harvest Body Round and baked as `HarvestBodyRound SDF.asset`. Use `HarvestBodyRound Plain.mat` with the Prefab's text color. The baked face already supplies the weight; additional faux bold is disabled.

`HarvestFontAuthoring.BuildAll()` or **Tools > Harvest > Bake Static Fonts** authors the two font assets. Each contains one 1024 × 1024 Alpha8 atlas and its default material as sub-assets. The four separate shared material presets point to those same atlases. Sampling size is 90, SDF padding is 8, and render mode is SDFAA. The 114-character set includes every printable ASCII character, the bullet, curly quotes, en/em dashes, ellipsis, and common currency/symbol punctuation. Missing glyphs or a second atlas cause authoring to fail. Completed assets use static population and have no runtime source-font reference. Source TTFs are in the `Editor` folder; runtime never creates a font atlas.

The two atlases occupy about 2 MiB of uncompressed texture data, before Unity's CPU-side texture storage and object overhead. There is no need to bake the full font character sets or initialize a dynamic atlas during play. Title outlining is a shared material setting; layout, text color, point size and alignment remain in Prefabs.

On successful baking, `font-bindings.json` records GUID/fileID pairs for the two fonts and four presets. Re-baking preserves existing font, atlas and material GUID/local IDs. Call `BakeFont(sourcePath, assetPath, familyName, pointSize, padding, atlasWidth, atlasHeight)` for explicit authoring overrides. This utility does not edit gameplay Prefabs or player state.

## Sources and license

Downloaded from the official Google Fonts repository on 2026-09-15:

- [Lilita One source](https://github.com/google/fonts/tree/main/ofl/lilitaone): `LilitaOne-Regular.ttf`, unmodified source file; SHA256 `f5b641c45c69d772ee4eda687bc9fda411d5cad6b0b45371491da4580cbc8d59`.
- [Nunito source](https://github.com/google/fonts/tree/main/ofl/nunito): `Nunito[wght].ttf`, stored without byte changes as `Editor/Source/Nunito-Variable.ttf`; SHA256 `bb55a5ca5c2042335b3991af27c4d0705d0ef41cac6164ac737fd8f2a1e85207`.
- `HarvestBodyRound-ExtraBold.ttf` is a modified Nunito instance at `wght=800`, with family/full/PostScript names changed. Reproduce it with `Editor/InstantiateBodyFont.py` and fontTools. The Nunito copyright and OFL metadata remain embedded.

Both font files declare SIL Open Font License 1.1, and their OS/2 embedding flag is `fsType=0`. Full copyright/license notices are included in `Resources/HarvestFontLicenses`, so Unity includes them with the build. Retain these notices in distributions. Lilita reserves its font name; the derived SDF uses the separate family name `Harvest Title Round`. The fonts and derivatives remain under OFL; they are not sold independently. No Windows system font has been copied.

Atlas generation and visual inspection must be performed in Unity after import. The authoring source's standalone C# compile does not prove that rasterization has completed or that layout matches the reference image.
