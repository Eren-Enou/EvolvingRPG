# Proposed V0.4 C — Evasion to discovery

Status, 2026-11-05: C1/C2/C3 have playable implementations and supported native Unity checks. [DISCOVERY_PLAYGROUND.md](DISCOVERY_PLAYGROUND.md) records actual controls/behavior; [OVERNIGHT_PROGRESS_REPORT.md](OVERNIGHT_PROGRESS_REPORT.md) records validation and remaining focused input/player checks. This document retains the design rationale and proposals; values are not finalized. PracticeTest remains independent.

## Hypothesis and smallest deliverable

A player who deliberately invests in an advancing Heavy and demonstrates actual evasive retaliation can receive an understandable clue about a new interaction, without automatically learning or equipping it.

Build this in three independent steps: C1 defensive evidence only; C2 knowledge/journal only; C3 optional combat-effect comparison. No full tree, paid assets, invulnerability, attack cancellation, stamina rebalance or save system is required by these steps. Investment fixtures must be explicitly labeled session-only; real purchase and save/reload follow in V0.4 D.

## Why C1 was needed

Before C1, CombatObservation recorded accepted/completed/interrupted dodges without threat association or trajectory. SandboxEnemy misses did not distinguish dodge, range, facing or a wall, and outgoing raw HitData had no committed strike identity. C1 now supplies a separate bounded diagnostic receipt and incoming identity. EnemyImpactContext still describes the enemy hit by the player; it is not incoming defensive evidence.

Therefore neither "dodge completed" nor "dodge followed by Heavy" is successful-evasion evidence. Do not reuse the practice points, latest enemy timestamp or no-damage outcome as a substitute.

## C1: proposed authoritative evidence boundary

The implementation guide above is authoritative for current bounds. Verified receipts wait for both completions; known rejections or unverified interruptions/resets can finalize earlier. The following specification records the rationale, not a claim that Editor checks have passed.

Keep the current enemy geometry as the initial contract. Factor its range/cone/line-of-sight decision into a side-effect-free query used by both actual hit attempts and evidence evaluation. Actual hit application remains separate. This is not a weapon-volume collision upgrade.

Use the following encounter-local records, only when C1 is implemented:

| Record | Required meaning |
| --- | --- |
| ThreatKey | Enemy reference + encounter revision + committed strike sequence. Session-only identity; never a save ID. |
| ThreatCommitted | Intended player receiver, committed facing, range/arc, active interval and reset revision. Freeze relevant geometry parameters. |
| DodgeAttempt | Actor + accepted action sequence, start time, pre-travel capsule pose/shape and real movement samples through the threat interval. |
| ThreatResolved | Actual contact/damage result, active interval finished or interrupted, collision/visibility result, and whether required samples were complete. |
| EvasionReceipt | Linked threat and dodge keys, completion times, stationary-proxy overlap, actual-path safety, classification and explicit rejection reason. |

An initial deliberately conservative classifier qualifies only if all conditions hold:

1. The live enemy committed this strike against this actor. The dodge begins after commitment and before the active interval finishes; no idle evasion credit.
2. At an active sample, the shared query would contact a stationary proxy of the actor's pre-dodge capsule at the captured location, including line of sight. This establishes a specific threatened position rather than proximity alone.
3. The actual actor avoids contact throughout that strike's active interval, and the linked dodge completes. Include swept or sufficiently bounded substep checks for the actor path; reject incomplete evidence rather than granting credit at a low frame rate.
4. The same threat completes its active interval normally. An enemy interrupted, killed or reset before resolution does not prove evasion.
5. The actor is not hit by that strike, and is not defeated, reset or interrupted before the dodge completes. For the first discovery trial, any incoming hit during the evidence interval rejects it conservatively; explain this multi-attacker restriction.
6. The actual sampled motion shows dodge travel, not merely a wall stopping the enemy or another actor interrupting it. Geometry-blocked stationary-proxy attacks and zero-travel dodges reject.

This proves evasion under a limited stationary-position proxy, not that dodging was the only possible defense. Walking away may also have worked. Name the diagnostic honestly: "Committed strike avoided during dodge; starting position was threatened." Do not claim perfect timing, a near miss, skill rating, or general causal certainty.

Use the enemy's sampled current origin with frozen facing/parameters when evaluating a sample, and record enough information for the reason. The initial stationary proxy is not simulated physics: it must not move, block the real player or receive damage. If collision geometry changes or a blocker makes the comparison ambiguous, classify it unverified. Start with one fixed-origin committed enemy and reject unsupported moving-strike cases.

