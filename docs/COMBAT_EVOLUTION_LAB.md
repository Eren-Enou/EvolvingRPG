# Combat evolution lab

Status: future experiment designs, 2026-11-03. Documentation only; no new runtime systems implemented or executed for this design pass. Current playable baseline remains V0.4 B in PracticeTest. Read with [the talent charter](EVOLVING_TALENT_SYSTEM.md), [the development path](PROGRESSION_DEVELOPMENT_PATH.md), and [current practice rules](HEAVY_PRACTICE_EXPERIMENT.md).

## Intended transition

The next step is to turn familiar actions into different tactical decisions. A developed Warrior should still use Light, Heavy, dodge, movement, and positioning, while seeking different openings and accepting different risks. Practice improves familiarity; discovery exposes possibilities; deliberate investment changes the character. These responsibilities stay separate.

Keep three comparisons available: ordinary combat, a technique with its tradeoff, and that technique with one proposed evolution. Do not use several improvements at once to conceal a weak underlying action. Current technique and practice experiments still need player feedback; functional checks do not settle their usefulness.

Established requirements are player agency, explainable discovery, distinct specialized identities, meaningful hybrids, and account knowledge without transferred power. Every named technique, number, gate, commitment rule, and reward below is a proposal. No permanent respec or specialization policy is selected here.

## System responsibilities

| System | Input and output | Existing foundation / next boundary |
| --- | --- | --- |
| Combat execution | Accepted input produces one committed action and resolved contacts | Keep CombatActions, AttackSnapshot, the existing motor, and hit resolution. Progression must not rewrite a running swing. |
| Encounter evidence | Actual exchanges produce bounded, source-specific records | EnemyImpactContext captures offensive context. Add verified defensive evidence before dodge-based discovery. |
| Familiarity | Eligible completed outcomes produce proficiency with explanations | HeavyPractice is a temporary trial. Its per-strike restriction and reset behavior are not a permanent economy. |
| Discovery | Character prerequisites plus contextual evidence change knowledge | Add one authored evaluator; reveal never purchases, equips, or grants a combat effect. |
| Investment | Explicit player choice changes character ownership | Introduce a controlled test resource and prerequisites in the choice/save slice. Debug number-key selection is not ownership. |
| Action resolution | Authored action plus valid selected development produces a snapshot | Reuse IAttackResolver; one actor-owned coordinator when a second effect is actually required. Do not add competing independent writers. |
| Identity expression | Chosen behavior is made visible and audible | Plan animation poses, travel, impact and recovery together; placeholder geometry does not validate final readability. |

The first complete loop needs these small boundaries, not a generic rule language or procedural talent engine.

## Candidate experiments

Latest steering prioritizes [depth within one discipline](DEPTH_FIRST_DESIGN.md). E1/E2 remain the narrow defensive-evidence/return-application candidates; use [the Heavy study](HEAVY_DISCIPLINE_DEPTH_STUDY.md) to evaluate them across familiar contexts. E3–E8 are conditional research options, not parallel work to unlock a larger tree. Presentation/clarity fixes required for a truthful comparison belong in the baseline; a later style pass is distinct from those fixes.

Each row is a separate comparison. Do not implement the whole table together.

