# Development experiment backlog

Proposed sequence, not a release schedule or a standing instruction to implement everything. All work in this design expansion is documentation only. The user's **do not run Unity** restriction remains in force for this assignment; test procedures below are future work, not executed checks.

[MILESTONES](MILESTONES.md) controls actual assignments. [The atlas](GROWTH_DESIGN_ATLAS.md) explains how these experiments connect. No new runtime code is required merely to maintain this backlog.

[FIRST_COMPLETE_SLICE_CONTRACT.md](FIRST_COMPLETE_SLICE_CONTRACT.md) gives the acceptance walkthrough for the first complete progression and mission demonstrations. [TABLETOP_PLAYTHROUGHS_AND_EXPLOITS.md](TABLETOP_PLAYTHROUGHS_AND_EXPLOITS.md) exposes predicted integration risks; these are paper scenarios, not executed tests.

## Current implementation versus future work

| Area | Current status | Next uncertainty |
| --- | --- | --- |
| Movement, camera, jump, Q/E | Implemented; user previously tested the foundation | Preserve through later slices |
| Light, Heavy, dodge, target | Implemented; user described fluid combat and acceptable starting timing | Continued interruption investigation |
| Two opponents | Implemented; user gave positive foundational feedback | Readability and fairness in combinations |
| Three Heavy debug techniques | Implemented; prior functional checks reported | Their comparative usefulness needs player evidence |
| Heavy practice / recovery trial | Implemented in PracticeTest; prior functional checks reported | Eligibility fairness, incentives and perceivable benefit |
| Defensive attribution and discovery | Specification only | Can reliable, explainable evidence be captured? |
| Character investment, saves, hub, missions, loot | Design only | Prove narrow full loops before expanding content |

No previously reported functional pass is a test performed during this documentation assignment.

## Dependency path

```text
Existing technique/practice feedback
  -> defensive evidence
  -> contextual knowledge reveal
  -> optional effect comparison
  -> deliberate investment + save/reload
  -> minimal hub/mission/settlement
  -> two-character specialization comparison
  -> one style presentation pass
  -> selected equipment / ability / enemy expansion
```

Some pure design and authoring can proceed in parallel, but keep one major live behavior change under comparison. A missing system should not be disguised by three unrelated additions.

## P0 — Preserve and understand the present baseline

**Question:** Which existing proposals are worth connecting to progression?

Future session: compare Baseline, Breaker and Momentum against each opponent, then compare current practice trial off/on. Keep stamina permissive. Record interrupted actions, misses, guard breaks, safe punish opportunities and player explanation. Capture the parameter set and selected scene.

Deliverable: a short player-feedback record with retain/revise/discard for each technique and each practice rule. No new talent graph. Stop if a technique has no credible tradeoff or basic controls regress.

Dependency: none beyond existing playable scenes. Programming is not automatically the next useful step; some open questions need actual player evidence when the user chooses to test.

## P1 — Defensive evidence, no rewards

Implementation prepared: see [EVASION_EVIDENCE_EXPERIMENT.md](EVASION_EVIDENCE_EXPERIMENT.md). Runtime and Editor sources pass offline compilation. The manual scene builder, isolated/lifecycle/physics fixtures and native-motor checks still require the user to execute them in the Editor. This is not a passed gameplay gate or authorization for P2/P3.

**Question:** Can the system distinguish a threatened starting position avoided during dodge from a harmless miss?

Implement only C1 in [the discovery specification](CONTEXTUAL_DISCOVERY_EXPERIMENT.md): shared pure strike query, source-specific threat identity, bounded dodge/trajectory evidence, conservative receipt and explanation. Copy a separate scene with Unity authoring APIs when implementation is assigned; preserve earlier scenes.

Future functional cases: ordinary evasion, empty-space dodge, wall, zero travel, hit during dodge, second attacker, interruption, reset, low-frame-rate incomplete samples, duplicate callback. No practice points or reveal from this system yet.

Exit: classification is reproducible and rejected cases are understandable. Stop if the classifier relies on "enemy missed recently" or rewards missing samples.

## P2 — One contextual knowledge reveal

**Question:** Can experience reveal a coherent option without granting power?

Add explicit session investment fixtures, one rule and a small journal. Reuse P1 receipts and completed same-opponent Momentum outcomes. Separate visibility from acquisition. Show causal explanation and prerequisites. New-character reset differs from encounter reset only in this new experiment scene.

