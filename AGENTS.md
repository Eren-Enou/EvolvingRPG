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

The extended validation/depth follow-up is implemented on `development/combat-readability`. On 2026-11-05 all eleven focused regression suites passed, including the 26-case combat suite, number-row events, ownership/save, attack review, panel lifecycle and Q/E movement/camera cases. These are technical results, not player feel validation. Read the latest continued report before choosing another slice.

Implement the smallest isolated V0.4 D ownership/spending slice, then minimal versioned persistence only after ownership checks pass. Preserve earlier comparison scenes, controls, attack assets, stamina, third-party code, Packages and ProjectSettings. Replace the empty defensive investment prerequisite with verified defensive experience in the owned-character scene; do not sell an empty support talent. Discovery, ownership and activation remain distinct.

Safe Unity tests are authorized. Never launch a second Editor against an open project, interrupt a user Play session, or overwrite unsaved edits. New scene authoring must use explicit Editor APIs. Report exact checks, limitations, commits, launch steps and future decisions in docs/CONTINUED_DEVELOPMENT_REPORT.md. Experimental budgets and specialization restrictions are not final release rules.
Current result: DevelopmentTest now implements owned Heavy choice, evidence-based contextual knowledge, separate retaliation purchase/activation, and explicit versioned A/B saves with protected recovery. See docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md and docs/CONTINUED_DEVELOPMENT_REPORT.md. Preserve existing comparisons and the absence of automatic purchases or transient opportunity persistence. Safe-spawn panel controls pause player input only, not the world. The one-discipline / two-grant fixture is experimental, not a permanent respec/economy decision. The old HeavyPractice proficiency track is not yet integrated into character saves. Prioritize player feedback before another ability/evolution.

Extended follow-up: `development/combat-readability` adds a bounded, session-only attack review and actual applied-impact messages, plus protection against unknown discovery-rule rollback. No balance, scene or save-schema change. Review is presentation evidence, never progression eligibility: R retains it for inspection; fresh creation/load clear it. Use the explicit `Run Extended Regression Checks` menu in a stopped, saved, focused Editor; it checks all eleven suites without another Editor process. The next design probes are in docs/NEXT_COMBAT_PLAYTESTS.md. No new combat talent or durable HeavyPractice integration has been selected; player feedback on existing return intent and consequences remains the next dependency.
