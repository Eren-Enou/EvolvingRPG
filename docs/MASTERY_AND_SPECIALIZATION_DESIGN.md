# Mastery, investment and specialization

Design proposals for future comparisons. The [talent charter](EVOLVING_TALENT_SYSTEM.md) establishes deep growth, player ownership, explainable discovery and distinct identities. It does not settle the formulas, resource names or commitment policies proposed here. Current HeavyPractice is a session experiment, not this production system.

**Depth-first refinement:** [DEPTH_FIRST_DESIGN.md](DEPTH_FIRST_DESIGN.md) supplies the current design priority: deepen a selected discipline's applications and judgment while retaining its liabilities. [The Heavy study](HEAVY_DISCIPLINE_DEPTH_STUDY.md) is the worked investigation. The map below remains an idea inventory, not a list to implement cumulatively. Continued familiarity must not mint unlimited major-focus capacity or require buying every sibling application.

## Give each progression signal one job

| Signal | What it tells the player | What it must not silently do |
| --- | --- | --- |
| Familiarity/mastery | This character has developed a technique through relevant use | Buy a branch, classify personality, or transfer to a new character |
| Discovery knowledge | A coherent new possibility has become understandable | Activate an effect or spend a resource |
| Investment resource | The character can deliberately choose among developments | Prove skill at the chosen technique merely because a mission paid it |
| Development commitment | Some deep choices compete with other deep choices | Hide an irreversible lockout until after purchase |
| Encounter opportunity | A short-lived contextual follow-up is currently possible | Survive reset, become character mastery, or start an automatic attack |

The first progression UI can show these distinctions with a few cards. They do not require five permanent combat meters or five wallet currencies. A commitment limit is a build rule, not necessarily a spendable item.

## Meaningful practice beyond action counts

Keep ordinary successful Heavy use relevant; do not make only rare guard breaks worthwhile. Contextual accomplishments can reveal paths without multiplying all progress. Missing an attack can teach the human player even if it does not award a mastery point; the interface should describe the outcome neutrally rather than scold the player.

Candidate eligibility comparison:

| Policy | Strength | Failure mode | Experiment |
| --- | --- | --- | --- |
| Current recent-strike budget | Deterministic, small, rejects dummy clicking | Rejects initiative and may encourage waiting for an enemy to attack |
| Authored encounter opportunity budget | Can recognize opening play and varied outcomes | Needs encounter lifecycle and replay-safe persistence |
| Impact-weighted budget | Can relate reward to actual contribution | Large damage builds dominate unless normalized; can penalize support |
| Completion-only award | Easy to inspect | Ignores useful trades and can encourage delaying a kill |
| Qualified impact with interruption retained | Respects a successful interruption/trade | Rewards reckless exchange if consequence is too small |

Test one alternative against current rules before replacing them. None is a complete anti-farming solution.

Recommended next economy experiment after mission identity exists: give each authored encounter a small Heavy-practice budget, with eligible contacts spending from it. Allow an opening contribution against a genuinely engaged opponent before its first strike, but require source/target attribution and actual impact. Distinct valid outcomes can explain what was practiced; they do not refill the budget.

An encounter's budget is fixed when that instance is created. Do not refill it when the enemy heals, leashes, changes phase, reloads a checkpoint or is individually reset. If durable progress is retained, the spent budget must be retained too. A new deliberate mission attempt may supply another budget, allowing repeatable growth; whether excessively easy replay is productive requires a separate comparison.

Do not base the budget on the player's current health, taking avoidable damage or elapsed stall time. Do not require all playstyles to demonstrate every outcome type on every mission. Novelty bonuses should not turn legitimate long-term specialization into permanently reduced progress.

## Prevent rewards from distorting combat

Check these incentives before adding a curve:

- Finishing an encounter should not yield less total practice than keeping its last enemy alive to generate strike sequences.
- A player should not need to let a dangerous action finish rather than interrupt it, solely to unlock eligibility.
- A guard specialist should not be forced to equip a weak action and deliberately avoid guard breaks to farm contacts.
- A missed discovery prerequisite should not permanently erase progress in ordinary familiarity.
- Resetting or dying must not reopen budget while preserving all gains.
- High HP targets and rapidly healing targets should not become unlimited practice batteries.

