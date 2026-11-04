# Development path toward evolving character progression

Status: specification-aligned architecture audit and proposed experiment sequence, reviewed 2026-11-03. Following the user's instruction to continue developing with judgment, the current slice is **V0.4 B: Heavy practice trial**. This does not settle the earlier technique proposals or authorize the full progression system.

## Source alignment

Read for this plan: `VISION.md`, `MILESTONES.md`, `EVOLVING_TALENT_SYSTEM.md`, `COMBAT_SANDBOX.md`, and the existing combat implementation.

`EVOLVING_TALENT_SYSTEM.md` is the long-term design charter. Its established intentions are project requirements; its examples, suggested prototype scope, and unresolved mechanics remain proposals. This plan translates that distinction into implementation boundaries and candidate experiments. `MILESTONES.md` remains the source for the currently assigned work. Preserve the Unity version declared by `ProjectSettings/ProjectVersion.txt`; older milestone text does not authorize an engine change.

Established principles supported by the vision and talent charter:

- Character identity develops through choices, experience, and discoverable adaptations; there is no final build to finish chasing.
- Playstyle may influence which options are revealed. The player retains agency to choose them.
- Discovery follows consistent, inspectable rules rather than arbitrary surprises.
- Growth should change available decisions and interactions, not merely inflate numbers or invalidate successful playstyles.
- Curated foundations, indefinitely extensible mastery, adaptations, and evolutions are distinct parts of the long-term direction.
- Warriors share understandable starting fundamentals, then become visibly different through meaningful practice and deliberate investment.
- The talent map is partially revealed. Visibility, eligibility, ownership, and activation must remain distinguishable.
- Deep specialized power involves consequential choices and opportunity costs. Hotbar limits alone do not satisfy this requirement.
- Account discoveries convey information only. Fresh characters must develop their own mastery, prerequisites, investments, and power.
- Light and Heavy remain recognizable actions as they evolve; complexity must not require mandatory extra key combinations.

Exact formulas, thresholds, tree shapes, specialization restrictions, mastery rewards, respec rules, and reveal mechanics remain proposals until the specification and playtests establish them.

### Requirement traceability

| Charter sections | Established requirement | Consequence for the development path |
| --- | --- | --- |
| 1–4, 8, 12 | Shared fundamentals; distinct, recognizable fighting identities; mastery and evolution have different jobs | Compare behaviorally different Heavy variants against the same baseline. Evaluate changed decisions and visual commitment, not just damage. Illustrative identities such as Breaker are not mandatory subclasses. |
| 3, 5–6 | Discovery reveals possibilities; players choose investments; partially revealed information is honest | Keep discovery separate from acquisition. Represent visibility independently from eligibility/ownership, with explanations for both. |
| 4, 6, 11 | Meaningful outcomes, coherent contextual discovery, reproducible diagnostics | Use resolved encounter evidence rather than action counts. A dodge followed by Heavy is not automatically proof of a successful evasive dodge. |
| 2, 7 | Depth over universal breadth, meaningful hybrids, room to experiment | Test competing deep investments later. Do not equate limited equipped slots with specialized character development or impose permanent respec rules without design review. |
| 9 | Account knowledge and character ability are separate | Test a fresh character with no inherited power. Keep any future archive informational and separate from character saves. Full account services are deferred. |
| 10–11 | Clear map/journal/build presentation; stable IDs, persistence, compatibility checks | Add only enough prototype UI and versioned state to explain and preserve the validated slice. Detect incompatible effects and guard recursive triggers before permitting combinations. |
| 13–14 | Small, playable validation before scale | Build the first complete loop from separate experiments. Include minimal save/reload in that loop rather than waiting for mission persistence. |

## What the existing implementation supports