Future cases: identical evidence yields identical knowledge; missing prerequisite, late hit, foreign target, duplicates and changed fixture reject; reveal does not spend or activate. Test delayed evidence without extending event-time windows.

Exit: the player can deliberately investigate the hint and explain the reveal. Stop if the optimal behavior is artificial empty dodging or knowledge secretly equips an effect.

## P3 — One optional retaliation-effect trial

**Question:** Does the revealed interaction add a meaningful positioning choice?

Implement C3 only after P1/P2. Use acceptance-linked consumption and committed target context, not a side effect in Resolve. Keep the effect toggle distinct from purchase. [Timing worksheets](COMBAT_TIMING_WORKSHEETS.md) identify potentially unsafe fast-opponent retaliation and late miss exposure.

Future cases: accepted versus rejected input, expiry, invalid target, bystander hit, wall-clipped advance, interruption, mid-swing toggle and unmodified older scenes. Compare effect disabled/enabled with one fixed baseline. Do not enable the practice recovery refinement in the first effect comparison.

Exit: useful in a recognizable situation, drawback matters, and no automatic follow-up is required. Stop if it makes every successful dodge a compulsory RMB or eliminates commitment.

## P4 — Deliberate investment and a minimal talent view

**Question:** Can the player make and understand a real character choice?

Introduce stable catalog IDs, one character-owned state record, two visible choices, one controlled token, explicit purchase and dependency explanation. A few cards and one journal panel are enough. Keep debug fixture mode separate from owned mode.

Before closing this slice, resolve the temporary defensive-investment prerequisite: select a worthwhile supporting development or explicitly revise the discovery rule to use genuine defensive practice. Do not sell an empty fixture talent. Also inspect whether the first-choice familiarity gate rejects competent opening play; P7's later durable-budget work does not justify an unexplained first-choice dead end. The later discovery purchase needs its own controlled resource grant in the test; do not leave the only revealed option permanently unaffordable.

Future cases: purchase, insufficient resources, already owned, stale preview, double input, missing prerequisite, incompatible configuration and safe respec fixture. Test ordinary combat with no purchased variant. A revealed node must be able to remain locked, available or learned independently of visibility.

Exit: two fresh Warriors can choose differently and the UI truthfully describes each state. Stop if display labels are saved IDs or the UI mutates shared action assets.

## P5 — Minimal versioned persistence

**Question:** Does the small character survive reload without duplicated or leaked power?

Persist P4 ownership, actual mastery and knowledge with schema version and stable IDs. No active combat opportunities. Use a small validated save envelope and recoverable prior copy. Define migration and missing-content handling before treating player records as durable.

Future cases: save/reload two different characters, rename display label, missing definition, older schema, unsupported future schema, corrupt latest copy and interrupted write. Populated archive information must not fill a fresh character's experience or learned state.

Exit: reload preserves legal state with safe explanations for unresolved content. Stop if an error silently clears a character or if a new scene creates another first-time reward.

P4/P5 together complete the first meaningful purchase-and-reload boundary. P5 is part of V0.4's first full progression loop, not postponed until all mission features are finished.

## P6 — One hub, one contract, one return

**Question:** Is combat worth repeating in a game-shaped loop?

Build the minimal [hub and relay mission](FIRST_HOURS_AND_CHARACTER_JOURNEYS.md) using one authored route and current enemy roles. Mission attempt identity, objective state, completion/failure and a controlled exactly-once milestone reward are required. Equipment loot can be omitted initially.

Future cases: deploy, complete, fail, voluntarily abandon, checkpoint reload, return, debrief replay, scene transition and retry. Explain retained versus unsecured progress. Retained practice and its anti-replay budget must save together.

Exit: the player knows why they are entering the site and has a meaningful decision on return. Stop if repeat traversal or reward screens consume more attention than the combat choice.

## P7 — Initiative and durable practice incentives

**Question:** Can practice reward genuine opening play without rewarding stall/reset abuse?

Compare the present per-enemy-strike rule against a small authored encounter budget. Preserve earlier comparison. Include a preemptive hit against an engaged opponent, guard break before its first completed strike, useful interrupted trade and immediate victory.

Future cases: leash/reset/heal, repeated checkpoint load, retained gains after defeat, killed target, trivial target and repeated missions. Inspect total progress per encounter, not just points per hit.

Exit: completing encounters is not penalized relative to prolonging them. Stop if the best route is letting an enemy attack indefinitely or resetting the same reward event.

