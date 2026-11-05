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
| PracticeTest | Preserved Heavy practice and optional recovery trial |
| EvasionEvidenceTest | C1 diagnostic-only evasion evidence |
| DiscoveryTest | C2 session investments, contextual hint/reveal and journal; no effects |
| RetaliationTest | C3 discovery plus optional extended-return A/B trial |

Newest playable slice: open **Assets/Game/Scenes/RetaliationTest.unity**, press Play and click Game view. See [discovery/trial controls](docs/DISCOVERY_PLAYGROUND.md) and the [overnight report](docs/OVERNIGHT_PROGRESS_REPORT.md). Start with 3 for Momentum and F4/F5 for the two explicit investment fixtures. Verified evasion plus a completed same-opponent Momentum exchange supplies a clue; three distinct strikes reveal the option. F2 enables a trial after reveal; a subsequent verified evasion plus forward intent + RMB requests the extended return. Discovery never automatically grants ownership.

C1/C2/C3 Unity compilation and supported native checks passed. The focused keyboard/mouse combat suite remains for interactive Editor testing; gameplay feel is not validated by these checks. Real talent purchases and saves are deferred. Stamina remains deliberately permissive and unchanged.

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

Keep custom code under Assets/Game. Preserve third-party assets, Unity .meta files, Packages and ProjectSettings. The user's Editor may remain open; do not launch a second Editor against the same project. Notify the user before changing scenes/prefabs they may be editing. New test scenes are generated with explicit Unity builder menus; importing scripts does not create them. The included scenes need no manual component wiring.

Keep generated Library, Temp, build output and local reports outside version control as configured by the project. Broader talent generation, equipment economy, account services and multiplayer are future ambitions, not implemented features.
