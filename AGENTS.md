# Instructions to Codex / coding agents

Read `docs/VISION.md` and `docs/MILESTONES.md` before making changes.

## Product goals

- Use the Unity version declared by ProjectSettings/ProjectVersion.txt (currently 6000.6.7f1), C#, URP, PC/keyboard-and-mouse first.
- Prioritize satisfying movement, thoughtful combat, and systemic extensibility over graphics.
- Maintain the core character-identity principle in `docs/VISION.md`.

## Working rules

1. Work **only on the current milestone** unless explicitly asked to expand scope.
2. Prefer the imported, official Unity starter character/controller and Cinemachine setup. Do not rewrite vendor package code unless absolutely necessary; customize through game-owned scripts/components.
3. Keep first-person/third-person package files in their own vendor directory; all new game code goes under `Assets/Game/`.
4. Use clear small C# components and serialized tuning parameters. Avoid global singletons and premature architecture for unimplemented systems.
5. When changing scenes/prefabs, be careful with Unity `.meta` GUIDs and serialized YAML. Prefer changes verified through Unity Editor rather than guessing references.
6. Never introduce new paid Asset Store dependencies without asking.
7. State exactly which files changed, how to configure them in the Editor, and how to test them.
8. Do not claim Unity tests/play mode were run unless Unity was actually available and run.
9. Preserve `.meta` files, `Packages/`, and `ProjectSettings/` in version control; avoid generated `Library/` and `Temp/`.
10. Keep a short list of unresolved gameplay questions rather than treating exploratory ideas as final decisions.

## Current assignment

The user's overnight assignment authorizes V0.4 C1 -> C2 -> C3, with D as a gated stretch. Validate each stage before proceeding. Safe automated Unity testing is authorized; never launch another Editor against an open project or interrupt unsaved user edits. Inspect running processes before exclusive batch tests.

Preserve existing scenes, attack assets, controls, balance, stamina, third-party assets, Packages and ProjectSettings. Keep discoveries distinct from ownership. Do not add unrelated systems. C1 must distinguish real, harmless and ambiguous evasions before discovery depends on it. See docs/OVERNIGHT_PROGRESS_REPORT.md for actual status, executed checks and next steps; no gameplay feel is established by automated checks.
Overnight result: C1/C2/C3 implemented; supported Unity checks passed. Preserve the three separate evidence/discovery/trial scenes and session-only knowledge versus power distinction. D is not implemented: complete focused input regressions and player comparison before actual investment/save work. Never rebalance stamina as a workaround for prototype feedback.
