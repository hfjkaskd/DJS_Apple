# Fruit-style withdrawal interfaces

The existing gameplay TEST money button now opens the fruit-style withdrawal pages. The Home entry stays removed. The original gameplay board and its controls have not been moved.

## Saved interfaces

The eleven prefabs in `Assets/Resources/WithdrawalImported/Panels` keep their original resource paths and metadata so the existing `WithdrawalHost` and `GameResCatalog` resolve the new interfaces directly:

- Cash and diamond wallets, reward claim, destination selection and confirmation.
- Published rules, conditions, timed review, completed or returned result, and scrollable records.

Both currencies use the existing configured tiers, rewards, conditions and storage. These are explicitly labelled local test simulations with no cash value. No account details or payment API have been added.

Wallet → channel → confirm → conditions → review → result → records is connected to the existing state engine. Rules returns to the page it was opened from; confirmation returns to channel selection. Cancelling or using **Test reject** during review returns the reserved units. Review completion is automatic. Pending rewards are only claimed by the Claim button.

## Editing and resources

Use the saved prefabs for normal art/layout edits. `Tools > Withdrawal > Build Fruit Style Interfaces` deliberately rebuilds all eleven page assets from `WithdrawalFruitUIAuthoring.cs`; the older Build Imported Flow menu forwards to the same fruit authoring recipe.

- The original fruit font, cream/orange popup and glossy buttons are reused. A separate `FruitTitle.mat` supplies the title outline without changing the original font material.
- `Assets/WithdrawalFruitArt` contains transparent generated cash, diamond, bank and hourglass icons, imported at a maximum of 512 pixels without mipmaps. The icons were generated individually using the built-in image generator from the approved fruit-game concept style; the original generated PNGs are preserved.
- All static hierarchy, styling, references and Button targets are serialized. Runtime code binds standard Buttons, loads the requested saved page, updates texts/visibility, and changes sliced progress fill width. There is no runtime construction of static UI and no Editor-only initialization fallback.
- The modal shade uses the game's existing `ProtectedAreaAdaptReverse`; it covers the whole screen while page content stays inside the host's safe area.
- Page art is loaded with its Resources page on demand. The gameplay entry only references its small cash icon; it does not reference the other modal pages or the large validation background.

## Validation

`Tests/Withdrawal/Run-CoreTests.ps1` validates reward/state/storage behavior. `Run-PresentationTests.ps1` runs the production presentation/controller code against an in-memory Unity presentation stub and the real state engine, without accessing player saves.

`WithdrawalFruitUIAuthoring.BuildAndRender` compiles in Unity, saves the prefabs and runs `WithdrawalUIRenderValidation.Run`. The renderer checks the actual catalog → gameplay entry → host → eleven page mappings, Button callback dispatch, fonts, references, text overflow, and record scrolling at 1080×1920, 1080×2340 and 720×1600. It writes 36 static renders and a JSON report to `C:/Projects/DJS_Apple/withdrawal-fruit-ui-validation`.

These renders use the real prefabs and native Map1008 background with fixed test data and the same safe-area inset/expansion helpers as the game. They are presentation checks, not screenshots of an actual player session or payment. Android/iOS device verification is still separate.
