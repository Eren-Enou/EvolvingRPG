# V0.4 B — Heavy practice trial

This is an isolated experiment toward meaningful-use mastery, following the user's instruction to continue development with judgment. It does not settle mastery formulas or imply that the earlier technique variants have been fully playtested. Their original scenes and parameters remain available for comparison.

**Hypothesis:** Heavy familiarity can reward completed actions in live exchanges, distinguish disruption/interception, reject inert repetition, and produce one bounded proficiency change without mutating authored actions or granting talents.

## Launch and controls

Open **Assets/Game/Scenes/PracticeTest.unity** in Unity **6000.6.7f1**, enter Play mode, and click the Game view. No manual configuration or new packages are required. If missing on another checkout, allow script import or choose **Tools > EvolvingRPG > Create Practice Sandbox**. It creates a separate copy of TechniqueTest and refuses to overwrite existing scenes.

Existing controls remain: WASD, Q/E strafe, Space jump, Shift dodge, LMB Light, RMB Heavy, number-row 1/2/3 to select Heavy experiments, mouse look/wheel zoom, Escape release, and F1 opponent details.

- **F2** toggles a proposed recovery trial for future Heavy attacks. It takes effect only after the practice threshold is met. It is an A/B debug control, not an unlock purchase or respec policy.
- **R** resets the encounter and the practice trial: points, qualified-action counters, threat budgets, history, and trial toggle return to fresh state. Stamina continues its existing regeneration; there is no refill.
- **F6** clears combat observations only. It does not clear practice or reopen a consumed threat budget.

The HUD shows practice points, qualified actions, requested/ready trial state, and the latest credit/rejection reason. `HeavyPractice.History` retains the last eight decisions for debugging. Leaving Play mode discards the experiment's state. Nothing is written to a character or account save.

## Proposed eligibility rules

All three explicitly referenced Heavy definitions share one track. Display names do not determine membership. Light and dodge never earn Heavy credit.

An accepted Heavy can earn credit only after its recovery completes, with an applied hit on an enabled opponent engaging this actor. The opponent must have committed a real strike within eight seconds of the hit and be in a live exchange, rather than idle, returning, or already defeated. A training target, disabled opponent, foreign source, stale threat, miss, or interrupted player action gives no credit.

| Completed outcome | Proposed credit |
| --- | --- |
| Applied Heavy impact in a qualifying exchange | 1 practice point |
| Frontal guard break, or a poise break interrupting enemy windup | 2 practice points total |
| Both guard break and windup interruption on that impact | Still 2 total; both qualitative counters increment |

Only **one qualifying player action per committed enemy strike** can receive credit. A further Heavy against the same strike gives an explicit duplicate rejection. An action hitting multiple targets receives at most one award, using its highest-credit eligible contact; bonuses do not stack across targets. Interrupted actions discard pending contact without spending a threat budget. Duplicate completion notifications cannot replay an award.

Resetting an opponent before recovery finishes invalidates its pending receipt. Resetting that opponent alone after it has supplied credit cannot farm the existing trial; use a full encounter reset to start fresh, which also clears practice. This is an experimental anti-repetition rule, not a finalized anti-farming or character progression policy.

## Optional proficiency effect

At **five proposed practice points**, F2 can enable **8% less base recovery** for future Heavy attacks. The reduction is a single bounded trial, not a growing multiplier. Its Inspector range is 0–15%; the threshold is configurable. Momentum's additional miss recovery is preserved in full. Damage, poise, stamina cost/regeneration, windup, active time, range, facing, advancement, Light, and dodge remain unchanged.

For the default baseline Heavy, base recovery changes from 0.65 to 0.598 seconds when the trial is enabled and ready. Toggling F2 or gaining a point mid-action does not rewrite that committed action. Turning the trial off returns future attacks to authored recovery. R resets the trial and toggle. These values and the decision to improve recovery remain proposals for player testing.

## Playtest sequence

1. Start with F2 off. Approach an opponent and let it commit a strike; land a Heavy and finish recovery. Check the reason and one-point result.
2. Use Breaker to interrupt the guarded fighter's yellow windup. If it breaks frontal guard and you finish recovery, expect two points, with guard-break and windup-interrupt feedback.
3. Compare a miss, a trade that staggers you during recovery, a repeated Heavy tied to the same enemy strike, and a later Heavy after a new strike. Their credit reasons should be understandable.
4. At five points, toggle F2 and compare recovery timing. Keep stamina permissive and stress-click to investigate interruption edges.
5. Press F6 and confirm the practice total remains. Press R and confirm a fresh trial with no points or active effect.

A completed dodge remains **unverified evasion**. This experiment does not infer success from a dodge-to-Heavy timestamp or reveal a counterattack talent. Future discovery work needs explicit defensive evidence, investment prerequisites, and separate knowledge/ownership presentation.

## Implementation and tuning

`EnemyImpactContext` freezes phase, encounter revision, enemy strike sequence/start time, and source engagement **before** health callbacks can stagger/defeat the enemy. `HitData.Source` identifies the actor's receiver; missing or foreign source identity is ineligible. The resolved receipt travels with `CombatObservation`, so the practice observer does not reconstruct pre-impact state from an already-reacting enemy.