## P8 — Specialization and a supporting hybrid

**Question:** Can depth and experimentation coexist without universal peak builds?

Use equal resources and the four-unit commitment fixture from [specialization design](MASTERY_AND_SPECIALIZATION_DESIGN.md). Compare deep-plus-support against broader shallow investment; reject two peaks if their combined commitment exceeds the fixture. Then compare the proposed rebuilding policies in separate sessions.

Future cases: purchase preview, incompatible deep investment, parent refund, retained discovery knowledge, inactive learned effect and fresh character. Include both guarded and fast encounter questions.

Exit: a player can explain the opportunity cost and still explore an adjacent technique. Stop if capacity is just a hotbar limit while all peaks are freely owned and instantaneously accessible. Permanent identity decisions require explicit review.

## P9 — One style expression pass

**Question:** Can a spectator recognize the developed technique and predict its commitment?

Author one planted and one advancing presentation with consistent anticipation, impact and recovery. Use the same tested mechanical timings. Add audio/shape support and optional feedback intensity controls before broad VFX expansion.

Future cases: camera distance extremes, wall, enemy overlap, low visual contrast and interrupted action. Ask an observer to distinguish technique and active timing without opening build data.

Exit: presentation makes mechanics easier to read. Stop if motion conceals contact timing, feedback breaks camera control or a pose implies protection that does not exist.

## P10 — Select one expansion, not all of them

| Candidate | Prerequisite | Small deliverable | Stop condition |
| --- | --- | --- | --- |
| Mobile Light | Stable Heavy comparison | One directional Light variant with a real poise tradeoff | It replaces dodge or becomes mandatory Heavy setup |
| Pressure identity | Light role understood | One optional pressure opportunity, no forced combo | Automatic rhythm outperforms deliberate choices everywhere |
| Equipment | P5/P6 state/reward boundaries | Two authored weapon profiles, later one real item choice | Loot bypasses investment or visual reach lies |
| Active ability | Stable poise/trade model | One situational tool versus a no-new-button alternative | It becomes compulsory pre-cast upkeep |
| Second enemy role | Existing mixed encounters readable | One new pressure with explicit counterplay | It requires an unimplemented defense or unseen information |
| Stamina balance | User resumes resource tuning after interruption work | One controlled regeneration/cost comparison | Multiple simultaneous changes hide the cause |
| Regional development | Reliable mission persistence | Two authored patrol profiles and an announced transition | It silently counters the player's specialty |

## P11 — Second contract and return-player test

**Question:** Does the loop remain interesting after the first discovery?

Use familiar geometry with one changed objective/composition, not every parameter randomized. Run an informed new Warrior with no inherited power, and a developed Warrior with their saved history. Compare routing, opening selection and hub decisions.

Exit: the repeat player can skip exposition but still has meaningful choices; the developed character behaves differently without trivializing every threat. Stop if all value comes from a reward-rate multiplier.

## P12 — Audit the cost of another evolution

**Question:** Can authored growth continue at a sustainable content cost?

Choose one candidate from [the content workshop](CURATED_EVOLUTION_CONTENT_CARDS.md). Record definition work, evidence needed, compatibility cases, animation/audio coverage, save effects and regressions. Compare it to reusing an existing application in a new encounter.

Exit: the new development adds a recognizable decision, its interactions remain bounded, and maintenance cost is understood. Stop if each node requires bespoke exceptions across movement, input, rewards and saves.

## Experiment record template

```text
ID / question:
Assigned scope and exclusions:
Source parameter set / scene:
Unchanged comparison:
One major variable changed:
Known unsupported cases:
Future functional cases:
Player observations (blank until tested):
Actual changes in decisions:
Unexpected incentives:
Retain / revise / discard:
Decision owner and date:
New dependencies, if retained:
```

Do not fill observations with predicted results. Code compilation is technical evidence only. A small number of sessions can reveal problems without establishing universal balance.

## Explicitly deferred

Full procedural talent generation, arbitrary effect scripting, an enormous map UI, multi-class breadth, an auction economy, account services, cooperative networking, persistent population learning, a huge equipment slot set and endless handcrafted animation trees.

They are not forbidden ambitions. Their design should be informed by a complete small loop and selected experiments. If multiplayer becomes an assigned requirement, review authority, time and persistence assumptions before expanding irreversible architecture.

No Unity launch, compilation, Play mode or executable gameplay tests were performed for this backlog. It records a path for future assignments, not completed work.