At low frame rates, a sampled miss can hide an unsampled collision. Define a maximum supported substep interval and motion bound before implementation, use the same checks for both paths, and report unsupported intervals as unverified. Do not claim frame-independent evasion from endpoint samples alone.

Finalize only after both the strike interval and full dodge recovery complete. Emit at most one evasion receipt per threat/actor. For the first experiment, at most one pending candidate per actor; reject a second overlapping threat explicitly rather than choosing whichever later grants a reward. Bounded storage exhaustion rejects safely. Reset, disable or target replacement clears candidates. Diagnostics clearing does not reopen consumed evidence.

## C2: one reproducible discovery

Add an explicit test build fixture with separate flags for Momentum investment and basic evasive-development investment. Number-row debug technique selection does not set these flags. Fixture changes occur only outside an action and are visibly labeled; they grant no permanent character power.

Proposed session rule:

- With both investments present, one qualified evade/retaliate exchange foreshadows the connection.
- Three distinct qualified enemy strikes reveal it. These counts are tuning fixtures, not a finished mastery formula.
- A qualifying exchange requires an EvasionReceipt followed by an accepted Momentum Heavy within 1.5 s of full dodge completion, an applied hit on that same opponent and encounter revision, and completed Heavy recovery without interruption. A reset opponent reusing the same GameObject is not the same eligible target generation.
- If threat resolution finishes after dodge completion, use the recorded Heavy acceptance time; delayed evidence processing must neither extend nor erase the actual timing window. Buffer only a bounded pending chain and finalize when all receipts exist.
- Capture prerequisite flags when the dodge starts and require them unchanged through Heavy completion. Mid-exchange debug investment changes invalidate the chain with an explanation.
- One threat supplies at most one discovery contribution. A Heavy contacting several targets cannot multiply it. Light, baseline Heavy, remote dummy hits, wall misses and interrupted Heavy actions do not qualify for this specific rule.

The input sequence alone never reveals the option. Conversely, do not require the optional evolved effect to discover itself. C2 uses ordinary Momentum and no effect.

Keep visibility separate from acquisition. Proposed display sequence:

| Knowledge | Player message | Acquisition/effect |
| --- | --- | --- |
| Undiscovered | Nothing promised | No effect |
| Foreshadowed | "Your evasive movement is beginning to shape your advancing strikes." | Still no effect; optional hint identifies relevant experience |
| Revealed | Retaliatory Momentum description, drawback, general cause and prerequisites | Known only; not learned or equipped |

Journal example after reveal: "You developed an advancing strike and repeatedly retaliated against an opponent whose committed attack you evaded." Developer details list evidence keys and rejection reasons; ordinary play need not expose a three-step farming checklist. The first prototype may expose exact counts in a debug panel for diagnosis.

In the isolated C scene, encounter reset clears transient chains/budgets but retains session knowledge and fixture investment; provide a separate clearly labeled Fresh Character operation that clears all session knowledge and fixtures. Do not change R behavior in existing PracticeTest. Explain both reset scopes before play. Nothing persists when leaving Play mode until D implements saves.

## C3: separately enabled effect trial

**Depth-first adoption gate:** C3 currently proposes an armed conditional form for the next eligible Momentum. [DEPTH_FIRST_DESIGN.md](DEPTH_FIRST_DESIGN.md) makes its intent policy explicit: compare a readable armed form, an existing-direction opt-in, or safe-location selected handling before treating conditional displacement as settled. The ordinary Heavy input must have a predictable result. [The Heavy study](HEAVY_DISCIPLINE_DEPTH_STUDY.md) supplies contextual cases and distinguishes taking an opportunity from always consuming it. No input policy has been player-validated.

Only after C1/C2 are understandable, offer a debug comparison toggle after reveal, explicitly labeled "trial, not purchased." It is not proof of ownership implementation.

For the proposal in the lab: an eligible evasion supplies one opportunity. If an ordinary Momentum Heavy is accepted within 1.5 s of full dodge completion, its snapshot gains 0.4 m windup advance and 0.25 s additional miss recovery. No damage, poise, stamina, invulnerability or dodge-recovery changes. Actual collision still clips travel; a wall is never bypassed.