| Existing component | Useful foundation | Gap before progression depends on it |
| --- | --- | --- |
| `AttackDefinition` | ScriptableObject timing, cost, damage, range, and movement data | DisplayName is presentation, not a durable ability identifier. Character upgrades must not mutate shared assets. |
| `CombatActions` | Explicit phases, immutable committed attack parameters, actor-local acceptance/outcome observations | A started attack is not evidence of a hit. Future temporary effects need acceptance-linked consumption, not a side effect in preview resolution. |
| `MeleeHitDetector` | Range/facing/obstruction checks, per-swing deduplication, generic `HitReceiver` boundary | Existing receipts support the practice trial; target-bound follow-up success needs its own committed context. |
| `TrainingTarget` | Independent damage/poise resolution and hit/reset events | Training outcomes and resets must not accidentally become production advancement rewards. |
| Movement and action adapters | Preserve the current controller and camera behavior | Avoid adding talent-specific branches to movement scripts. Resolve future action changes before execution. |
| `HeavyPractice` | Session-only explicit Heavy credit/rejection, duplicate/reset protection, optional bounded refinement | Not durable mastery or a settled eligibility economy; encounter and character reset scopes must split before persistence. |
| `EnemyImpactContext` / enemy vitality | Frozen pre-hit phase, source engagement, threat sequence and encounter revision | Offensive impact context does not prove that an incoming strike was evaded. |
| `IAttackResolver` | One optional actor-owned snapshot preview boundary | Keep preview side-effect-free; extend narrowly when real ownership or temporary effect commitment is assigned. |

There is currently no durable character progression state, production mastery ledger, talent graph, discovery evaluator, or progression persistence. HeavyPractice is a transient experimental ledger, not a substitute for those systems. Do not add placeholder implementations for all of them now.

V0.3 introduces incoming telegraphed enemy strikes and player vitality. Dodge completion still does not establish successful evasion: the sandbox does not yet attribute an enemy miss to a particular defensive action. Meaningful evasive evidence still needs an explicit outcome definition during enemy/progression experiments; elapsed time between dodge and attack is insufficient by itself.

## Accepted combat baseline and regression reference

The user accepted V0.2 as a working baseline and requested V0.3. Preserve CombatTest as a regression reference before introducing progression effects. Automated checks cover functional behavior; only player feedback establishes feel.

Playtest questions: Are Light and Heavy different enough to motivate choices? Does Heavy's commitment create useful positioning decisions? Can Q/E and directional dodge be used intentionally without fighting the camera? Are stamina recovery and stagger clear enough to reason about?

Record each experiment's tuning values, the behavior being tested, player observations, and the next change. Adjust one major variable at a time. Keep a known baseline so future progression can be compared against ordinary combat rather than concealing problems in it.

If V0.2 debugging requires action observations, add only the observations needed for that check. Do not award XP, discover talents, infer fighting-style identities, or introduce an event-bus framework as part of that work.

**First code slice:** `COMBAT_OBSERVATIONS.md` documents the implemented actor-local observation contract and bounded training diagnostics. It distinguishes accepted actions, actual hit receipts, completed attacks/misses, interruptions, and completed dodges without granting progression or claiming successful evasion. This makes baseline playtests inspectable while leaving meaningful enemy-context validation and progression eligibility for their assigned experiments.

**Second code slice:** `ATTACK_SNAPSHOTS.md` documents immutable parameters captured at attack acceptance. Execution, feedback, movement restrictions, and historical evidence now retain those values even if the authoring definition changes. This closes an existing consistency gap in V0.2 and provides the eventual per-character action-resolution boundary without adding talent effects or changing combat tuning.

**Combat acceptance support:** `MELEE_MISS_FEEDBACK.md` documents completed-miss explanations for range, facing, obstruction, and unavailable targets. This makes baseline positioning/commitment playtests easier to diagnose. These training explanations do not qualify outcomes for mastery or contextual discovery.

## Proposed independently playable experiments

These are candidate experiments aligned with charter sections 13–14, not approved balance rules or immediate assignments. Keep individual slices playable, then combine their validated parts into the first complete progression loop.

