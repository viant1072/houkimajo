# Repository Guidelines

## Project Overview

Houkimajo is a C# 2D arena bullet-hell prototype using Unity **6000.6.5f1** and Universal 2D (URP). The current slice includes a main menu, three data-driven stages, enemy waves, multi-phase bosses, Q/E/R skills, a prototype shop, saves, story dialogue, and authored sprites/animations.

Treat `docs/` as design specifications, not proof that every planned feature exists. XP/level progression, general equipment/inventory, and skill unlocking are still planned; the current progression implementation covers gold, HP potions, and purchased Q/E/R upgrades.

## Project Structure

- `Assets/_Game/Scripts/`: `Player`, `Bullets`, `Boss`, `Enemies`, `Skills`, `Progression`, `Systems`, and `UI` modules.
- `Assets/_Game/Editor/`: scene setup, targeted asset builders, and gameplay checks. Keep editor-only code here.
- `Assets/_Game/Prefabs/`, `Data/`, `Art/`, `Audio/`: authored resources. `Data/` contains ScriptableObjects; `Art/Sprites/` separates Player, Enemies, Bosses, Bullets, Skills/Q/E/R, Backgrounds, UI, and placeholders.
- `Assets/_Game/Scenes/MainMenu.unity` and `Stage01.unity`: menu and shared gameplay scene. Three stage configurations live in `Data/Stages/`; they are not three separate scene files.
- `Assets/_Game/SETUP.md`: detailed Unity setup and integration instructions; verify current behavior against code when older sections disagree.
- `docs/`: Vietnamese module specs and `coding-rules.md`. Update the relevant spec before major changes, using Goal / Requirements / Non-goals / Open questions. `story.md` covers narrative.
- `output/`: prompts, production manifests, screenshots, previews, and validation reports.
- `RawAssets/`: categorized copies of assets, Unity metadata, concepts, previews, and documentation; `index.html` is the local asset catalog.
- `Packages/` and `ProjectSettings/`: Unity dependencies and configuration. Keep custom gameplay work under `Assets/_Game/`; avoid unrelated template edits.

## Running and Controls

Open the project through Unity Hub with the version in `ProjectSettings/ProjectVersion.txt`. Let compilation finish, then press Play. **Houkimajo > Start Play Mode From Main Menu** is enabled by default for the editor session; disable it to play the open gameplay scene directly.

- WASD/arrows move; mouse aims; hold **left mouse button** to shoot.
- Hold **Left Control** for focus; **Left Shift** triggers the separate default dash.
- **Q/E/R** cast the equipped skills; **B** toggles the prototype shop; **1** uses an HP potion.
- **Escape** opens the pause menu during gameplay or closes the active shop/pause layer.
- Editor-only: **F1/F2/F3** cycle Q/E/R skills, **F4** fills ultimate energy, **F5** cycles equipped skill levels.

Use **Houkimajo > Setup Playtest Scene (Placeholder Art)** only when setting up missing playtest content. The historical menu name remains even though builders now apply authored art. **Rebuild Playtest Scene** replaces generated scene content; prefer targeted Apply menus for an existing authored scene. Save scene changes explicitly with **Ctrl+S**; some broader stage builders also save scenes themselves.

Use **File > Build Profiles** for player builds. There is no repository-wide CLI build/test wrapper.

## Asset and Catalog Workflow

- Edit gameplay resources in `Assets/_Game/`; `RawAssets/` is an archive/catalog copy, not the runtime source of truth.
- Keep sprite `.meta` files with their PNGs: they preserve GUIDs, sliced rects, pivots, and PPU. Never import the archive wholesale into `Assets/` with duplicate GUIDs.
- Player flight and casting sheets share alignment. Shooting raises the hand once and holds the final casting frame until firing stops; do not reintroduce a looping hand-raise or reset it per bullet.
- Keep visual animation on child renderers so it cannot move the gameplay collider. Pool reused combat effects and reset their state on reuse.
- Open `RawAssets/index.html` in a browser to browse previews, sprite regions, animations, and selections. It uses local HTML/CSS/JS with no frontend dependencies.
- Refresh archive copies and SHA256 manifest from the project root: `pwsh -File ./RawAssets/Sync-RawAssets.ps1` (PowerShell 7). This copies without deleting and regenerates catalog data when Node.js is available.
- Refresh catalog data only: `node RawAssets/build-catalog.mjs`. Do not hand-edit `catalog-data.js`; it is generated from `manifest.csv` and copied `.meta` files.
- Catalog syntax check: `node --check RawAssets/catalog.js`. Verify UI behavior in a browser when changing interactions.

## Coding Style and Architecture

Follow `docs/coding-rules.md`: four-space indentation, braces on separate lines, one class/interface per matching filename. Use `PascalCase` for types, methods, properties, and public fields; `camelCase` without prefixes for private/protected fields. Match namespaces to modules, e.g. `Houkimajo.Bullets`.

Use English identifiers and Vietnamese comments explaining intent. Prefer `[SerializeField] private` fields with useful tooltips. Cache references in `Awake()`. Input is read directly from `Keyboard.current` / `Mouse.current` through the Input System package. No formatter or linter configuration is committed.

Pool runtime bullets, enemies, skill projectiles, and pickups; avoid per-shot Instantiate/Destroy and allocations in hot paths. Keep patterns, boss phases, skills, enemies, waves, and stages data-driven through ScriptableObjects. Use events for UI updates. Preserve existing serialized fields, SO IDs, and save compatibility when extending behavior.

## Validation

Unity Test Framework is installed and targeted editor checks exist, but no unified automated suite or coverage threshold is established. Use checks relevant to the change and actual Play mode for gameplay changes: movement, aim, shooting/casting, focus/dash, Q/E/R, collisions, pooling, HUD, shop/potions/upgrades, pause, story, boss transitions, and victory/game-over.

Record what was actually tested and any verification limits. Include screenshots/clips for visual changes. Keep temporary validation scenes, camera overrides, test save data, and local editor settings out of authored gameplay changes. `SaveSystem` writes `save.json` under `Application.persistentDataPath`; isolate validation saves when needed.

## Commits and Pull Requests

Use descriptive, focused commit subjects; no strict subject prefix is required. PRs should explain behavior changes, link relevant specs/issues, and list validation. Commit asset `.meta` files alongside assets. Exclude generated `Library/`, `Temp/`, `Logs/`, builds, and IDE project files. Preserve unrelated work in the shared workspace.
