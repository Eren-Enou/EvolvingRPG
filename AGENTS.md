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

Latest follow-up: the user requested an extremely basic test of the new ideas. A standalone design-model smoke script at `tools/DesignChecks/DepthIdeaSmokeChecks.ps1` is authorized and has run; results and limits are in `docs/DEPTH_IDEA_SMOKE_RESULTS.md`. This does not authorize launching Unity or changing runtime systems. Keep actual gameplay validation distinct from these small model checks.

Latest steering: **prioritize depth over breadth in the proposed paths**. Read `docs/DEPTH_FIRST_DESIGN.md` and `docs/HEAVY_DISCIPLINE_DEPTH_STUDY.md`. Prefer one Heavy lineage developed through familiar contexts, with the other existing technique as an independent comparison. Keep new Light branches, abilities, gear effects, weapon families and enemy roles dormant until a specific missing decision justifies them. This remains documentation-only work; do not run Unity, add runtime systems or tune stamina.

The latest user request authorizes a **broad documentation-only design expansion without running Unity**: develop the game's beginning, combat evolution, missions, specialization, equipment, world growth and independently testable paths. Start at `docs/GROWTH_DESIGN_ATLAS.md`; consult `docs/DEVELOPMENT_EXPERIMENT_BACKLOG.md` before selecting future implementation and `docs/DESIGN_REVIEW_REGISTER.md` for open decisions. These are proposals, not implemented features or permission to run Unity/tests in this pass. Do not add runtime code or alter scenes for this documentation assignment. Preserve the user's talent charter and VISION. V0.4 B remains the current playable implementation, and stamina tuning remains deferred.

Current playable baseline: **V0.4 B — Heavy practice trial** uses completed Heavy actions in meaningful opponent exchanges, explicit credit/rejection diagnostics, duplicate/reset protection, and one optional bounded recovery trial in a separate `PracticeTest` scene. Preserve earlier scenes, authored technique assets, existing controls, player/enemy balance, stamina costs, and regeneration. Technique usefulness and all mastery thresholds/formulas remain proposals awaiting player feedback. The user needs frequent actions for interruption testing; do not tune stamina yet. Talent unlocks, discoveries, a full tree, account power, equipment, loot, saves, multiplayer and gameplay menus are not implemented by this baseline or authorized for runtime implementation during the current design pass. See `docs/HEAVY_PRACTICE_EXPERIMENT.md`, `docs/TECHNIQUE_EXPERIMENT.md`, and `docs/ENEMY_SANDBOX.md`.
