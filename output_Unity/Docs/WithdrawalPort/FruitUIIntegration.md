# Fruit-style withdrawal interfaces

The gameplay header now shows separate cash and diamond cards with their current simulated balances. Each green Cash button opens the matching wallet; the small Rewards button opens unclaimed rewards. The former gameplay coin/purchase entry and bottom TEST shortcut are removed. The Home withdrawal entry stays removed. The gameplay board and tool positions are unchanged.

## Top currency HUD and inventory-only tools

`WithdrawalHud.prefab` contains the two currency cards, standard Buttons, live amount labels, reward entry and explicit test-state label. `WithdrawalHud.Bind`/`Unbind` own the Changed subscription and code-only click listeners. Claiming rewards, reserving a request and returning it all refresh the displayed balances from the existing withdrawal service. No extra balance storage or real payment integration is introduced. An unavailable service disables the entries and clears stale amounts.

The old gameplay `CommonCoinBtn` is inactive and the controller's serialized reference is cleared, so level transitions cannot reactivate it. Four tool roots now contain standard Unity Buttons targeting their visible child artwork; their old empty raycast graphics are disabled. Tools consume existing inventory only. Zero stock displays 0 and disables use; it cannot spend the legacy coins or open the shop. Free grants and unlock rewards remain available. Legacy coin plus signs and purchase callbacks are removed; paid shop items and offer buttons are hidden.

`Tools > Withdrawal > Build Top Currency HUD` (Ctrl+Alt+H) saves the HUD and validates it outside Play mode. Its static authoring recipe is `Assets/Editor/WithdrawalHudAuthoring.cs`; the full reference interface builder forwards to it. `Tools/InstallCurrencyHud.py` performs bounded, idempotent edits to the gameplay prefab: old coin activation/reference, nested HUD overrides and four Button components. It preserves unrelated gameplay YAML documents.

Verification: 20 presentation/HUD tests and 7 purchase-removal tests pass. `WithdrawalHudValidation.Run` rendered 1080x1920, 1080x2340 and 720x1600 with no reported overlap, overflow, missing binding or active legacy-coin issues. Images and the report are in `C:/Projects/DJS_Apple/withdrawal-hud-validation`. These image fixtures use fixed sample balances; the live game reads the current local test balance.

The open Unity Editor also ran Level 4 with the saved gameplay prefab. Both header buttons opened their corresponding cash/diamond wallets, and closing each returned to the game. The original coin entry was absent from the gameplay header.

## Saved interfaces and flow

The eleven prefabs in `Assets/Resources/WithdrawalImported/Panels` retain their resource paths and metadata. `WithdrawalHost` and `GameResCatalog` continue to resolve the saved pages:

- Cash and diamond wallets, reward claim, channel selection and confirmation.
- Published rules, conditions, timed review, completed or returned result, and scrollable records.

Wallet → channel → confirm → conditions → review → result → records uses the existing state engine. Rules returns to its source page and request; confirmation returns to channel selection. Viewing another request does not replace the wallet's selected tier. Pending rewards remain pending until Claim is pressed, including after closing and reopening the page.

Both currencies retain the configured rewards, tiers, conditions and binary storage. Every page displays `TEST MODE • NO CASH VALUE`. No real payment API, account collection or fabricated real payout has been added. Review completes automatically when the existing engine countdown expires. The review interface provides Back to game, Records and Cancel request; it has no visible test-reject or manual-completion button. Cancelling returns reserved units according to the unchanged engine rules.

## Measured authoring and assets

Use the saved prefabs for ordinary art/layout edits. `Tools > Withdrawal > Build Measured Reference Interfaces` calls `Assets/Editor/WithdrawalReferenceUIAuthoring.cs`. Its source rectangles are measured from the approved concept boards and converted to a 1080-unit design canvas before saving the eleven pages. The older Build Fruit Style Interfaces and Build Imported Flow menus forward to this recipe.