Debugging should show eligibility and budget exhaustion without implying that further fighting is worthless. Once an encounter's practice budget is spent, mission progress, loot and player learning can remain valuable. Whether the player-facing UI needs to expose a hard budget is an open question; never show an XP promise that the system will not honor.

## A small deliberate-investment prototype

Proposed first choice/save fixture:

1. A fresh Warrior has ordinary Light, Heavy and Dodge with no purchase required.
2. A controlled milestone grants one development token exactly once.
3. Breaker and Momentum are revealed alternatives, each costing that token. The UI compares behavior and drawbacks.
4. Preview either for free in a non-rewarding practice area. Preview exit restores the committed character configuration.
5. Purchase is explicit and saved. A discovery later reveals a possibility, but another valid resource/condition is needed to learn it.

This is a transaction test, not final resource pacing. Do not publish a huge economy around a fixture token. For the first prototype, a reversible debug respec restores the same token and explains dependent-effect deactivation; it must not grant duplicates.

## Three specialization policies worth comparing

| Policy | Identity mechanism | Experimentation cost | Risk |
| --- | --- | --- | --- |
| Deep focus capacity | Major learned developments consume scarce character commitment; broad fundamentals do not | A safe-location focus change with explicit affected prerequisites | Becomes only a loadout restriction if every peak can be freely owned/swapped |
| Path investment | Advanced development requires sustained prior character investments in its branch | Respec may refund some investments and require requalification | Early mistakes can become punishing; grinding currency may eventually buy all peaks |
| Anchored discipline | A selected deep discipline grants eligibility for its peak applications and blocks competing peaks | Changing anchor is an explicit character-development decision | Can feel like a disguised permanent class if alternatives are too constrained |

Do not choose by intuition alone. Prototype capacity first because it is easy to reverse, but evaluate what the character can learn and become, not just which effects are currently equipped. If the character can freely own and instantly access all peaks, capacity has not satisfied the charter's identity requirement.

Concrete comparison fixture: four commitment units. One deep development costs three; an adjacent support costs one; two deep developments cost six and cannot coexist. This allows deep-plus-support or a broader shallow selection. Four is a fixture value, not the final capacity. Do not let indefinite mastery mint unlimited capacity; that would eventually remove the distinction being tested.

Compare free outside-combat rebuilding with hub-only rebuilding and an explicit deep-focus change flow. Avoid permanent loss in early experiments. Once actual character identity depends on the rule, surface reversibility, dependency effects and replay implications for user decision before implementation.

## An illustrative authored map

This is a finite content sketch, not a committed graph or a list of implemented talents. IDs below are candidate durable identifiers for a later catalog; display names can change.

| Candidate ID | Role | Knowledge / prerequisite idea | Behavior / opportunity cost |
| --- | --- | --- | --- |
| `warrior.heavy.breaker` | Visible development | Ordinary Heavy familiarity plus explicit investment | Planted poise pressure, longer commitment; existing debug variant is the comparison source |
| `warrior.heavy.momentum` | Visible development | Same early choice gate | Forward windup travel, lower poise, miss recovery; existing debug variant is the comparison source |
| `warrior.dodge.discipline` | Supporting development | Demonstrated basic defensive control; do not require perfect evasion for all Warrior growth | No effect finalized; needed as an explicit fixture only for the first discovery rule |
| `warrior.discovery.retaliation` | Knowledge rule | Momentum investment + defensive investment + verified evade/retaliate history | Reveals the next option; grants no effect |
| `warrior.heavy.retaliatory_momentum` | Contextual evolution | Revealed, relevant investments and deliberate purchase | Conditional added advance with target-scoped miss drawback |
| `warrior.light.measured_step` | Alternative Light development | Later Light-focused experiment | Directional preparation travel at reduced poise impact |
| `warrior.heavy.breaker_followthrough` | Contextual candidate | Own guard-break experience with planted investment | Optional follow-up; proposed poise payoff may be redundant during stagger and must be tested |
| `warrior.light.pressure` | Pressure candidate | Sustained live Light practice and later investment | Optional advancing pressure opportunity, exposure when it misses |

The dodge supporting development is intentionally not filled with a gratuitous numerical buff just to satisfy a prerequisite. If it has no independently worthwhile behavior, revise the discovery prerequisite rather than selling an empty node. The first explicit fixture can model an intended investment boundary without pretending the missing talent is production-ready.