Bind the opportunity to its opponent and encounter revision, and capture that target generation in the action context. The added miss drawback applies unless that generation receives an applied impact during the strike, even if a bystander or reset replacement is hit. Current global TargetsHit/MissRecovery cannot express that distinction: add explicit target-scoped outcome handling for this effect when implementing C3. Do not silently treat any-target contact as successful retaliation. A reset after acceptance does not rewrite the committed action; it invalidates eligible target success and knowledge credit.

An effect opportunity becomes available only when the EvasionReceipt is finalized. Its expiry remains full dodge completion plus 1.5 s; evidence processing does not restart the timer. Unlike knowledge evaluation in C2, an attack accepted before verification receives no retrospective effect. If the receipt arrives too late, report an expired opportunity. This conservative choice may make some enemy timings unsuitable; measure that gap and revise the experiment rather than changing a committed snapshot or granting an unverified bonus.

Consume the opportunity only after action acceptance succeeds. Failed input leaves it available until its original expiry. Consume on acceptance even if the strike later misses, is interrupted or the target dies; do not refund failed commitment. Reject the effect if the linked target has already reset, disappeared or become defeated. An expired or invalid opportunity permits ordinary legal Momentum with a clear diagnostic and no opportunity consumption; resolve this fallback before action acceptance. No stacking, refresh from the effect's own hit, recursive attacks or opportunity carryover across resets.

The effect may use existing snapshot fields for travel, but targeted drawback and opportunity identity require an action-scoped committed context alongside the snapshot. Keep source definitions unchanged and restore authored future behavior when the trial is disabled. Changing a toggle during a swing does not rewrite that action.

Before writing this effect, compare the proposed 1.5 s window with current dodge recovery, Momentum windup and opponent recovery. [COMBAT_TIMING_WORKSHEETS.md](COMBAT_TIMING_WORKSHEETS.md) supplies paper examples, not runtime validation. A technically valid window can still be tactically impossible or encourage suicide trades; revise one variable at a time after playtesting.

## Future verification cases

These are specifications, not executed tests.

| Case | Expected result |
| --- | --- |
| Dodge in empty space, outside strike range, or before enemy commitment | No verified evasion or discovery credit |
| Dodge through an actual hit, or take another opponent's hit during the evidence interval | Reject with incoming-hit explanation |
| Threatened starting position, safe actual path, completed strike and dodge | Exactly one conservative EvasionReceipt |
| Enemy strike blocked by wall; dodge blocked with no travel | No verified evasion |
| Enemy interrupted/defeated/reset before active interval completes | Unverified; no discovery contribution |
| Interrupted dodge; actor reset/disabled; incomplete low-frame-rate samples | Reject and clear pending chain |
| Q/E directional dodge, backward dodge, diagonal dodge | Same evidence rules; facing/camera behavior unchanged |
| Heavy accepted on time, hit same enemy, full recovery, prerequisites present | One contribution, deterministic clue/reveal |
| Late Heavy, different target, dummy, wrong technique, interrupted recovery, changed fixture | Explicit rejection; no knowledge change |
| Duplicate receipts, multi-target hit, F6 clear, individual enemy reset | No duplicate contribution or reopened budget |
| Reveal | No resource spent, no learned flag, no active effect |
| C3 rejected action; opportunity expired; successful acceptance | Preserve until original expiry; reject expired; consume once on acceptance |
| C3 hits bystander but misses linked opponent | Target-scoped drawback applies |
| C3 toggle changes mid-swing; wall clips movement; ordinary old scene | Committed behavior stable; collision respected; baseline unchanged |
| Fresh Character / future character-save isolation | No inherited knowledge fixtures, mastery, purchases or active opportunities |

Future Editor setup: create a separate DiscoveryTest scene with a Unity builder; configure one opponent first, explicit fixture and debug journal, effect off. Preserve prior scenes. Regress movement, jumping, strafing, camera, interruption, existing techniques and current practice rules before player testing. Add the mixed-opponent case only after single-threat classification is reliable.

## Exit decision

Retain only if the player can reproduce and explain the clue/reveal without artificial dodge spam, the optional evolution changes a positioning decision, and its cost is credible. If defensive evidence is too brittle, stop at C1 and improve the threat model; do not replace it with a timestamp reward. If the effect is unhelpful, keep validated evidence and discovery boundaries while revising or discarding the effect.

No Unity invocation, compilation or tests were performed for this specification. Implementation and gameplay validation remain future work.
