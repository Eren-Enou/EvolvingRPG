# World growth and discovery threads

Creative proposals for a future small campaign. These are not approved lore, generated missions or implemented world state. They extend the [opening journey](FIRST_HOURS_AND_CHARACTER_JOURNEYS.md) and [mission design](MISSION_AND_ENCOUNTER_DESIGN.md) without changing the vision or talent charter.

## A world that changes in useful ways

The world can evolve through changed access, threat composition, local understanding and visible consequences. It does not need to increase every enemy's health whenever the character improves.

Candidate framing: a frontier outpost studies routes that have become unreliable. The player brings back equipment, observations and recovered records. Some successes reopen paths; others expose a threat or reveal that a familiar enemy group has changed its approach. Combat experience and exploration inform possibilities for the character, while explicit choices determine which possibilities become part of the build.

Avoid making "evolution" a universal explanation for every arbitrary mutation. Each local development needs a comprehensible cause or an honestly unresolved mystery. A creature can adapt within an encounter, a patrol can change equipment between missions, and a region can change after a milestone; those are separate mechanisms with different evidence and reset rules.

## One small regional arc

The Relay Approach is a candidate first site. Its narrative can be told through recognizable geometry and a few hub conversations, not a large cinematics pipeline.

| Contract | Situation | Combat purpose | World consequence | Character-development opportunity |
| --- | --- | --- | --- | --- |
| First survey | Recover a missing route record | Establish committed strikes and two basic opponent roles | Reliable map/report available in the hub | Understand ordinary Heavy before choosing a development |
| Reopen the approach | Clear a constrained route or secure an open route | Compare geometry using the chosen technique | Short return connection or safer deployment point | Explore movement/commitment tradeoffs without a mandatory discovery |
| Changed patrol | Investigate announced guard/formation variation | Test adaptation while keeping the specialty viable | Regional report gains an explained patrol profile | Qualitative disruption or evasion history may contribute to authored rules |
| Recover the relay core | Reach an optional deeper chamber | Combine two known pressures with a clear objective | Unlock a new contract, not an automatic power multiplier | First repeatable opportunity to pursue a known clue |
| Hold the connection | Face a guardian/elite only after its grammar is ready | Demonstrate an identity against several familiar questions | Outpost has a functioning route; next region becomes a future lead | Review chosen specialization and one adjacent support |

This is a possible content order. It does not authorize a five-mission implementation now. The first complete slice needs only the first contract and enough return state to prove the loop. Reused geometry is acceptable if the changed decision is real.

The route choice should not determine permanent talent access. A player who selects the open route can later visit the passage. Different experiences may suggest different clues, but missing one contract variant cannot permanently eliminate a viable character direction.

## Show a consequence before adding another system

After the first relay success, the hub's board can gain a concise report and a visible route marker. Later, a repaired practice area can offer better comparison tools, not free mastery from dummies. A returning survey worker can describe the changed patrol in terms the player can test.

These small changes make the hub feel connected to play without housing construction, a resource-management minigame or reputation grinding. Add an NPC interaction only when it gives a useful decision, understandable consequence or compelling piece of the mystery.

A visible report should distinguish rumor from confirmed information. If a region changes on a fixed authored milestone, do not claim that its enemies analyzed the player's actual dodge habits. If a patrol change really uses recorded local events, the cause can be explained without revealing all future content.

## Candidate regional state model

Start with a finite authored state per region:

```text
Unsurveyed -> Surveyed -> Route reopened -> Relay restored
                   \-> Patrol change announced -> Patrol investigated
```

These are orthogonal facts where appropriate, not necessarily one linear number. Restoring a relay need not erase an unresolved patrol observation. The state selects offered contracts and a small number of encounter profiles.

Initial scope proposal: region state belongs to the current character's campaign. Another Warrior begins its own sequence, using account knowledge only as information. Do not accidentally transfer mission unlocks, rewards or encounter history through the account discovery archive. Whether eventual shared-world or account-wide regional progress exists requires a separate design decision.

Once a mission is accepted, freeze its advertised profile for that attempt. Reloading cannot reroll an easier composition while retaining all rewards. Changes between attempts are announced. A future dynamic event system must honor this stability or explain a deliberate exception.

## Three discovery threads

### Thread A — How defenses fail

The player encounters a guard, sees that frontal HP damage and poise pressure are different, and learns that commitment can create an opening. Ordinary practice builds familiarity. Disruption investment and meaningful guard-break history can later make a related option understandable.

