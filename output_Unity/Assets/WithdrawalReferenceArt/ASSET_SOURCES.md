# Withdrawal reference UI asset sources

Last verified: 2026-10-09.

## Concept references

The project-specific concept boards were generated with the built-in image generator for the fruit game's simulated withdrawal interface. They use the existing game's blue sky, blossom tree, cream/orange panels and glossy green buttons as visual references. They depict test balances and simulated outcomes, not real payments.

Original concept directory: `C:/Projects/DJS_Apple/withdrawal-ui-concepts-20261008/`.

| Board | Screens used as references |
| --- | --- |
| `01-rewards-and-wallets.png` | Game entry, fruit rewards, cash wallet and diamond wallet |
| `02-application-and-progress.png` | Channel selection, confirmation, rules and application progress |
| `03-review-results-and-history.png` | Simulated review, completion, returned units and records |

All three boards are 1672 x 941 pixels. `generation-prompts.json` in the same directory records the original board prompts and input references. Those references include the project's `Map1008`, `PopBg`, `Btn_Normal` and `Btn_Orange` artwork. The boards are design references; they are not loaded by the game.

## Generated PNG assets

The PNGs in this directory are individual image-generator outputs redrawn from the above references. Each saved PNG preserves the selected generation/edit output; the source raster was not programmatically cropped, resized or repainted. Transparent regions, including the clean panel exterior, were produced by the image generator.

| Files | Reference and intended use |
| --- | --- |
| `Panel.png` | Original generated blank cream/orange panel draft; retained for provenance, unused by the current withdrawal page prefabs |
| `PanelClean.png` | Image-generator edit of `Panel.png` removing exterior amber glow and brown haze; the panel used by all eleven current page prefabs |
| `Title.png`, `PrimaryButton.png`, `SecondaryButton.png`, `InsetCard.png` | Blank title, action and inner-card surfaces matching the concept boards |
| `SelectedCard.png` | Blank cream card with a thin green selection border, based on the selected wallet tiers in board 01; no baked text or check badge |
| `CashBundle.png` | Compact horizontal green banknote bundle with a gold band, based on the cash-wallet icon in board 01 |
| `Gem.png` | Single pink diamond with a few large facets, based on the diamond-wallet icon in board 01 |
| `CloseButton.png`, `CheckBadge.png` | Orange circular X button and green circular check badge from board 01; their symbols are part of the image |
| `Wallet.png` | Green folding wallet with a gold snap, based on the channel page in board 02 |
| `Trophy.png`, `Calendar.png`, `Crown.png`, `Clock.png` | Condition/rule icons matching the corresponding board 02 rows |
| `GoldenSparkle.png` | Small gold reward/completion decoration matching the concept style |
| `ReturnArrow.png` | Orange curved left-pointing return arrow, based on the returned-units page in board 03 |
| `RewardBurst.png` | Separate translucent golden reward rays based on board 01; the selected refined output has a nearly transparent center and contains no money, gem or text |

Additional existing references used by the authoring tool are `Assets/WithdrawalFruitArt/Bank.png` and `Hourglass.png`, generated earlier for this fruit-style UI, plus the original game's `Star.asset`, `MidBtn_Green.asset` and `MidBtn_Gray.asset`. These remain at their existing paths.

## Fonts and licenses

The source TTFs come from the official Google Fonts repository. Both local TTFs were compared with the linked upstream bytes on the verification date and matched exactly. Their SIL Open Font License 1.1 files are preserved alongside them.

| Local source | Role | Official source | Included license |
| --- | --- | --- | --- |
| `Fonts/LilitaOne-Regular.ttf` | Display headings, primary labels and amounts | [Google Fonts raw TTF](https://raw.githubusercontent.com/google/fonts/main/ofl/lilitaone/LilitaOne-Regular.ttf) | `Fonts/LilitaOne-OFL.txt` ([upstream OFL](https://raw.githubusercontent.com/google/fonts/main/ofl/lilitaone/OFL.txt)) |
| `Fonts/BarlowCondensed-SemiBold.ttf` | Body copy and compact labels | [Google Fonts raw TTF](https://raw.githubusercontent.com/google/fonts/main/ofl/barlowcondensed/BarlowCondensed-SemiBold.ttf) | `Fonts/BarlowCondensed-OFL.txt` ([upstream OFL](https://raw.githubusercontent.com/google/fonts/main/ofl/barlowcondensed/OFL.txt)) |

Source SHA-256 values:

```text
LilitaOne-Regular.ttf
f5b641c45c69d772ee4eda687bc9fda411d5cad6b0b45371491da4580cbc8d59

BarlowCondensed-SemiBold.ttf
7b619d14bc2327509a9ef32b0890f709626f7ecc9ff61191c2a4314c5499d2d9
```

`Fonts/ReferenceDisplay.asset` and `Fonts/ReferenceBody.asset` are the saved TextMesh Pro SDF assets created from these TTFs, with static 1024 x 1024 atlases. `ReferenceWhiteOutline.mat` supplies the white face and brown outline for designated display text. Keep the accompanying OFL notices with distributed font assets.

## Unity import and runtime use

`Assets/Editor/WithdrawalReferenceUIAuthoring.cs` owns the import rectangles, nine-slice borders, fonts and saved page layout. Sprite import rectangles trim transparent padding without changing the PNG pixels. Some sprites use alpha-bound discovery during authoring; the saved imports are non-readable at runtime. No runtime image cropping or static UI construction is needed.

Current maximum imported texture dimensions are:

| Maximum | Assets |
| --- | --- |
| 128 | `GoldenSparkle` |
| 256 | `Trophy`, `Calendar`, `Crown`, `Clock`, `CloseButton`, `CheckBadge` |
| 512 | `CashBundle`, `Gem`, `Wallet`, `ReturnArrow` |
| 1024 | `Title`, `PrimaryButton`, `SecondaryButton`, `InsetCard`, `SelectedCard`, `RewardBurst` |
| 2048 | `PanelClean`; the unused `Panel` draft also retains a 2048 import limit |

These reference imports preserve alpha, disable mipmaps and use the authoring tool's current uncompressed texture setting. Import limits affect Unity's built textures, not the original PNG file dimensions. The larger surfaces are referenced by their saved page prefabs; the host selects pages through `Resources` paths instead of holding every page or concept board as a startup reference.