| Experiment | Changed player decision | Benefit and cost to prototype | Evidence to collect / reason to revise |
| --- | --- | --- | --- |
| E1: verified evasion | Dodge out of an actual committed strike rather than dodge on cooldown | Initially evidence only; no bonus or invulnerability | Nearby harmless dodges, wall-blocked attacks, taking a hit, and interrupted dodges must not count. If players cannot understand the reason, fix evidence before rewards. |
| E2: Retaliatory Momentum | Evade a threat, then decide whether an advancing Heavy is safe | After an earned opportunity, add 0.4 m to Momentum's windup advance; add 0.25 s extra miss recovery. No damage, poise, or stamina change in the first trial | Compare useful pursuit against overshoot, walls, late retaliation and unsafe commitment. Discard if it is simply a mandatory free improvement. |
| E3: Breaker follow-through | Decide whether to exploit a guard break or use the opening to reposition | Following an own Heavy guard break, one optional Light has +25% poise and +0.10 s recovery; unchanged damage/range. Candidate window: 1 s after Heavy completes | Does Light become a distinct precise follow-up? Test a target with remaining poise, because bonus poise against an already staggered target may be worthless. If the only useful behavior is automatic button mashing, redesign the payoff. |
| E4: measured Light | Choose a deliberate spacing attack instead of always fishing for Heavy | A selected Light variant permits 0.4 m of directional travel during windup but has 25% less poise impact; unchanged damage and attack timing | Can it maintain spacing without becoming a free dodge? Test inward, outward, strafing and collision cases. Give it no invulnerability or cancellation privilege. |
| E5: sustained pressure | Choose to continue pressure or disengage when the enemy threatens retaliation | After two separately completed applied Lights against the same active opponent, the next Light has 0.3 m windup advance and 0.15 s additional miss recovery. Candidate opportunity expiry: 1.5 s | Does it support an optional pressure style without creating a mandatory three-hit combo? Test stopping after either Light. No forced continuation, recovery cancellation, or automatic strike. |
| E6: initiative and varied practice | Create an opening before an opponent's first strike | Compare the current reactive practice gate against a separate one-use encounter budget for a genuine opening hit | Verify that reset/re-engagement cannot mint durable rewards. Distinguish an active approaching opponent from a stationary dummy; never use distance alone as proof of danger. No persistent awards until encounter lifecycle rules exist. |
| E7: restrained enemy adaptation | Recognize an enemy adjustment and vary the approach | Compare the existing brace/backstep/flank memory against one clearly telegraphed response with a reliable alternative opening | Measure whether a specialty remains viable. No immunity escalation, input reading, hidden player-stat scaling, or permanent punishment for successfully specializing. |
| E8: visible style | Recognize another Warrior's commitment without reading a build sheet | One planted Breaker pose and one advancing Momentum pose, aligned to unchanged phase timings | Can an observer predict active impact and recovery? Revise animation if it masks gameplay timing. Do not increase power to compensate for unreadable presentation. |

E3–E5 are later alternatives, not additions required for the first vertical slice. E3's benefit must be tested against actual poise recovery; it may fail as written. E5 must remain an optional opportunity rather than a mandatory combo or a universal damage ramp.

## Detailed first evolution

[CONTEXTUAL_DISCOVERY_EXPERIMENT.md](CONTEXTUAL_DISCOVERY_EXPERIMENT.md) specifies E1 and E2: evidence, false positives, an explicit investment fixture, deterministic reveal, optional effect, and future test cases. It is the recommended next implementation specification after current baseline feedback. Start with E1 alone.

Do not require the player to hit an opponent immediately after a dodge until existing dodge recovery and Heavy windup leave a credible opportunity. The retaliation timer starts when the dodge's full recovery finishes, not at the beginning of travel. A later Heavy must still hit the same opponent and satisfy its own range, facing and collision checks.

## From one evolution to character identity

Proposed comparison: two fresh test Warriors receive equal controlled resources. One deliberately develops the planted disruption technique; the other develops the advancing technique. Let each encounter both the guarded and fast opponent and a mixed encounter. Compare choices, exposure, reliable openings, and perceived identity, not only completion time.

Then offer one adjacent supporting technique. A hybrid should open a new option without automatically gaining both disciplines' strongest developments. Prototype a finite commitment budget only for the experiment: e.g. four units, a deep development costs three and a supporting technique costs one. Deep plus support fits; two deep developments do not. This exposes a real tradeoff; equipped-slot limits alone do not establish specialization. The budget and costs are hypotheses, not a permanent progression system. See [the specialization comparison](MASTERY_AND_SPECIALIZATION_DESIGN.md).

Test two reversible development policies in separate sessions: unrestricted swapping outside an encounter, and swapping only at a test hub. Observe learning and identity without committing to permanent character choices. Never silently impose irreversible purchases in a prototype.

Long-term mastery should deepen control and new applications. Do not repeatedly multiply the current 8% recovery trial. Shortening all recovery indefinitely would erase commitment. Test one bounded refinement at a time, preserve meaningful minimum exposure, and spend later content effort on authored interactions and readable styles. The exact indefinite reward curve remains unresolved; a finite experiment cannot prove infinite progression balance.

## Controlled composition

When learned effects are connected, use explicit roles:

1. Select the actor's legal authored base technique.
2. Apply its one tested familiarity refinement, if enabled.
3. Apply at most one eligible behavior-changing interaction for the initial comparison.
4. Validate the resulting fields, then commit a single immutable snapshot.

