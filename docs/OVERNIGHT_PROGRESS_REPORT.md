# Overnight progress report — first playable character evolution

Session: 2026-11-04 to 2026-11-05, America/Los_Angeles. Unity **6000.6.7f1**. Implementation and automated correctness are distinct from player validation.

## Git state and checkpoints

Started on `main` at `6f07882`, matching the reviewed GitHub baseline, with the previous C1 implementation/documentation uncommitted. No ahead commits or unrelated local edits were found. Preserved that work and developed on `development/overnight-character-evolution`; main was not moved.

- `1055d8b` — C1 native-query correction, evidence sandbox and verified supported checks.
- `bf02919` — C2 deterministic discovery, session fixtures, journal, live exchanges and inactive-enemy reset fix.
- `e691c0386d81a0b72ad45f5426aeca0d47cf86e2` — final tested gameplay implementation: C3 immutable target-bound trial, delayed C2 evidence handling and current instructions.

The implementation branch was pushed successfully to `origin/development/overnight-character-evolution`. This report is a subsequent documentation checkpoint; `git log -4 --oneline` identifies its branch-head commit as well as the implementation checkpoints. No force push, merge or PR was performed.

## Phase status

| Phase | Result |
| --- | --- |
| 0: assessment | Completed: project instructions, vision/talent charter, milestones, progression/context/depth documents, actual local Git state, packages and controller architecture inspected. Unity was closed before exclusive batch runs. |
| 1: C1 evasion | Implemented; model, real physics/lifecycle and native motor checks pass in the documented conservative envelope. EvasionEvidenceTest included. |
| 2: C2 discovery | Implemented; deterministic negative cases and three real native evade/Momentum exchanges pass. DiscoveryTest included. One clue, then reveal after three distinct strikes; knowledge grants no power. |
| 3: C3 optional effect | Implemented; nine real native cases plus fresh-character isolation pass. RetaliationTest included with trial off initially. Mechanical distinction is verified; usefulness/feel awaits player testing. |
| 4: D specialization/save stretch | Not started. The focused input regression gate remains unverified in batch mode. Actual ownership, stable saved content IDs, spending and persistence are deliberately deferred; fixtures are not substitutes. |

C1/C2/C3 are functional prototypes, not finalized balance or player acceptance. The full baseline keyboard/mouse regression suite remains an explicit validation limitation.

## What works

C1 links an accepted dodge to a committed enemy identity, captures actual native-controller displacement and checks a threatened stationary starting position against the real hit query. It refuses incomplete/coarse trajectory samples, ambiguous overlapping threats, interrupted/reset sources and incoming contact. It retains bounded, duplicate-resistant diagnostic receipts without rewards.

C2 captures two separate session investment prerequisites, requires ordinary Momentum accepted within 1.5 seconds of full dodge recovery, an applied impact on the evaded opponent generation, and complete uninterrupted Heavy recovery. One qualified strike foreshadows; three reveal Retaliatory Momentum. The journal records time, cause and evidence keys. A post-dodge attack can precede final defensive evidence; its actual acceptance time remains authoritative. New dodges, incoming damage, source interruption and sampling failures still invalidate evidence as appropriate.

C3 uses `IAttackResolver` for a side-effect-free preview and a narrow acceptance hook for one-time consumption. An immutable action context binds the opponent generation and drawback. The optional trial adds 0.4 m advance, with 0.25 s added recovery unless that generation receives an applied impact. Bystander-only hits retain the added cost; complete misses also retain Momentum's existing 0.55 s miss cost. No shared attack definition is mutated. Collision, facing commitment, existing stamina and interruption rules remain.

## Tests actually executed

Final supported suite ran inside the real Unity Editor in exclusive batch mode, using `OvernightChecks.RunHeadless`; it exited successfully after the final revision. This is engine execution, not offline compilation or a player playtest. No game-script compilation errors remained.

| Suite | Actual final result |
| --- | --- |
| C1 | PASS: threatened versus harmless/pre-commit/blocked/interrupted/reset/coarse cases; duplicate protection; actual collider/LOS queries; real enemy lifecycle; native 60 Hz dodge, wall clipping and unverified 15 Hz evidence. |
| Techniques | PASS: six isolated groups and three native motor cases (60/15 Hz and wall). Number-row bindings resolve. **SKIP** synthetic number-row event edges in batch mode; they need a focused interactive Game view. |
| Enemies | PASS: 11 isolated groups plus normal-frame approach, commitment, strike, player damage and reset. |
| Heavy Practice | PASS: six isolated groups and live native Breaker/windup interruption/completion/award/reset. |
| Attack snapshots | PASS: eight groups covering committed parameters, hits, movement/feedback, interruption, misses and subsequent actions. |
| C2 | PASS: deterministic clue/reveal, duplicate strike, missing/changed prerequisites, wrong technique/target, late/incomplete/interrupted actions, stale generation and reset isolation. Three real native exchanges reach Revealed; the middle counter starts before the final evidence receipt. Run in the knowledge scene and again before C3 in the trial scene. |
| C3 | PASS: ordinary reference, enhanced linked hit, bystander-only hit, full miss, actual wall, target reset after acceptance, natural expiry, reset before acceptance and defeated target fallback. Also preview/rejection/busy consumption protection, ordinary neutral/backward/lateral previews, fixed facing, resolver disable during commitment, unchanged assets/stamina, and fresh-character isolation. |
| Focused combat input | Attempted, **not passed**: batch mode accepted no synthetic mouse attack (`accepted 0`). Do not interpret this as validated physical input or a completed 26-case combat regression. Run its existing menu interactively. |

