# Combat observations: first development slice toward progression

This is a V0.2 combat-diagnostics addition supporting `EVOLVING_TALENT_SYSTEM.md` sections 4, 6, and 11. It creates observable, testable action outcomes before any mastery or discovery evaluator is introduced. It grants no XP, talents, knowledge, or power and does not change attack/dodge tuning.

## Try it

1. Open `Assets/Game/Scenes/CombatTest.unity` in Unity, enter Play mode, and click the Game view. Existing components pick up the new diagnostics automatically; no scene or prefab wiring is required.
2. Use Light/Heavy, intentionally miss, break target poise, and dodge. The lower debug panel shows actions, target hits, completed attack misses, poise breaks, completed dodges, and the latest observation.
3. Press **F6** to clear diagnostic history and counters. This does not reset the target, cancel an action, restore stamina, or reset the action sequence. Clear while idle to capture a complete new action lifecycle.
4. **R** still resets the target only. Resetting it never grants progression or clears the actor's diagnostic history. Leaving Play mode discards observations.

## Evidence contract

`CombatActions.Observed` publishes actor-local immutable `CombatObservation` values. `CombatActions.Diagnostics` retains at most 64 observations; counters summarize the session since the last clear even when old records are evicted. There is no global event bus or disk logging of sessions.

| Observation | What it establishes | What it does not establish |
| --- | --- | --- |
| Accepted | A resource/grounding/commitment-valid attack or dodge started | A hit, successful evasion, or mastery credit |
| HitResolved | A training target accepted one hit from that swing; applied health/poise and break/defeat outcomes are recorded | Meaningful enemy context, progression eligibility, or repeated credit from extra colliders |
| Completed attack | Recovery finished; target count distinguishes hits from misses | Qualification for a reward |
| Completed dodge | Dodge travel/recovery finished | An enemy attack was avoided; the summary explicitly says `evasion unverified` |
| Interrupted | A busy action component was disabled before completion | A completed technique, a miss after full recovery, or a reward |

Records carry a session-local action sequence, Attack/Dodge kind, attack-definition reference where applicable, timestamp, and target/result information. The definition reference identifies runtime content independently of its display name. The sequence is meaningful only within this actor instance; neither it nor a Unity object reference is a save-game ID. Stable content IDs and versioned persistence remain part of the later progression experiments.

The subsequent [committed attack snapshot slice](ATTACK_SNAPSHOTS.md) adds `CombatObservation.Attack`: a nullable immutable copy of the parameters committed at attack acceptance. Summary names now use that copy rather than rereading the mutable authoring asset. Definition references remain useful for attribution; historical values do not change when an asset is edited.

[Melee miss feedback](MELEE_MISS_FEEDBACK.md) adds `MissReason` for completed attack misses. It retains the most specific failed geometry gate observed during the active window; it does not classify player skill or progression eligibility.

Hit receipts distinguish configured impact from applied outcomes. Overkill damage is capped to remaining HP. Poise applied while already staggered is zero; a threshold-breaking hit reports only the remaining poise needed to break it. The existing visual hit event still receives the original attack impact so animation feedback remains appropriate. Defeated targets emit no additional resolved hit.

Rejected attempts produce no accepted observation. Each accepted attack has at most one hit record per target, including targets with multiple colliders. A normal attack lifecycle is Accepted -> zero or more HitResolved -> Completed; disabling it replaces completion with Interrupted. Training-target hits invoked directly outside an actor's swing are not attributed to that actor.

## Why this is the first step

The charter requires qualitative outcomes rather than raw button-count farming. Existing `AcceptedActions` and HUD strings cannot establish that distinction on their own. This slice provides structured diagnostics while retaining the separation between action execution, geometry queries, target resolution, and feedback.

The next contextual evidence needs V0.3's real opponent: a telegraphed threat, a resolved defensive outcome, and an explicit definition of a meaningful opening. Do not infer successful evasion from a dodge-to-Heavy timestamp, or enable mastery on the training dummy. When progression is assigned, validate two Heavy variants first, then let a narrowly scoped mastery/discovery observer consume the proven outcomes without embedding talent rules into movement or hit detection.

## Validation

Unity **6000.6.7f1** compiled the runtime and Editor changes successfully. The combined **22-check combat suite passed in one uninterrupted Play-mode run** after the snapshot and miss-feedback additions. Checks cover Light/Heavy timing, impact/commitment, anti-repeat/cancel rules, range/arc/wall misses and their explanations, collider deduplication, poise, strafing/diagonal/wall-constrained dodges, stamina, jumping, mouse-look/zoom, resets, applied receipts, bounded history, and interruption. No runtime Console errors were captured.

