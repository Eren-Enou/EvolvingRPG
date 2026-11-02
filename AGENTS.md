# Instructions to Codex / coding agents

Read `docs/VISION.md` and `docs/MILESTONES.md` before making changes.

## Product goals

- Unity 6.3 LTS, C#, Universal Render Pipeline (URP), PC/keyboard-and-mouse first.
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

Implement **V0.2 — Combat foundations** according to `docs/MILESTONES.md` and the user's explicit combat-sandbox assignment. Movement, jumping, camera controls, and Q/E strafing have been player-tested. Preserve that foundation and the movement-only scene. Add separate Light/Heavy actions, a directional dodge, basic stamina, and a stationary resettable health/poise target in a separate combat sandbox. Do not add enemy AI, talents, equipment, loot, multiplayer, or gameplay menus. See `docs/COMBAT_SANDBOX.md` for controls, configuration, validation, and open decisions.
