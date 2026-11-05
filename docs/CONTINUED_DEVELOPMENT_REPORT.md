# Continued development report — 2026-11-05

## Recoverable state

Started with a clean working tree at `4bdf2e0` on `development/overnight-character-evolution`. Work continues on `development/character-ownership`; no previous checkpoints were reset.

Implementation checkpoints:

- `bb610ba`: record the passed focused input gate and scope ownership.
- `9d0c84f`: character-owned Heavy discipline, contextual knowledge, separate learning and activation.
- `8fe3816`: versioned explicit saves, validation and protected backup recovery.
- `ecaf2f2`: focused panel input checks, readable confirmations, safe Play-mode teardown.

This report is the final documentation checkpoint. Use `git log` for its commit identifier. The prior C1/C2/C3 scenes remain comparisons.

## What is playable

`Assets/Game/Scenes/DevelopmentTest.unity` adds a character-owned development experiment using the existing opponents, combat definitions, evidence classifier and native movement. Shared attack assets do not hold purchases or character balances.

A fresh character has a unique identity and one experimental point. Baseline is free. An explicit preview and confirmation spends the point on either Breaker or Momentum and selects that Heavy. This creates an actual opportunity cost. One-primary ownership, the two possible grants and absence of respec are prototype rules awaiting feedback, not permanent character policy.

Momentum knowledge grows through the existing verified evasive exchanges. Three distinct qualifying enemy commitments reveal Retaliatory Momentum and grant one evolution point once. Reveal does not purchase or activate it. The player must separately learn the revealed development and activate it with compatible Momentum. Verified evasion supplies the defensive experience prerequisite; there is no purchasable empty defensive fixture. Debug investment/technique/trial switches cannot grant owned power in this scene.

The compact panel explains eligibility, learned versus active power and tradeoffs. Its background is opaque, cards wrap and scroll, and confirmations remain above the scrolling area. Development commands require a ready, living, unstaggered player within 2 m of spawn and idle or defeated active opponents. Opening the panel suspends player input only; the world continues.

Explicit A/B saves preserve identity, legal ownership, paid costs, balance, knowledge and configuration. Schema 1 uses stable IDs and validates prerequisites and economy on load. It does not serialize enemies, accepted attacks, pending exchanges or retaliation opportunities. A loaded active evolution needs a new verified evasion to arm.

Writes flush and validate a same-directory temporary file before replacing the destination and retaining its prior version as `.bak`. A malformed latest record can recover a valid backup without rewriting the damaged original. Unsupported schemas refuse loading; unknown primary/configuration IDs retain the original record, select Baseline, deactivate evolution and block overwrite pending reviewed migration. Failed loads retain the current in-memory character. There is no automatic refund, repair, migration or autosave.

## Actual validation

All checks below ran in Unity 6000.6.7f1. Editors were used sequentially; no second Editor was launched against an open project. The final Editor is stopped on DevelopmentTest.

| Gate | Actual result |
| --- | --- |
| Focused prerequisite input | Existing 26-case combat suite passed; focused technique suite passed, including number-row event edges. The original focused capture remains in `Logs/FocusedUnity.log`. |
| Existing regression coverage | Exclusive batch runs passed supported C1 evidence/query/lifecycle/native checks, technique/motor checks, enemy checks, practice checks, immutable snapshots, C2 discovery and nine C3 native retaliation cases. Batch reports explicitly skip focused number-row edges; they do not replace the focused run. |
| Ownership model | Fresh identity, costs, stale/duplicate commands, single primary, once-only reveal grant, knowledge/learning/activation distinction, compatibility and character isolation passed. |
| Native owned loop | Actual three-exchange discovery, deliberate purchase and activation, blocked debug bypass, nine owned retaliation cases and fresh reset passed. |
| Save model/files | Real JSON roundtrip and File.Replace backup; repeated-load balance integrity; independent A/B characters; corrupt/future/missing-content protection; invalid balance/configuration, traversal and I/O failure handling passed. |
| Native owned save/load | Busy commands rejected; legal build, knowledge and balance restored; independent second character retained; no accepted action or opportunity resumed. |
| Focused panel input | Input System Tab events opened, closed and reopened the panel while correctly suspending/restoring movement and look. Exit with panel open passed without runtime teardown errors. |
| Visual UI action | Inspected the small Game view and confirmed an actual mouse-driven Momentum preview/confirmation changed balance from 1 to 0 and locked the competing primary. No user save was written. |
| Final checks | After the UI/lifecycle fixes, focused combat (26 cases), owned development/save/native retaliation and focused panel exit checks passed again. Unity compiled the final code. |

Local reports are ignored generated files under `Logs/`, with final copies under `Logs/ContinuedValidation`. Native file tests use isolated temporary directories under Logs, not the player's persistent save directory. The later batch technique report includes its explicit focused-input skip; the original focused Editor log supplies that earlier gate's evidence.

One real failure was found during UI validation: closing Play mode with the panel open could reactivate PlayerInput after Unity had unregistered it. The owner now verifies enabled/registered input before restoration, and the dedicated exit check captures the corrected behavior. The earlier diagnostic remains in historical logs; fresh focused captures pass. Windows-injected helper keystrokes did not reliably reach Unity runtime Raw Input, so focused Tab checks inject through the existing Input System rather than weakening production input handling.