There is no requirement that every node depend on Heavy, that all characters adopt one of these identities, or that each discovery requires exactly three repetitions. The small map tests boundaries and coherent connections before more content exists.

## Continuing mastery without runaway combat

Separate an unbounded history measure from the finite set of currently authored combat applications. Very large history can coexist with bounded numerical refinement. It cannot guarantee infinitely many unique animations or tactical effects from finite content.

One mathematical candidate for a bounded refinement is:

```text
refinement(M) = cap * M / (M + K)
```

M is eligible mastery, K is a tuning scale, and cap is the maximum improvement for that one authored field. At M = K, the character has half the cap; at M = 9K, nine tenths. Increasing history has diminishing returns. The scale and cap are proposals, not replacements for the current five-point/8% toggle.

For a recovery refinement, derive from the authored baseline and apply an explicit minimum recovery:

```text
effectiveRecovery = max(minimumRecovery, authoredRecovery * (1 - refinement(M)))
```

Never repeatedly apply the formula to an already refined value. Never improve the explicit miss drawback as an accidental side effect. A bounded curve can still ruin combat if its cap crosses a timing threshold: a few frames may turn a risky trade into a guaranteed punish. Balance against encounter windows, not only percentage size.

Consider four sources of continued growth:

| Source | Why it can matter | Practical boundary |
| --- | --- | --- |
| Bounded control refinements | Existing action becomes more reliable or efficient | Preserve meaningful exposure and resource decisions; diminishing rewards may become imperceptible |
| New authored applications | A familiar discipline solves a new contextual question | Requires content and interaction testing; cannot be generated without constraint |
| Deeper selected developments | Greater commitment changes technique expression | Competes with other depths; cannot unlock every peak through time alone |
| Historical recognition | Record of the character's actual experience | Useful identity/history, but cosmetic records alone do not fulfill all promised mechanical growth |

The design tension remains real: infinite countable ranks are easy; indefinitely meaningful non-inflationary gameplay is a content and balance problem. Do not market an asymptotic percentage or repeating cosmetic title as a solved infinite evolution system. Prove the first several meaningful developments, then evaluate the cost of adding another.

## Cross-discipline interaction rules

Begin with authored connections, not arbitrary combinatorics. An advancing strike and an evasion technique can interact because their timing and travel have a designed relationship. Two shared tags are not sufficient evidence that any pair should generate a talent.

Each added connection needs: source investments, valid experience, what becomes known, what the player chooses, precise runtime trigger, once-only consumption, drawback, incompatible effects, and presentation coverage. A content tool can later validate these fields without becoming a universal effect scripting language.

For early combinations, allow one base technique and one tested contextual interaction on an action. Keep familiarity refinement explicit. Adding another interaction is a new experiment, not a default right of every learned node. Trigger effects must not recursively award their own prerequisite evidence.

## Knowledge, respec and history

Proposed policy: knowledge once discovered is not erased by respec; learned prerequisites and active eligibility are re-evaluated. A known evolution can become locked or inactive with a clear reason. Its historical journal entry remains true about what the character did at that time.

If a player refunds a parent investment, dependent purchases cannot remain silently active. Compare an atomic dependent refund with retaining ownership but deactivating the dependent option. These have different economic consequences; choose one before real save data is created. Preview the entire change before confirmation.

A new character can consult account knowledge but begins with its own empty investments and familiarity. Importing an archive entry never creates a fulfilled character experience receipt. Whether archive knowledge makes a map node immediately visible or exposes only a journal clue is a later UI comparison.

## Future economy acceptance tests

Compare two equal-resource characters; verify independent histories. Repeat a mission, abandon it, reset an enemy, replay a checkpoint and reopen the debrief. Test that ordinary practice remains possible but the same reward event cannot be replayed. Attempt to buy without prerequisites, double-click purchase, refund a parent and reload after each step.

Player questions: Did you understand why progress occurred? Did the first purchase matter? Was there a credible alternative? Could you explore a support technique without erasing your specialty? Did a higher mastery character still need positioning and timing? Did pursuing rewards make you fight less naturally?

All policies, formulas and content candidates in this document remain unimplemented and untested.
