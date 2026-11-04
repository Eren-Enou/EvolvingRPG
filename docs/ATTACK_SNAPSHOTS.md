# Committed attack parameters

This V0.2 foundation supports the talent charter's future action evolution without adding progression. An attack now copies its authored parameters when `TryAttack` accepts it. Timing, hit geometry, damage/poise, movement restriction, feedback, and observation history use that immutable copy until the action ends. Editing the source definition or configuring another attack affects subsequent actions.

## Why it matters

Previously these systems reread the shared `AttackDefinition` asset during the swing. A runtime change could shorten recovery, alter a hit's damage/range, change movement restrictions, or even rename past observations. A future character-specific technique resolver needs one consistent committed action. This change establishes that execution boundary without adding a modifier framework or mutating shared assets.

`CombatActions.CurrentAttack` retains the authoring reference for identification/compatibility. Runtime consumers should use `HasCommittedAttack` and `CommittedAttack`. `CombatObservation.Attack` holds a nullable copy of the same values, while `Definition` retains the source reference. Neither reference nor display name is a persistence ID. Stable content identity remains deferred until persistence/progression needs it.

The snapshot covers attacks only. Existing dodge parameters and behavior are unchanged. Snapshots clear on completion, interruption, and dodge start. Rejected attempts do not replace an active snapshot or spend stamina.

## Editor use and testing

No manual configuration is required. Let Unity finish importing, open `Assets/Game/Scenes/CombatTest.unity`, enter Play mode, and click the Game view. Existing controls remain: LMB Light, RMB Heavy, Left Shift directional dodge, Space jump, WASD movement, Q/E strafe, mouse look/wheel zoom, R target reset, F6 diagnostic clear.

Tune the existing Light/Heavy assets through the Inspector. Changes take effect on the next attack. For a clean comparison, leave Play mode before changing baseline assets; edits to ScriptableObject assets during Play mode can persist in Unity.

Run **Tools > EvolvingRPG > Run Attack Snapshot Checks** with Play mode stopped and scene edits saved. The tool uses temporary Play-mode objects and an in-memory definition; it does not open or save another scene or edit authored attack assets. It enters/exits Play mode and writes `Logs/AttackSnapshotChecks.txt` (ignored local evidence).

Validation on Unity **6000.6.7f1**: compilation succeeded and all six check groups passed with no runtime Console errors captured:

- All committed values survive definition edits, with the original stamina charge.
- Original active timing, range/cone, damage/poise, and per-target deduplication remain effective.
- The movement adapter uses the committed multiplier and restores motor settings.
- Blade color and reach guide use the committed values.
- Original recovery/completion and historical observation values/name remain stable.
- A subsequent attack captures edits; interruption clears its snapshot; dodge starts without an attack snapshot.

These are isolated component checks. They directly step action phases and inspect adapters/feedback; they do not establish real-input responsiveness, collision-integrated movement, or gameplay feel. In the subsequent miss-feedback slice, the expanded eight-group component suite and the combined 22-check combat/input suite both passed in Unity 6000.6.7f1; see `COMBAT_OBSERVATIONS.md` and `MELEE_MISS_FEEDBACK.md`. Player combat acceptance remains pending.

## Files changed in this slice

- Added `Assets/Game/Combat/AttackSnapshot.cs` and Unity-generated `.meta`.
- Updated `CombatActions.cs` to capture, execute, publish, and clear the snapshot.
- Updated `CombatMovement.cs` and `CombatActionFeedback.cs` to consume committed values.
- Updated `MeleeHitDetector.cs` and `HitData.cs` with snapshot overloads; existing definition-based entry points remain available for direct training/debug use.
- Updated `CombatObservation.cs` so historical parameters and names are stable.
- Added `Assets/Game/Editor/AttackSnapshotChecks.cs` and Unity-generated `.meta`.
- Added this guide and updated `COMBAT_OBSERVATIONS.md`, `COMBAT_SANDBOX.md`, `PROGRESSION_DEVELOPMENT_PATH.md`, and `MILESTONES.md`.

Scenes, prefabs, movement input/camera scripts, shared attack assets, third-party assets, Packages, ProjectSettings, and the user-provided talent charter have no changes from this slice. No mastery, unlocks, talent choices, or saves are implemented.