These are technical checks and a narrow UI inspection. They do not establish player feel, balance, accessibility or hardware-input acceptance. No standalone player build was produced. Conservative evasion sampling remains unchanged; sparse 30/15 fps evidence can remain unverified under its existing maximum sample-gap rule.

## Launch and playtest

1. In the existing Unity Editor, open `Assets/Game/Scenes/DevelopmentTest.unity` and press Play. No package installation or Inspector wiring is required for the committed scene. If the scene is missing, stop Play and use **Tools > EvolvingRPG > Create Character Development Sandbox**; the builder refuses to overwrite an existing scene.
2. Click Game view. Use WASD, Q/E strafe, Space jump, mouse look/wheel zoom, LMB Light, RMB Heavy and Left Shift directional dodge. R resets the encounter and returns to spawn while preserving character development.
3. At the safe starting area, press Tab. Preview and confirm a primary discipline. Close Tab and click Game view to recapture the cursor. Compare Breaker and Momentum using separate fresh characters; fresh creation requires confirmation and replaces unsaved state.
4. On the Momentum character, evade a genuine committed threat, then apply Momentum to that same opponent within 1.5 s after full dodge recovery and finish Heavy recovery. Repeat against three distinct commitments. Read the clue/reveal feedback. Harmless or unverified dodges do not qualify.
5. Press R, then Tab. Purchase the revealed Retaliatory Momentum and explicitly activate it. On a subsequent verified evasion, forward movement intent plus RMB requests the extra 0.4 m advance; neutral/backward/lateral intent retains ordinary Momentum. Missing the linked opponent adds 0.25 s recovery, in addition to ordinary miss behavior where applicable.
6. At spawn, save deliberately to A or B, confirming the action. After exiting/re-entering Play, open Tab and load the desired slot. Loading explicitly replaces unsaved state and resets the encounter. There is no automatic load. Save a fresh second character to the other slot to compare independently.

The panel displays the resolved directory: `Application.persistentDataPath/CharacterDevelopmentPrototype`, currently `%USERPROFILE%\AppData\LocalLow\DefaultCompany\EvolvingRPG\CharacterDevelopmentPrototype`. Corrupt original records are protected; save recovered data to another healthy slot instead of expecting automatic repair.

Repeatable Editor checks are in **Tools > EvolvingRPG**: **Run Character Development Checks**, **Run Character Save Checks**, **Run Development Panel Input Checks** (Ctrl+Shift+F9, leaves Play running) and **Run Development Panel Exit Checks** (Ctrl+Shift+F10, exits Play). These run test encounters and should only be invoked when no personal Play session or unsaved scene work is in progress.

## Actual changed files

New runtime files, with their `.meta` files:

- `Assets/Game/Progression/CharacterDevelopmentState.cs`
- `Assets/Game/Progression/CharacterSaveStore.cs`
- `Assets/Game/Progression/OwnedCharacter.cs`
- `Assets/Game/Progression/DevelopmentPanel.cs`
- `Assets/Game/Progression.meta`

Modified combat adapters:

- `Assets/Game/Combat/ContextualDiscovery.cs`
- `Assets/Game/Combat/RetaliationDiscovery.cs`
- `Assets/Game/Combat/RetaliatoryMomentumTrial.cs`

New Editor files, with their `.meta` files:

- `Assets/Game/Editor/DevelopmentSandboxBuilder.cs`
- `Assets/Game/Editor/DevelopmentChecks.cs`
- `Assets/Game/Editor/CharacterSaveChecks.cs`
- `Assets/Game/Editor/DevelopmentPanelInputChecks.cs`

Modified check integration: `Assets/Game/Editor/DiscoveryChecks.cs`, `Assets/Game/Editor/RetaliationChecks.cs`, `Assets/Game/Editor/OvernightChecks.cs`.

New scene: `Assets/Game/Scenes/DevelopmentTest.unity` and its `.meta`.

Documentation: `AGENTS.md`, `README.md`, `docs/MILESTONES.md`, `docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md`, and this report.

Earlier scenes, attack assets, movement/camera scripts, stamina tuning, third-party files, Packages and ProjectSettings are unchanged from the starting checkpoint. No broader game systems were added.

## Next smallest milestone

Player acceptance of this complete loop is the next dependency: compare two owned characters, assess whether the one-primary choice expresses identity, whether the discovery explanation is understandable, and whether learning versus activation is clear. Check ordinary/evolved Momentum commitment and whether manual save/reload preserves the expected character. Keep stamina permissive for interruption testing.

After feedback, refine the existing cards/causal explanations or demonstrated input defects before adding another evolution. A subsequent bounded experiment may connect the earlier HeavyPractice proficiency track to this character history, but only after deciding what useful experience should persist and how schema migration protects existing characters. That proficiency track is currently separate and is not saved here. Full mastery, permanent economy/respec policy, procedural evolution and broader content remain proposals or deferred work. More mechanics would outrun the current feedback evidence.
