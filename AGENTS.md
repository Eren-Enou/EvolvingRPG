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

Current branch `development/heavy-experience` connects completed live Heavy outcomes to bounded per-character history. Read docs/HEAVY_EXPERIENCE_HISTORY.md, docs/CHARACTER_IDENTITY_PATH.md and the latest continued report before choosing another slice. History has no bonus, points or discovery prerequisite. The older PracticeTest recovery trial remains separate. Technical checks do not establish player feel.

Preserve the V0.4 D ownership/discovery/activation separation and all comparison scenes, controls, attack assets, stamina, third-party code, Packages and ProjectSettings. No empty defensive talent is sold. Heavy history records completed live contact, guard break and windup interruption once per profile per character, including initiative before an enemy strike. Full recovery and current opponent generation are required; R retains completed facts. The fact set is its durable lifetime credit budget, not an unlimited mastery economy.

Safe Unity tests are authorized. Never launch a second Editor against an open project, interrupt a user Play session, or overwrite unsaved edits. New scene authoring must use explicit Editor APIs. Report exact checks, limitations, commits, launch steps and future decisions in docs/CONTINUED_DEVELOPMENT_REPORT.md. Experimental budgets and specialization restrictions are not final release rules.
Current result: DevelopmentTest now implements owned Heavy choice, evidence-based contextual knowledge, separate retaliation purchase/activation, and explicit versioned A/B saves with protected recovery. See docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md and docs/CONTINUED_DEVELOPMENT_REPORT.md. Preserve existing comparisons and the absence of automatic purchases or transient opportunity persistence. Safe-spawn panel controls pause player input only, not the world. The one-discipline / two-grant fixture is experimental, not a permanent respec/economy decision. The old HeavyPractice proficiency track is not yet integrated into character saves. Prioritize player feedback before another ability/evolution.

The earlier bounded attack review remains presentation-only; experience reads authoritative observations independently. Saves now use schema 2; schema 1 migrates in memory with empty history, never fabricated accomplishments. Read does not rewrite; explicit save retains the previous supported record as backup. Protect unknown experience/discovery/schema records, and never add history merging or autosave casually. Run Extended Regression Checks in a stopped, saved, focused Editor: twelve suites, without another Editor process. Combat input checks use a restored 60 Hz test clock; production timing is untouched. Player feedback on history fairness, return intent and a useful familiarity benefit is required before adding an owned modifier or using history as a purchase gate.