Clue: "Your committed strikes are exposing weaknesses in guarded stances." It should appear only when the rule has real supporting evidence. The exact future development may be a wider disruption technique or a positioning follow-up; do not promise both before they exist.

The reveal explains relevant history without defining the player's class. A player may choose not to become more disruption-focused. A veteran can investigate the same thread on another character, but must establish that character's own investment and experience.

### Thread B — Returning through an opening

An enemy visibly commits, the player moves out, and an advancing strike returns to that opponent. Once defensive attribution works, invested evasion and Momentum can reveal Retaliatory Momentum. This thread already has a [narrow future specification](CONTEXTUAL_DISCOVERY_EXPERIMENT.md).

Clues must not praise unverified dodges or count merely surviving nearby. If a wall stopped the strike, the journal can report the actual geometry rather than attribute it to an evasive technique. The world should not train players to game a vague "dodge often" counter.

### Thread C — Making room under pressure

A later mixed encounter teaches that breaking one opponent's poise is not the same as being safe from the other. A disruption-focused character may become interested in controlled withdrawal or a supporting spacing technique.

This thread needs real multi-threat evidence before claiming defensive success. Initially it can be a visible offered development or a narrative suggestion based on a completed mixed encounter. Do not invent a causal rule from the player being far away after a guard break.

The important result is a choice between pressing the opening and using it to reset position. The exact mechanic remains exploratory; [Breach and Withdraw](CURATED_EVOLUTION_CONTENT_CARDS.md) is a candidate, not a promised reward.

## Mystery without arbitrary prerequisites

A useful mystery gives the player an actionable direction: a reported phenomenon, an inspectable connection, an encounter worth investigating. It does not demand obscure standing positions, exact hidden frame counts, a rare missable drop or a one-time dialogue answer for core viability.

An optional unusual discovery may have demanding context, but provide repeatable ways to attempt it. Hints can describe the relevant relationship without exposing a checklist of hidden thresholds. After reveal, explain generally what mattered. If an event fails to qualify, developer diagnostics should identify the missing condition; player messages can remain concise.

The journal should not fabricate a destination from a vague clue. If a silhouette represents an authored future option, it must correspond to real content. If the system only has a thematic observation, present it as an observation instead of an unlock promise.

## Expansion directions after the relay

**Depth-first gate:** retain the first site's decision vocabulary until one lineage has a meaningful accepted application and varied uses. A second contract can recontextualize existing roles before any second environment is selected. The environments below are dormant contrasts, not a sequence of required biomes. See [DEPTH_FIRST_DESIGN.md](DEPTH_FIRST_DESIGN.md).

These are contrasts to explore later, not a world map commitment.

| Candidate environment | New question | Required foundation before building it | Avoid |
| --- | --- | --- | --- |
| Exposed quarry terraces | Spacing, cover and approach lines | Reliable slopes, navigation and camera handling; ranged role if used | Narrow fall traps that dominate action combat |
| Overgrown interior | Visibility, route choice and constrained commitment | Camera occlusion readability, sensible corridor encounters | Enemy threats hidden by foliage and unreadable VFX |
| Broken ceremonial arena | Multiple authored patterns and a strong culminating foe | Multi-attack telegraphs and tested elite poise rules | A pure HP/damage gear check |

Do not create all three to establish variety. A second site should answer a question the first cannot, and reuse validated action/role grammar. A new biome with identical decisions is presentation growth; valuable eventually, but not proof of systemic depth.

## Keep long-term progression attached to play

An experienced Warrior might return to the relay for a different authored contract, pursue a known interaction with a new technique, compare an equipment choice or help resolve a later regional development. Each reason should have a real gameplay consequence or satisfying objective.

Avoid endless mandated low-risk repetition solely to fill a mastery bar. If the old site becomes tactically solved, offer another question rather than secretly scaling every enemy to keep damage ratios constant. If no new content exists yet, acknowledge that the prototype's current content is finite.

An indefinitely extensible system is not an infinitely large finished game. The development path should demonstrate that adding a meaningful new application is feasible, then choose how much content to author. Do not promise procedural discovery will automatically solve content production.

## Future review questions

- Did the hub change help the player understand the world's response?
- Was a contract interesting because of its play or only because it paid a larger reward?
- Could a chosen specialty still succeed after the patrol changed?
- Did a clue suggest a coherent experiment without requiring external instructions?
- Was the world-state scope clear when creating a second Warrior?
- Did a route choice feel meaningful without becoming a hidden permanent build lock?

No lore, mission chain, regional state or discovery thread here is established beyond the existing vision. All requires selection, implementation and player feedback.