C3 measured ordinary travel **1.200 m**, evolved travel **1.600 m**, and wall-clipped travel **0.425 m**. Successful return duration was approximately **1.017 s**, bystander/reset-target drawback **1.267 s**, complete evolved miss **1.817 s**, and expired/defeated ordinary miss **1.567 s**, including frame quantization. These establish execution of authored values, not balanced or enjoyable timing.

Final executed suites captured no runtime Console errors. Unity startup still emits package/assembly-reference warnings from imported packages; those assets were not rewritten. Initial failing runs were investigated and corrected, rather than counted as passes. Logs are local, ignored `Logs/*Checks.txt` and `Logs/OvernightUnity.log`; the earlier failed focused-input report remains clearly separate from the passing supported run.

Preservation review: the five original scenes and their metadata, attack data, movement/camera sources, CombatStamina, third-party assets, Packages and ProjectSettings have no changes against `6f07882`. Vision and the talent charter are unchanged. New scenes were generated with Unity authoring APIs. Changed sources pass Git whitespace checks.

## Important corrections and design decisions

- Unity's native CharacterController closest-point query differs from the authored ideal capsule. Stationary proxy queries now translate the real native query; the larger ideal capsule remains a conservative movement enclosure. Gameplay strike geometry was preserved.
- Test fixture homes now respect enemy leashes; fixed 60 Hz simulation prevents batch startup frames from invalidating timing checks. This changes fixtures, not enemy balance.
- Resetting the initially inactive optional enemy exposed uninitialized motor/home state. Idempotent initialization now makes that actual user R-reset path safe.
- The expiry boundary is inclusive; the test now checks the first genuinely expired frame.
- Intent policy is **forward world movement relative to current facing + ordinary RMB**, with a visible armed cue. Neutral/backward/lateral intent uses ordinary Momentum. Held forward input can opt in; readability and control preference remain playtest questions.
- Investment flags and F2 activation are session research fixtures, not real learned talents. F8 clears knowledge/prerequisites/trial activation; debug technique selection remains a separate comparison control.
- Evidence defaults require gaps no larger than 0.025 s and the documented supported movement/geometry bounds. Approximately 30 FPS and 15 FPS can remain unverified. Do not expand the bound merely to produce more positive receipts.
- Stamina costs/regeneration are unchanged. No enemy families, gear, classes, procedural talents, networking or mission systems were added.

## Exact Editor launch and playtest

1. Open this existing project in Unity **6000.6.7f1**: `C:\Users\Aaron\CoderVibe\EvolvingRPG\EvolvingRPG`. Let it import scripts. Do not open another Editor against the same project.
2. Open **Assets/Game/Scenes/RetaliationTest.unity**. The included scene needs no manual wiring. If missing, choose **Tools > EvolvingRPG > Create Retaliation Trial Sandbox** with Play mode stopped.
3. Press **Play**, then click Game view. Existing WASD, Q/E, mouse look/wheel, Space jump, Shift dodge, LMB/RMB remain.
4. Press **3** for Momentum; press **F4** and **F5** to enable both clearly labeled prerequisite fixtures while idle. Technique selection does not set investments.
5. Approach the guarded opponent in the left lane. Let its yellow windup commit, then dodge early enough to leave the threatened space. A lateral dodge preserves facing; deliberately turn/reposition for the return. Land Momentum on that same opponent, accepting RMB within 1.5 seconds of full dodge recovery, and finish recovery.
6. Observe the clue; repeat against three distinct committed strikes to reveal the possibility. **R** resets the encounter while retaining session knowledge/fixtures. **F7** expands journal details. **F8** clears the session character's knowledge, fixtures and trial activation.
7. After reveal, press **F2** while idle to enable the trial. Perform another verified evasion and wait for the armed cue. Before its original window expires, use deliberate forward movement intent plus RMB for the longer return. Neutral/backward/lateral intent retains ordinary Momentum. Toggle F2 off to compare. Nothing is purchased or saved.
8. Compare a connected return, a poorly faced miss, a bystander-only hit, and a wall-clipped return. Check whether the cue lets you predict travel. Gameplay feel, usefulness and the activation policy need your feedback.

Evidence-only reference: **EvasionEvidenceTest**. Knowledge-only reference: **DiscoveryTest**. F3 toggles the optional fast opponent; use the guarded single-opponent case first. See [DISCOVERY_PLAYGROUND.md](DISCOVERY_PLAYGROUND.md) and [EVASION_EVIDENCE_EXPERIMENT.md](EVASION_EVIDENCE_EXPERIMENT.md) for tuning/support limits.

