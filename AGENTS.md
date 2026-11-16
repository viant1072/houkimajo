# Repository Guidelines

## Project Structure & Module Organization

Houkimajo is a C# 2D bullet-hell prototype using Unity **6000.6.5f1** and Universal 2D (URP).

- `Assets/_Game/Scripts/`: gameplay modules (`Player`, `Bullets`, `Boss`, `Systems`, `UI`).
- `Assets/_Game/Prefabs/`, `Data/`, `Art/`, `Audio/`: authored assets; `Data/` contains ScriptableObject configurations.
- `Assets/_Game/Scenes/Stage01.unity`: playtest scene; `Editor/SceneBootstrapper.cs` builds its placeholder content.
- `docs/`: Vietnamese module specifications and `coding-rules.md`. Write or update the relevant spec before major changes, using Goal / Requirements / Non-goals / Open questions.
- `Packages/` and `ProjectSettings/`: Unity dependencies and configuration. Keep custom work under `Assets/_Game/`; avoid unrelated template edits.

## Build, Test, and Development Commands

There are no repository build scripts or CLI wrappers. Use Unity Hub to open the project with the version in `ProjectSettings/ProjectVersion.txt`.

- Open `Stage01.unity`, then run **Houkimajo > Setup Playtest Scene (Placeholder Art)** to create test content.
- Use **Houkimajo > Rebuild Playtest Scene** when generated content needs refreshing; save explicitly with **Ctrl+S**.
- Press **Play** to run locally: WASD/arrows move, mouse aims, Space fires, Shift focuses.
- Use **File > Build Profiles** for player builds; no automated build entry point is defined.

## Coding Style & Naming Conventions

Follow `docs/coding-rules.md`: four-space indentation, braces on separate lines, one class/interface per matching filename. Use `PascalCase` for types, methods, properties, and public fields; `camelCase` without prefixes for private/protected fields. Match namespaces to modules, e.g. `Houkimajo.Bullets`.

Use English identifiers and Vietnamese comments explaining intent. Prefer `[SerializeField] private` fields with useful tooltips. Cache references in `Awake()`. No formatter or linter configuration is committed.

Pool runtime bullets, enemies, skill projectiles, and pickups. Keep patterns and boss phases data-driven through ScriptableObjects; use events for UI updates and avoid allocations in hot paths.

## Testing Guidelines

Unity Test Framework is installed, but no automated suite or coverage threshold exists. Validate changes in Play mode with a clean Console: movement, aiming, firing, focus, collisions, scoring, boss transitions, and victory/game-over. Record results in the PR. If adding automated tests, use Unity Test Runner and descriptive names such as `BulletPoolTests.cs`.

## Commit & Pull Request Guidelines

The short history uses descriptive English and Vietnamese subjects; no strict prefix convention exists. Describe the concrete change and keep commits focused.

PRs should explain behavior changes, link relevant issues/specs, list validation, and include screenshots or clips for visual changes. Commit asset `.meta` files alongside assets. Exclude generated `Library/`, `Temp/`, `Logs/`, builds, and IDE project files.