Earlier interactive reruns were interrupted by focus changes. The harness now measures durations using the gameplay simulation clock, and the consolidated passing run supersedes those incomplete reports. The separate eight-group snapshot/feedback component suite also passed. These checks verify functional behavior, not gameplay feel; player combat acceptance is still required.

Subsequent player feedback found repeated jumping, which the original short jump check missed. [JUMP_INPUT_FIX.md](JUMP_INPUT_FIX.md) records the reproduced failure, fix, and four landing-cycle regressions. The expanded **26-check suite passed in one run** after the fix, with no runtime Console errors captured; `Logs/CombatPlayModeChecks.txt` now contains that report. The player reports fluid combat; the corrected jumping behavior still needs their retest.

Local ignored evidence: `Logs/ObservationValidation.log` contains compilation and Play-mode results, `Logs/CombatPlayModeChecks.txt` contains the consolidated 22-check PASS, and `Logs/AttackSnapshotChecks.txt` contains the eight-group snapshot/feedback PASS. Scenes, movement input/camera code, Packages, and ProjectSettings have no diff from these slices.

Run **Tools > EvolvingRPG > Run Combat Play Mode Checks** (Ctrl+Shift+F8), with scene edits saved and Unity focused. The suite checks existing combat and diagnostics together and writes its local result to the ignored `Logs/CombatPlayModeChecks.txt` file.

## Exact file changes

- Added `Assets/Game/Combat/CombatObservation.cs` and `.meta`: observation kinds, immutable evidence and applied-hit receipt.
- Added `Assets/Game/Combat/CombatObservationLog.cs` and `.meta`: bounded actor-local history/counters.
- Updated `Assets/Game/Combat/CombatActions.cs`: accepted, resolved-hit, completed and interrupted observations.
- Updated `Assets/Game/Combat/MeleeHitDetector.cs`: optional callback after a target accepts a hit, retaining per-swing deduplication.
- Updated `Assets/Game/Combat/TrainingTarget.cs`: applied-hit receipt overload and accurate debug amounts; existing boolean API and visual event remain available.
- Updated `Assets/Game/Combat/CombatSandboxHUD.cs`: training observation panel and F6 clear action.
- Updated `Assets/Game/Editor/CombatPlayModeChecks.cs`: lifecycle, receipt, history, and interruption assertions alongside existing regression checks.
- Added this guide; updated `docs/COMBAT_SANDBOX.md`, `docs/PROGRESSION_DEVELOPMENT_PATH.md`, and `docs/MILESTONES.md` to describe the implementation.

No scenes, prefabs, attack assets, movement scripts, vendor packages, or project version settings were intentionally edited for this slice. The user-provided talent charter is preserved.

## Received impacts during commitment (2026-11-06)

`ImpactReceived` is appended to the observation-kind enum. When a busy actor receives an applied hit, CombatActions publishes the actual HitResolution and current CombatPhase before any synchronous stagger/defeat interruption. Publishing from the action's existing impact handler avoids subscriber-order-dependent loss of the very hit that ends an attack. It does not change hit resolution, interruption, costs or timing.

These receipts are incoming impact, not outgoing hits, misses, inflicted poise breaks or experience candidates. Outgoing diagnostic counters are unchanged. Ready-state hits do not belong to an old action. Dodge impacts can appear in diagnostics but do not replace the attack-only review. A zero-applied-impact receipt does not increment the review's received-impact count.

DevelopmentTest's Recent attacks retains independent applied damage/poise dealt and received, number of received impacts, phase of first impact, and player stagger/defeat. The interval is acceptance through completion/interruption only. It does not measure later exposure, damage after control returns, intent, enemy damage prevented, exchange efficiency or whether a trade was worth taking. An interrupted trade may be tactically useful while still failing the current experimental full-recovery experience rule. No eligibility or reward policy reads this presentation.

The snapshot suite tests actual CombatVitality.ReceiveHit and CombatActions events, including non-staggering completed trades, active/recovery damage, the stagger-causing receipt preceding interruption, clamped lethal damage, post-completion/dodge/zero exclusion and separation from outgoing counters. Fixtures use temporary objects and no authored asset edits.

Interruptions now retain the actual reason supplied by the production action path, including stagger, defeat, encounter reset and component disable. The reason is captured in the terminal observation before cleanup and retained in the immutable review; later HUD messages or actions cannot replace it. Legacy synthetic interruptions without a cause are labeled Cause not recorded rather than guessed. Completed attacks carry no interruption cause. This does not change which actions interrupt.