Interactive regression menus, with scenes saved and Play mode stopped: **Run Combat Play Mode Checks**, **Run Technique Play Mode Checks**, then **Run Discovery Checks** or **Run Retaliation Trial Checks**, all under **Tools > EvolvingRPG**. Tests temporarily take over Play mode; run them separately from your own playtest.

Exclusive batch reproduction, only after closing the existing Editor:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\Aaron\CoderVibe\EvolvingRPG\EvolvingRPG' -executeMethod EvolvingRPG.Editor.OvernightChecks.RunHeadless -logFile 'Logs/OvernightUnity.log'
```

The runner exits Unity itself; omit `-quit` so asynchronous native checks can finish. It intentionally omits the focused mouse-input suite and explicitly skips number-row edge injection. Run those interactively.

## Recommended next milestone

First finish focused input regressions and player comparison of the cue, window, extended travel and miss exposure. Then implement V0.4 D as one small ownership/spending slice with stable content IDs and a controlled experimental budget. Resolve the empty defensive fixture through meaningful defensive practice or an independently worthwhile support development; do not sell it as an empty prerequisite. Keep discovered, eligible, learned and active separate. Begin versioned persistence only after legal spending and fresh-character isolation pass. The talent economy, respec rules and permanent identity limits remain proposals.

## Actual changed files

Paths below are relative to the Unity project root. This manifest includes the pre-existing uncommitted C1 work preserved at session start, plus this session's implementation. New scripts/scenes include their accompanying metadata. This report itself is an additional changed file.

```text
AGENTS.md
Assets/Game/Combat/AttackCommitment.cs
Assets/Game/Combat/AttackCommitment.cs.meta
Assets/Game/Combat/AttackSnapshot.cs
Assets/Game/Combat/CombatActions.cs
Assets/Game/Combat/ContextualDiscovery.cs
Assets/Game/Combat/ContextualDiscovery.cs.meta
Assets/Game/Combat/DiscoveryHUD.cs
Assets/Game/Combat/DiscoveryHUD.cs.meta
Assets/Game/Combat/EnemySandboxHUD.cs
Assets/Game/Combat/EnemyThreat.cs
Assets/Game/Combat/EnemyThreat.cs.meta
Assets/Game/Combat/EvasionAttempt.cs
Assets/Game/Combat/EvasionAttempt.cs.meta
Assets/Game/Combat/EvasionEvidence.cs
Assets/Game/Combat/EvasionEvidence.cs.meta
Assets/Game/Combat/EvasionEvidenceHUD.cs
Assets/Game/Combat/EvasionEvidenceHUD.cs.meta
Assets/Game/Combat/HitData.cs
Assets/Game/Combat/RetaliationDiscovery.cs
Assets/Game/Combat/RetaliationDiscovery.cs.meta
Assets/Game/Combat/RetaliatoryMomentumTrial.cs
Assets/Game/Combat/RetaliatoryMomentumTrial.cs.meta
Assets/Game/Combat/SandboxEnemy.cs
Assets/Game/Combat/StrikeContactQuery.cs
Assets/Game/Combat/StrikeContactQuery.cs.meta
Assets/Game/Editor/CombatPlayModeChecks.cs
Assets/Game/Editor/DiscoveryChecks.cs
Assets/Game/Editor/DiscoveryChecks.cs.meta
Assets/Game/Editor/DiscoverySandboxBuilder.cs
Assets/Game/Editor/DiscoverySandboxBuilder.cs.meta
Assets/Game/Editor/EnemyPlayModeChecks.cs
Assets/Game/Editor/EvasionEvidenceChecks.cs
Assets/Game/Editor/EvasionEvidenceChecks.cs.meta
Assets/Game/Editor/EvasionSandboxBuilder.cs
Assets/Game/Editor/EvasionSandboxBuilder.cs.meta
Assets/Game/Editor/OvernightChecks.cs
Assets/Game/Editor/OvernightChecks.cs.meta
Assets/Game/Editor/RetaliationChecks.cs
Assets/Game/Editor/RetaliationChecks.cs.meta
Assets/Game/Editor/RetaliationSandboxBuilder.cs
Assets/Game/Editor/RetaliationSandboxBuilder.cs.meta
Assets/Game/Editor/TechniquePlayModeChecks.cs
Assets/Game/Scenes/DiscoveryTest.unity
Assets/Game/Scenes/DiscoveryTest.unity.meta
Assets/Game/Scenes/EvasionEvidenceTest.unity
Assets/Game/Scenes/EvasionEvidenceTest.unity.meta
Assets/Game/Scenes/RetaliationTest.unity
Assets/Game/Scenes/RetaliationTest.unity.meta
README.md
docs/CONTEXTUAL_DISCOVERY_EXPERIMENT.md
docs/DEVELOPMENT_EXPERIMENT_BACKLOG.md
docs/DISCOVERY_PLAYGROUND.md
docs/EVASION_EVIDENCE_EXPERIMENT.md
docs/GROWTH_DESIGN_ATLAS.md
docs/MILESTONES.md
docs/PROGRESSION_DEVELOPMENT_PATH.md
```
