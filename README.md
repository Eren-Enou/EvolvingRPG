# EvolvingRPG

A Unity third-person action-RPG prototype built around positioning, commitment, and a character whose identity develops through experience and deliberate choices.

> A character is not a finished build to chase. It is something the player continuously discovers, in a world that evolves alongside them.

This checkout is an existing Unity project. Use the engine version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt), currently **6000.6.7f1**. Do not recreate the project from the old scaffold instructions or change versions to match historical milestone text.

## Current prototype

| Scene under Assets/Game/Scenes | Purpose |
| --- | --- |
| MovementTest | Third-person movement, jumping, strafing and camera comparison |
| CombatTest | Light/Heavy, directional dodge and stationary training target |
| EnemyTest | Guarded/fast opponents, health, poise and encounter reactions |
| TechniqueTest | Baseline, Breaker and Momentum Heavy debug comparisons |
| PracticeTest | Latest playable slice: meaningful Heavy practice and optional recovery trial |
| EvasionEvidenceTest (manual builder) | C1 diagnostic-only evasion evidence; Editor validation pending |

For future playtesting, open the desired scene in the existing Editor, enter Play mode and focus the Game view. See [Heavy practice instructions](docs/HEAVY_PRACTICE_EXPERIMENT.md) for the latest slice's controls, tuning, limitations and previously recorded functional results. The technique/practice formulas remain proposals; player testing establishes feel.

The current assignment is **P1 / V0.4 C1 meaningful evasion evidence only**. See [the experiment guide](docs/EVASION_EVIDENCE_EXPERIMENT.md) for the manual scene-builder and check menus. Runtime and Editor sources pass offline C# compilation; the user must run Editor import/Play-mode/physics checks and interactive validation. Unity was not launched or controlled for this implementation. Stamina remains intentionally permissive and unchanged.

## Design and development

The latest design focus is **depth within a selected discipline**. Read [Depth-first design](docs/DEPTH_FIRST_DESIGN.md) and [the Heavy discipline study](docs/HEAVY_DISCIPLINE_DEPTH_STUDY.md) before promoting more features from the broader design inventory.

Start at the [Growth Design Atlas](docs/GROWTH_DESIGN_ATLAS.md) for the game's proposed opening, missions, combat evolution, specialization, world growth, equipment/abilities and UI. It links the detailed specifications and distinguishes proposals from implemented systems.

- [Vision](docs/VISION.md) — enduring direction.
- [Talent charter](docs/EVOLVING_TALENT_SYSTEM.md) — established long-term principles and unresolved mechanics.
- [Milestones](docs/MILESTONES.md) — actual implementation scope/status.
- [Progression development path](docs/PROGRESSION_DEVELOPMENT_PATH.md) — existing foundations and progression boundaries.
- [Experiment backlog](docs/DEVELOPMENT_EXPERIMENT_BACKLOG.md) — narrow future slices with dependencies and stop conditions.
- [Design review register](docs/DESIGN_REVIEW_REGISTER.md) — open decisions, paper findings and design-pass change record.
- [Agent instructions](AGENTS.md) — collaboration constraints.

## Working safely in the project

Keep custom code under Assets/Game. Preserve third-party assets, Unity .meta files, Packages and ProjectSettings. The user's Editor may remain open; do not launch a second Editor against the same project. Notify the user before changing scenes/prefabs they may be editing. The current documentation assignment does not require Editor configuration.

Keep generated Library, Temp, build output and local reports outside version control as configured by the project. Broader talent generation, equipment economy, account services and multiplayer are future ambitions, not implemented features.