| Stage | Small playable slice | Evidence needed before expansion |
| --- | --- | --- |
| V0.3: meaningful combat context | Existing planned enemy behaviors create an opening that makes Light, Heavy, dodge, and positioning matter. Keep player progression disabled. | Players can identify why an action worked. A future style system would observe deliberate choices rather than forced repetition. |
| V0.4 A: technique variation | Baseline Heavy plus two debug-selectable, behaviorally different variants against the same opponent. Candidate examples: a planted disruption strike and an advancing strike with a miss drawback. | Different positioning/commitment decisions and credible tradeoffs are visible during play. Existing controls still work. Debug switching is an experiment tool, not a permanent respec policy. |
| V0.4 B: one mastery track | Heavy records explicitly eligible outcomes in a meaningful encounter. Show why each outcome qualifies or is rejected. Trial one configurable proficiency effect. | Credit is deterministic, deduplicated, and cannot be obtained by idle dummy clicking alone. Reward formulas and anti-farming thresholds remain tunable proposals. |
| V0.4 C: one contextual discovery | One authored dodge-to-Heavy interaction considers relevant investment and validated encounter evidence. Present one clue/reveal and an explanatory journal entry. | Identical evidence produces identical knowledge; missing prerequisites are diagnosed. Revealing neither spends a resource nor changes the active build. |
| V0.4 D: deliberate investment and minimal persistence | Connect the two visible choices, one mastery track, and one discovery through a small talent-map/debug interface. Use a controlled test resource. Save and reload the character's investments, mastery, and discovery state. | Visibility and acquisition remain separate. Costs/prerequisites are enforced. A new Warrior has no inherited mastery, purchases, or fulfilled prerequisites. Reload preserves state without mutating shared action definitions. |
| After the first loop: specialization and expression | Trial competing deep investments and a meaningful hybrid, then one distinctive stance/animation or rhythm change. | Peak branches cannot all be acquired through effortless swapping; an adjacent technique does not erase a character's specialty. Surface respec/identity tradeoffs for design review. |
| V0.5: mission persistence integration | Integrate the proven small character-save model into the hub/mission loop and rewards. | Mission transitions preserve validated state. Account information, character power, and transient encounter evidence remain separately scoped. |

An indefinite mastery direction does not require infinitely many authored nodes in the first prototype. A finite experiment can test a rule intended to remain extensible; its reward curve and long-term balance still need separate validation.

The first completed loop should let two fresh Warriors choose different Heavy developments, experience one reproducible discovery, deliberately invest if eligible, and retain their own state after reload. A full account archive and large navigable tree are not required for this prototype. Small UI coverage must still demonstrate honest revealed/locked/available/learned distinctions and foreshadowing; if limited active configurations are adopted, learned and active must also remain distinct.

## Proposed architecture boundaries when progression begins

1. **Definitions:** authored ability, talent, and discovery data with stable IDs independent of labels. Use Unity asset references for authoring; avoid relying on names or runtime object IDs for saved identity.
2. **Character state:** selected options, mastery progress, and discoveries belong to each character. Keep this separate from shared ScriptableObject definitions and any account-level knowledge archive. Model visibility (undiscovered/foreshadowed/revealed) independently from acquisition (locked/available/learned and, if adopted, active configuration).
3. **Action resolution:** derive the effective action parameters for that character when committing an action. Preserve that snapshot through the action so mid-swing unlocks cannot change its timing or damage unexpectedly.
4. **Outcome observations:** record accepted actions and resolved outcomes separately. Include only the action/target identity and context needed by the first experiment. Retain per-action identity for duplicate protection if one swing produces several callbacks. Never classify a dodge as a successful evasion without authoritative encounter evidence, or treat a training-target reset as eligible combat progress.
5. **Discovery evaluation:** evaluate explicit, testable conditions against observations and character state. Emit eligibility with an explanation; selecting an eligible option remains a separate player choice.
6. **Persistence:** add a versioned local character-save boundary within the first complete progression prototype, after its state model survives the isolated experiments. Test fresh-character isolation and missing/changed content definitions. Do not create an account-wide service framework before deciding what must persist.
7. **Compatibility and presentation:** use explicit compatibility rules and bounded trigger execution for the few authored effects. Present prerequisites, costs, drawbacks, reveal explanations, and actual ownership honestly. Plan animation coverage separately from gameplay composition; code modularity does not supply finished animations.

Implement only the boundaries used by the next experiment. Avoid a universal modifier engine, procedural talent generator, hidden personality classifier, global singleton event bus, or networking architecture in advance of demonstrated needs.

## Next development step and decision gates

**Latest focus: depth within a discipline.** [DEPTH_FIRST_DESIGN.md](DEPTH_FIRST_DESIGN.md) and [HEAVY_DISCIPLINE_DEPTH_STUDY.md](HEAVY_DISCIPLINE_DEPTH_STUDY.md) prioritize a single advancing-Heavy lineage across familiar contexts, with planted disruption as an independent comparison. The broader candidate list is dormant unless an observed missing decision justifies promotion. The two visible Heavy choices remain part of the small progression comparison; this does not assume both already have equally deep authored content. Keep depth-within-one-lineage and balance-between-lineages as separate evaluations.