`HeavyPractice` observes actor-local accepted/hit/completed/interrupted events. It keeps at most 16 pending contacts per action, eight history entries, and 64 tracked enemy budgets; target-capacity exhaustion rejects new targets until reset. These limits fit the two-opponent prototype without adding a global event bus or a general mastery framework. Session sequences/revisions and Unity references are not save IDs.

`CombatActions` has one optional actor-owned `IAttackResolver`. It resolves the effective snapshot before acceptance, then the existing execution path consumes that snapshot. The experiment implements this interface and changes only base recovery through `AttackSnapshot.WithRecovery`. Earlier scenes have no configured resolver and retain their behavior. The action and movement code contain no mastery eligibility rules.

Select Player in PracticeTest to tune `HeavyPractice`'s explicit Heavy definitions, threshold, reduction, and threat window. The component is wired into `CombatActions.Attack Resolver` by the scene builder. Disabling the resolver component makes future attacks use authored values. No shared ScriptableObject is modified.

## Scope and open questions

This is a transient practice track, not the final infinitely extensible mastery economy. R intentionally discards trial state to keep comparisons repeatable. There are no character saves, stable content IDs, account archive, talent purchases, mastery tiers, automatic discoveries, or permanent character choices yet. Stable IDs/versioned state belong in the later persistence slice.

Questions to evaluate: Does requiring full recovery fairly reward commitment, or punish useful trades? Does one award per enemy strike distinguish practice from repetition without feeling arbitrary? Do guard/interception bonuses encourage varied choices? Is a recovery improvement noticeable and compatible with each variant's drawback? None of these thresholds or rules are permanent design decisions.

The conservative threat rule currently rejects a preemptive opening hit before the opponent's first committed strike, even against an approaching enemy. That is a known bias to investigate, not a requirement that the finished game reward only reactive play. Broader meaningful initiative needs its own evidence and repetition limits before expanding eligibility.

## Files changed

- Added `Assets/Game/Combat/HeavyPractice.cs`, `EnemyImpactContext.cs`, `IAttackResolver.cs`, and Unity-generated `.meta` files.
- Added `Assets/Game/Editor/PracticeSandboxBuilder.cs`, `PracticePlayModeChecks.cs`, and `.meta` files.
- Added Unity-generated `Assets/Game/Scenes/PracticeTest.unity` and `.meta`.
- Updated `HitData.cs`/`MeleeHitDetector.cs` for source identity; `CombatObservation.cs`/`CombatVitality.cs`/`EnemyVitality.cs` for frozen impact context; `SandboxEnemy.cs` for strike timestamps and reset revisions.
- Updated `CombatActions.cs` for the optional resolver, `AttackSnapshot.cs` for an immutable recovery copy, and `EnemySandboxHUD.cs` for optional practice feedback.
- Updated current-assignment and progression documentation. The user-provided talent charter is preserved.

Movement/camera code, prior scene files, player/variant attack assets, CombatStamina, Starter Assets, Packages, and ProjectSettings are unchanged. The optional effect is present only in PracticeTest.

## Automated checks

With scenes saved and Play mode stopped, choose **Tools > EvolvingRPG > Run Practice Play Mode Checks**. The six isolated groups test rejection/duplicate/reset rules, pre-impact phase capture, completion/interruption, threshold/effect isolation, and disabled-resolver fallback. A normal-frame check then waits for an opponent windup, lands Breaker, finishes recovery, checks the bounded two-point result, and resets the trial. Reports go to ignored `Logs/PracticePlayModeChecks.txt`.

Validation completed in Unity 6000.6.7f1: all six isolated practice groups and the normal-frame practice check passed. Regression checks also passed: 26 baseline combat checks, six isolated technique groups plus three native-motor checks, 11 isolated opponent groups plus a live check, and eight snapshot groups. No runtime errors were captured by these checks. Reports are in the ignored `Logs` directory. Functional checks do not establish mastery feel, fairness, or balance; those require player testing.

## Retained action review (2026-11-06)

PracticeTest now offers F7 to show/hide a six-attack session review. F1 hides opponent details to provide more room; Escape releases the cursor for scrolling. This is a diagnostic overlay, not a pause or safe-development menu: the world continues. Closing F7 does not change action input, trial state or earned practice. No new button is required to fight.

Each completed/interrupted attack retains its actual committed base recovery, applied outgoing impact, incoming impact while committed, first received phase and completion state. The base recovery is the accepted snapshot, including the existing optional F2 trial if eligible; later F2 changes cannot rewrite the earlier record. It is labeled committed, not time already paid after an interruption.

R still clears practice points, consumed-threat budgets, practice decision history and the trial toggle. The separate presentation review survives R so an off/on comparison can be inspected afterward. F6 still clears only raw combat diagnostics. Review entries cannot give points, re-open a consumed threat or affect resolution, and nothing is saved. Leaving Play starts a new review.

Tools > EvolvingRPG > Inspect Practice Review Fixture (Ctrl+Shift+Alt+F10) runs the existing practice checks and one real Breaker guard interruption, resets the encounter, then leaves its retained review visible. It hides opponent details and releases the cursor; stop Play when finished. The two F2 model snapshots are explicit production actions; synthetic setup evidence used by the older isolated eligibility checks is not labeled player playtesting.

For a human comparison, earn the existing trial normally, use a matched starting context, alternate F2 off/on and inspect committed base recovery plus actual contact, incoming impact and return to control. Keep miss recovery separate. An 8% change is a prototype treatment, not an accepted owned talent or an automatically meaningful benefit.
