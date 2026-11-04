# Design review register

Working register for the documentation expansion, 2026-11-03. No entry below is player-approved merely because it has a detailed specification. This file separates established direction, observed feedback and proposals awaiting evidence.

## Established direction and current constraints

The vision and talent charter require shared understandable fundamentals, player-selected development, explainable discovery, deep identity with meaningful hybrids, information-only account knowledge, and growth that changes play. They do not establish exact talent costs, classes, respec rules, formulas or infinite content production.

The user previously accepted corrected jumping and a working combat baseline, described combat as fluid, gave positive feedback on the opponent/playstyle foundation, and requested continued development. That supports experimentation; it does not establish final balance or acceptance of every later technique and practice rule.

Current explicit constraints: do not run Unity during this assignment; preserve stamina tuning for frequent-action/interrupt testing; preserve imported assets, current controls and prior scenes. Design and documentation can expand without promoting a proposal to a runtime requirement.

## Priority questions

| ID | Question | Alternatives / current candidate | Smallest evidence needed | Decision status |
| --- | --- | --- | --- | --- |
| R1 | What qualifies as meaningful practice? | Current recent-strike rule versus authored encounter budget; completion versus retained qualified impact | Compare initiative, useful trades, stall and reset incentives | Open; current rule remains an isolated trial |
| R2 | What evidence supports an evasive discovery? | Conservative threatened-position proxy versus a richer swept threat model | False-positive/unsupported-case diagnostics before rewards | Open; C1 specification is a candidate |
| R3 | How does deep identity constrain breadth? | Capacity, path investment, anchored discipline | Two equal-resource characters plus adjacent support | Open; four-unit fixture is not policy |
| R4 | How reversible are major choices? | Free safe-location rebuilding, hub-only rebuilding, explicit deep-focus change | Exploration friction versus meaningful identity | Open; do not impose permanent loss in early prototypes |
| R5 | What survives a failed mission? | Immediate practice commits versus settlement at debrief; secured versus unsecured rewards | Checkpoint replay and reward/budget consistency | Open; failure policy must be visible before departure |
| R6 | How can continued mastery remain meaningful? | Bounded refinements plus authored applications and deeper selected expression | Several successive developments and measured authoring cost | Unsolved long-term tension; a formula alone is insufficient |
| R7 | What does account knowledge show? | Separate archive versus annotated character-map information | Fresh Warrior comprehension without inherited prerequisites | Open; no power transfer in either option |
| R8 | What scope does regional change have? | Character campaign first; eventual shared/account world requires separate review | One announced authored patrol transition across reload | Open; character-scoped state is only the initial proposal |
| R9 | What should equipment contribute? | Physical handling/tradeoffs versus mostly stats; narrow effects later | Two authored profiles with truthful presentation | Open; must not replace learned identity |
| R10 | Does an active ability add a decision? | Set Footing-style tool versus a contextual no-new-button alternative | Observe optional use, setup exposure and input burden | Open; no ability selected |

Do not convert an open question into a requirement by copying a candidate value into several documents. When a result changes, update the relevant specification and cross-links, not just a tooltip.

## Paper review findings

These are design deductions, not measured gameplay observations:

1. The earlier illustrative three-unit budget could not fit a three-unit deep development plus a one-unit support. The updated four-unit fixture can demonstrate the intended hybrid while excluding two three-unit peaks. No final capacity has been selected.
2. Ideal timing suggests fast-opponent retaliation may expose Momentum before its recovery finishes. Position, poise and actual samples still determine the result.
3. Extra poise immediately after guard break may be weak because the target is already staggered. The proposed follow-through needs revision or a more relevant context.
4. Evidence can arrive after an action is accepted. Knowledge may evaluate the recorded sequence later; an action effect cannot be applied retroactively. C3 uses only finalized evidence and retains the original expiry.
5. Any-target hit success is insufficient for a target-bound retaliation drawback. A bystander hit must not erase the cost of missing the linked opponent.
6. Durable practice plus resettable in-memory anti-farming state is inconsistent. Character gains and spent encounter budgets must share a persistence boundary if either survives reload.
7. C2's defensive-investment fixture has no selected standalone gameplay effect. A full choice/save slice must resolve that prerequisite rather than charge for an empty node.
8. A competent opening attacker may satisfy a mission while gaining no practice under a recent-enemy-strike gate. First-choice eligibility needs review before the opening is presented as complete.

[TABLETOP_PLAYTHROUGHS_AND_EXPLOITS.md](TABLETOP_PLAYTHROUGHS_AND_EXPLOITS.md) walks through these integration risks. [FIRST_COMPLETE_SLICE_CONTRACT.md](FIRST_COMPLETE_SLICE_CONTRACT.md) records what must be resolved before claiming a complete loop.

## Decision record template

```text
Question ID:
Alternatives actually compared:
Versions / parameters / scenes:
Functional evidence and limits:
Player feedback and source:
Decision: retain / revise / discard / remain open
Who decided / date:
Affected definitions, documents and saves:
Migration or reversibility consequences:
Next test, if unresolved:
```

Leave absent evidence absent. Use "predicted" for paper analysis and "observed" only for something actually tested. The user should not need to discover that a supposedly final system was only an illustrative table.

## Expansion guardrails

Do not expand the tree while ownership/reveal states are misleading. Do not add more enemy roles while current mixed threats are unreadable. Do not add loot randomization before one authored item choice matters. Do not tune stamina while using it to stress interruption edges. Do not add a second effect composer when one actor-owned resolver boundary can be extended deliberately.

If a candidate fails, keep any validated reusable evidence boundary and remove the unused behavior. Sunk implementation effort is not a reason to make a weak mechanic foundational.

## This assignment's change record

New design documents: GROWTH_DESIGN_ATLAS, FIRST_HOURS_AND_CHARACTER_JOURNEYS, MISSION_AND_ENCOUNTER_DESIGN, MASTERY_AND_SPECIALIZATION_DESIGN, EQUIPMENT_ABILITIES_AND_COMBAT_EXPRESSION, CHARACTER_STATE_AND_CONTENT_CONTRACTS, COMBAT_TIMING_WORKSHEETS, DEVELOPMENT_EXPERIMENT_BACKLOG, CURATED_EVOLUTION_CONTENT_CARDS, WORLD_AND_DISCOVERY_THREADS, TALENT_MAP_AND_JOURNAL_FLOWS, FIRST_COMPLETE_SLICE_CONTRACT, TABLETOP_PLAYTHROUGHS_AND_EXPLOITS, and this register.

Updated entry points: README, AGENTS, MILESTONES and PROGRESSION_DEVELOPMENT_PATH. Updated COMBAT_EVOLUTION_LAB's commitment example and CONTEXTUAL_DISCOVERY_EXPERIMENT's invalid-opportunity fallback/timing references for consistency. The user's talent charter and VISION are preserved.

No runtime implementation, scene/prefab configuration, package change or stamina change is part of this assignment. No manual Editor setup is required for these documents. No Unity launch, compilation, gameplay test or feel validation was performed. Document-link/format checks are separate static checks.

Static review completed: 32 documentation/entry-point files checked, 96 local links resolved, code fences balanced, and illustrative JSON parsed. Tracked documentation changes pass whitespace checks. A before/after SHA-256 comparison of 142 files under Assets/Game, Packages and ProjectSettings, plus VISION and the user's talent charter, found no changes. This verifies preservation during this pass, not runtime behavior or design quality.