**Broader design expansion:** [GROWTH_DESIGN_ATLAS.md](GROWTH_DESIGN_ATLAS.md) now connects this progression path to the proposed first sessions, hub/mission loop, equipment, abilities, visible styles and regional change. [DEVELOPMENT_EXPERIMENT_BACKLOG.md](DEVELOPMENT_EXPERIMENT_BACKLOG.md) provides bounded assignments; [DESIGN_REVIEW_REGISTER.md](DESIGN_REVIEW_REGISTER.md) records policy alternatives and paper-review findings. The user's no-Unity restriction applies to this design assignment. No new runtime features, compilation or gameplay results are claimed.

**Documentation-only follow-up:** At the user's request to develop future systems and ideas without running the program, [COMBAT_EVOLUTION_LAB.md](COMBAT_EVOLUTION_LAB.md) develops bounded combat comparisons, Light/Heavy/dodge roles, specialization proposals and state lifetimes. [CONTEXTUAL_DISCOVERY_EXPERIMENT.md](CONTEXTUAL_DISCOVERY_EXPERIMENT.md) specifies the next candidate in three steps: verified defensive evidence, one reproducible knowledge reveal, then an optional retaliation-effect comparison. These are future specifications, not implemented systems, approved balance rules or evidence that V0.4 A/B feel has been validated. No Unity execution occurred during this follow-up.

**Current update:** V0.2's baseline has been accepted for continued development, and the user gave positive feedback on V0.3's opponent foundation. V0.4 A compares three Heavy techniques in TechniqueTest. The user's subsequent instruction to continue with judgment supports an isolated V0.4 B practice experiment in PracticeTest, with proposed meaningful-outcome credit rules and a toggleable bounded recovery effect. See `HEAVY_PRACTICE_EXPERIMENT.md`. Leave stamina unchanged, preserve original scenes, and do not treat any proposed formula or technique as validated merely because functional checks pass.

The current-slice step is **V0.4 B meaningful-practice comparison**: can completed live Heavy exchanges produce understandable, non-repetitive credit and one bounded proficiency change? Earlier technique usefulness still needs player comparison. Preserve both experiments independently so a practice rule can be revised without committing to the Heavy variants. Stamina tuning remains explicitly deferred. Contextual discovery needs separately defined defensive evidence and knowledge/ownership rules; it is not inferred from these practice points.

V0.4 A implements the planned baseline Heavy and two authored behavioral variants as a debug comparison, following the user's request to continue from the opponent foundation. It reuses the committed snapshot and adds only zero-default advancement/miss-recovery parameters. Introduce stable identity when saved state requires it, and resolve any future character-specific parameters before commitment. Mastery, a talent graph, and an archive are not part of this variation slice.

Each experiment requires: a concrete hypothesis, an unchanged baseline comparison, explicit non-goals, deterministic diagnostics/tests, a manual playtest, and a decision to retain, revise, or discard the mechanism. Escalate design choices affecting identity, irreversibility, or replayability before treating them as rules.

Open questions retained from charter sections 2 and 15:

- Which mastery outcomes qualify, and how should deliberate practice differ from repetitive farming?
- How should a player investigate an undiscovered option without either arbitrary guessing or a checklist dominating play?
- Which specialization/evolution choices coexist, and which are exclusive or reversible?
- What persists between missions, and which rules must remain stable as authored content evolves?
- Are combat styles explicitly configured, inferred from investments, or both? How many evolutions may affect one action simultaneously?

## Changes and validation for this planning step

Updated this document and MILESTONES.md after reading the supplied talent charter. Removed the missing-document gate, mapped requirements to their source sections, corrected the first experiment to compare two variants, and brought minimal character persistence into the first progression loop. No runtime code, scenes, prefabs, package settings, shared attack assets, or the user's talent charter were changed. This was a source/design review; Unity compilation and Play mode were not run for this documentation-only step.

The subsequent implementation step added combat observations and regression checks as described in `COMBAT_OBSERVATIONS.md`; its validation status is recorded there. The earlier documentation review and this code slice remain separate from progression-system implementation.