- `Assets/WithdrawalReferenceArt` contains the reference-derived panel, title, buttons, cards, cash, gem and supporting icons. The generated PNG pixels are preserved; authoring selects the Unity Sprite region and border slicing. Existing bank/hourglass art remains in `Assets/WithdrawalFruitArt`.
- Display text uses `Assets/WithdrawalReferenceArt/Fonts/LilitaOne-Regular.ttf`; body text uses `BarlowCondensed-SemiBold.ttf` in the same directory. Their supplied SIL Open Font License 1.1 notices are retained as `LilitaOne-OFL.txt` and `BarlowCondensed-OFL.txt` alongside the fonts.
- `ReferenceDisplay.asset` and `ReferenceBody.asset` contain the authored static TMP atlases. The authoring recipe includes basic Latin and the bullet used by the test label. Runtime does not create or populate font assets. `Assets/WithdrawalReferenceArt/ReferenceWhiteOutline.mat` provides the shared title/button outline.
- Static hierarchy, styles, references and standard Button targets are serialized into Prefabs. Runtime loads a configured saved page, binds callbacks, refreshes text/visibility, and updates sliced progress widths. It does not construct static UI hierarchies or use reflection to invoke methods/fields. There is no Editor-only initialization fallback.

`WithdrawalPageView.UseCanvasFrame` scales content against the full root canvas and the configured design size. `ArtworkBounds` limits the actual art to the safe root's available dimensions. Full-canvas centering preserves the reference position; only art crossing a safe edge receives the minimum corrective translation. The cached canvas is refreshed on parent/canvas hierarchy changes, with a reentry guard and no per-frame search. The original `ProtectedAreaAdaptReverse` still expands the modal shade to cover the full screen.

The conditions page groups level milestones, daily wins, qualified days and rank. Zero-target groups are hidden; the reserved amount remains disclosed above the tasks. Values such as rank 49 / 31 retain actual progress. Rules includes all configured requirements before submission. Wallet tier labels omit decimals only for whole-dollar targets, while amounts such as $0.40 retain two decimal places. Record rows use short dates and single-line statuses, with a green dot only for simulated completion and an orange dot for other records.

## Verification scope

`Tests/Withdrawal/Run-CoreTests.ps1` previously passed **10 domain/storage tests**; the domain/storage code is unchanged by the HUD update. `Run-PresentationTests.ps1` now passes **20 presentation/HUD checks** using the production Host/PageView/Presentation/Hud code, an in-memory Unity presentation stub, and the real state engine. The harness does not access player saves. Coverage includes full-canvas centering, safe-area fitting and minimal translation, cached layout lookup, sliced fill behavior, reward previews, synchronous Changed callbacks, Rules return paths, request-history isolation, grouped requirements, status indicators, short copy, automatic review completion, currency balance changes and HUD listener lifecycle.

The three runtime presentation files have also compiled against the installed Unity Core/UI/TMP assemblies without launching the Editor. This is a code check, not device verification.

`WithdrawalReferenceUIAuthoring.BuildAndRender` saves the pages and invokes `WithdrawalUIRenderValidation.Run`. The current recipe covers **11 pages and 48 requested renders**: 1080×1920, 1080×2340, 720×1600 and each page's measured reference aspect, plus the bottom of the records scroll view. It checks catalog → gameplay entry → host → page references, standard Button callback dispatch, static fonts, text overflow, safe-area placement, shade coverage and scrolling. Results are written to `C:/Projects/DJS_Apple/withdrawal-fruit-ui-validation/withdrawal-ui-report.json` with the page images.

The final Unity batch run at `2026-10-09T08:34:46.1752339Z` passed all **48 renders and 276 Button callback dispatches**, with no reported structural, binding, missing-font, text-overflow or safe-area errors. Fixtures use the real saved prefabs and native Map1008 background with synthetic display data. They are not an actual player session or proof of payment. Android/iOS device verification remains separate.

The original three concept boards and the actual reference-aspect renders are compared in `C:/Projects/DJS_Apple/withdrawal-reference-comparison/index.html`. The gallery preserves aspect ratios and labels the drawn concept background separately from the native-game validation background. Layout bounds are measured from the references; generated art and available licensed fonts remain a reconstruction, not a claim of identical source pixels. Source and font provenance is recorded in `Assets/WithdrawalReferenceArt/ASSET_SOURCES.md`.
