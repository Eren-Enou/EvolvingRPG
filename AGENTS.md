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

Continue the user's character-ownership/progression goal, after validation gates. On 2026-11-05 the existing focused Editor combat suite (26 cases) and full technique suite including number-row event checks passed. These are technical results, not player feel validation.

Implement the smallest isolated V0.4 D ownership/spending slice, then minimal versioned persistence only after ownership checks pass. Preserve earlier comparison scenes, controls, attack assets, stamina, third-party code, Packages and ProjectSettings. Replace the empty defensive investment prerequisite with verified defensive experience in the owned-character scene; do not sell an empty support talent. Discovery, ownership and activation remain distinct.

Safe Unity tests are authorized. Never launch a second Editor against an open project, interrupt a user Play session, or overwrite unsaved edits. New scene authoring must use explicit Editor APIs. Report exact checks, limitations, commits, launch steps and future decisions in docs/CONTINUED_DEVELOPMENT_REPORT.md. Experimental budgets and specialization restrictions are not final release rules.