This is a proposed initial restriction, not a permanent one-evolution limit. Resolution is a preview without side effects. Spend a temporary opportunity only when CombatActions successfully accepts the resolved action; a rejected RMB must not consume it. Existing IAttackResolver alone does not supply that acceptance transaction, so implement the smallest acceptance hook when E2 is enabled.

No proc recursively creates another attack or practice award. Use explicit source action and enemy strike identities, consume once, and never derive further trigger chains from an effect's own diagnostic notification. An expired opportunity does not queue another action. An active swing retains its snapshot even if a prerequisite or toggle changes during recovery.

## State lifetimes

| State | Lifetime | Reset / future persistence rule |
| --- | --- | --- |
| Threat, dodge evidence, retaliation opportunity | Current encounter and actor | Clear on reset, death, disable, opponent replacement; never save active opportunities. |
| Current Heavy practice trial | Current PracticeTest session | Keep existing R-clears-all behavior. It is not durable mastery. |
| Trial build/investment fixture | Future isolated discovery session | Explicitly distinguish from the current numeric debug selector; clear with a documented fresh-character operation. |
| Character mastery, learned developments, discovery knowledge | Future character save | Stable authored IDs, versioned data, fresh-character isolation, explicit migrations. Add in the choice/save slice. |
| Account discovery archive | Later informational archive | Knowledge only; no learned techniques, mastery values, or fulfilled character prerequisites transferred. |

Durable progression cannot share the present encounter-reset semantics. Split practice encounter budgets from character totals before adding persistence. F6 remains diagnostic clearing and must not change eligibility or knowledge.

## Future playtest method

Use separate copied scenes created with Unity APIs only when implementing a slice. Preserve CombatTest, EnemyTest, TechniqueTest and PracticeTest. No scene or prefab needs changing for this design pass.

For each future trial:

1. Keep stamina costs and regeneration at the current permissive settings. Establish an ordinary-action comparison with the evolution disabled.
2. Change only the chosen interaction. Alternate comparison order between sessions so familiarity does not always favor the evolved version.
3. Include a guarded opponent, fast opponent, wall/corridor case, intentional miss, damage trade, interruption, and reset.
4. Record opportunity created/consumed/expired, benefit actually used, misses, incoming damage, interruptions and the explanation. Retain bounded local records, not an analytics service.
5. Ask whether the effect changed a decision, whether its drawback mattered, and whether the player could explain the result without watching debug counters.
6. Decide retain, revise, or discard. Do not add another interaction just because the first is technically functional.

Short trials can expose broken incentives but cannot prove long-term farming resistance. Later test repeated missions, trivial opponents, reset abuse and content changes before durable rewards are enabled. Stamina balance is a separate later comparison; do not tune it while establishing which action interactions work.

## Implementation order and stop conditions

1. Review the existing technique and practice comparisons with the player. Preserve rejected rules as removable experiments.
2. Implement E1 threat/dodge evidence with diagnostics only. Stop if false-positive explanations are unreliable.
3. Add one E2 clue/reveal using explicit session investment fixtures. Stop if discovery feels arbitrary or rewards artificial dodge spam.
4. Add a separately toggled E2 effect for comparison. Stop if the tradeoff is irrelevant or the action becomes mandatory.
5. Connect deliberate purchase, minimal map/journal states, stable IDs and versioned character save/reload. Test two fresh Warriors.
6. Compare one specialization constraint and one adjacent hybrid; surface identity/respec tradeoffs before establishing rules.
7. Add one readable style expression. Expand Light/pressure experiments only when they answer a missing combat role.

## Open design questions

- Should a successful evasion grant only positional advantage, a discoverable option, or also an immediate follow-up opportunity?
- How much counterfactual evidence is fair enough to explain "you evaded this strike" with the present simplified hit geometry?
- Does completion-only familiarity unfairly discard good trades or preemptive play?
- Can Light support a distinct identity without becoming setup work that every Heavy build must repeat?
- Which developments need deep character commitment, and which should remain broadly accessible fundamentals?
- What would make continued mastery rewarding once timing and numerical refinements reach safe bounds?

## This design pass

Added this lab and the contextual-discovery specification; linked them from the existing development path and milestones, and recorded the documentation-only assignment in AGENTS.md. Read combat observations, enemy strike resolution, dodge phases, practice eligibility and the long-term charter. No runtime code, scenes, prefabs, authored assets, packages, project settings or stamina tuning changed. Unity was not launched or controlled, and no compilation, tests or Play mode were run. All future validation described above remains pending.
