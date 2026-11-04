# Missions, encounters and a world that responds

Future design proposal. Related: [opening journey](FIRST_HOURS_AND_CHARACTER_JOURNEYS.md), [combat experiments](COMBAT_EVOLUTION_LAB.md), [implementation backlog](DEVELOPMENT_EXPERIMENT_BACKLOG.md). The current SandboxEnemy is a small collision-aware direct-pursuit prototype, not a mission AI or navigation system. Nothing in this document has been built or run during this design pass.

## The first mission envelope

Use one compact authored site provisionally called the Relay Approach. It has an arrival clearing, an open yard, a short alternate passage, a relay chamber and a return/extraction point. It should support a full departure/encounter/reward/return loop with low traversal overhead.

First objective proposal: reach the relay, recover its survey record, and return. Interacting takes place only after nearby immediate threats are resolved. A carried object does not initially alter movement or occupy a new inventory grid. This isolates mission state from escort AI, defense timers and crafting.

Start with a fixed layout and explicit encounter placements. Later variations can change a route's blockage, objective location or composition one at a time. Seeded procedural layouts are not needed to test whether repeating a mission is interesting.

## Site grammar

```text
Arrival / return point
          |
   Clear approach fight
          |
    Visible route split
       /          \
 Open yard     Short passage
       \          /
       Safe observation ledge / staging point
                    |
             Relay encounter
                    |
        Short unlocked return connection
```

This is a connectivity sketch, not a scene file or a final navigation layout. The observation point must not create an unreachable ranged exploit if ranged actions are later added. The return connection is a pacing aid, not a reward duplication shortcut.

The open route allows lateral movement, longer approaches and room to see advancing strikes. The passage constrains overextension and makes line of sight concrete. Neither path should invalidate an entire legal build; both reconnect before the final objective. Show enough of each route for a player to make an informed choice.

Graybox evaluation uses the actual character, camera zoom limits, dodge distance and attack reach. Corridor width cannot be chosen from aesthetics alone. Test camera clearance, two combatant bodies passing, a dodge ending at a wall, and whether a telegraph remains visible beside a pillar. Current direct pursuit can get stuck behind walls, so early placements need unobstructed approaches until navigation is deliberately addressed.

Avoid stairs, low ceilings, tight doorframes and untested slopes in the first combat route. These multiply movement and camera failure modes without answering the mission-loop question. Add them later as geometry tests.

## Encounter cards

**Depth-first use:** first vary a meaningful context using existing actions/roles rather than adding more cards or enemy families. The [Heavy study](HEAVY_DISCIPLINE_DEPTH_STUDY.md) uses distance, opening length, travel line and a later visible threat to test the same lineage. Reusing the site is valuable when the player's judgment changes; repetition count alone is not depth.

Each card identifies a decision, not just a list of enemies. Counts are candidate starting fixtures.

### R1 — Learn a committed strike

- One fast-role opponent in a wide clearing, alone and visible before engagement.
- Question: can the player bait a strike, leave its committed direction, and choose a safe response?
- Ordinary walking must remain viable. No forced dodge or Heavy input.
- Failure test: the player is hit once, retreats, and can understand what happened without an unseen second attacker.
- Do not award a special "perfect evasion" unless the separate evidence system can prove its limited definition.

### R2 — Choose an approach

- One guarded-role opponent near the route split; initially no ranged support.
- Question: break guard, circle, or exploit recovery?
- Planted Heavy offers disruption; advancing Heavy helps re-enter from outside reach but has less poise impact. Both retain basic options.
- The frontal guard needs a readable boundary/pose. A flank cannot be an invisible damage multiplier that looks identical to a frontal hit.
- Failure test: repeated Light pressure causes the existing brace response, but poise pressure and positioning still offer progress.

### R3 — Manage two threats

- One guarded role and one fast role, separated enough to observe and pull deliberately.
- Question: isolate the mobile threat, maneuver around the guard, or accept pressure to force a break?
- Begin without attack-slot coordination so baseline simultaneous threats can be measured, but do not call the encounter fair merely because each enemy is individually readable.
- If overlapping telegraphs become unreadable, compare a small engagement coordinator in a separate test. It must not require enemies to politely take turns forever or allow permanent stun-lock on the only active attacker.
- Failure test: camera obstruction and offscreen damage are identified before increasing enemy health.

### R4 — Relay guardian, later

- One larger opponent reusing the committed-threat grammar before introducing new attack patterns.
- Candidate two actions: a directional sweep that rewards leaving its arc, and a slower planted strike that rewards recognizing a long recovery. Give distinct anticipation poses and sounds.
- Do not add unavoidable arena-wide damage, enrage timers or an explicit build-check shield in the first version.
- A disruption specialist creates earlier breaks; a movement specialist gets different retaliation positions. Both need repeatable safe ways to win using fundamentals.
- Future boss resistance may limit repeated stagger without hiding immunity. Test poise reset/recovery and an explicit brief post-stagger resistance state, rather than secretly raising thresholds whenever the player succeeds.

R4 depends on a multi-attack enemy model and presentation coverage; it is not required to prove the first hub/mission loop. The first relay encounter can simply be R2 or a carefully spaced R3.

## Enemy roles before enemy catalog

| Role | Pressure | Vulnerability | Readable cue | Content dependency |
| --- | --- | --- | --- | --- |
| Guarded anchor | Holds a direction, discourages frontal careless hits | Turning limits, poise break, recovery | Guard posture and planted strike | Existing guarded prototype; refine readability |
| Mobile pursuer | Closes and pressures long preparations | Committed facing, shorter but usable recovery | Shorter preparation and visible pursuit | Existing fast prototype; navigation later |
| Delayed punisher | Waits for obvious overcommitment, then attacks from its observed state | Bait and reposition; long own recovery | Visible ready stance, not secret input reading | Future decision model using visible action/position evidence |
| Ranged harasser | Makes standing still risky; shapes approach | Line of sight, cover, interrupted preparation | Visible projectile/beam preparation | Future ranged hit model and camera readability; deferred |
| Supporter | Alters priority by assisting another role | Clear limited-range connection and exposed support action | Connection cue with start/end | Future effect ownership/duration; deferred |

Create new roles only when existing ones cannot ask the desired question. A new silhouette with the same timing and response is art variety, not a new tactical role. Role combinations consume readability and animation budget as well as programming time.

## Fair adaptation at three timescales

### Immediate reaction

An enemy responds to observable position and ongoing action state. It does not read a mouse click before the action becomes visible. A reaction has a response time, turn limit and commitment. Do not alter a strike's facing after the rules say it is locked.

### Encounter memory

The current brace/backstep/flank responses are the starting experiment. Future variants should expose one bounded memory at a time. Example: after repeated same-side approaches, a fighter visibly favors that side during approach, leaving the other side available. It cannot rotate faster only when off camera.

Compare response enabled/disabled with the same player technique. Success means a player notices and adapts without abandoning the specialty. A response is not successful merely because it increases damage taken or mission duration. Show when memory expires or behavior relaxes.

### Regional development, much later

Candidate: completed expeditions affect an authored regional report that offers a different contract composition next visit. A patrol may carry a different guard tool, or a route may gain cover after activity at the relay. This uses explicit, versioned regional states; it is not unbounded learning from every input.

Surface a transition before departure: "Shield patrols have appeared on the inner approach." Do not silently replace all enemies with counters to the player's current build. Keep at least one familiar contract or viable way to exercise the specialty. Provide causal narrative only for changes actually driven by the player's/world's recorded state.

First population experiment: alternate between two authored patrol profiles after one clearly named mission milestone. Measure whether the change feels meaningful and fair. Persistent player-tactic analysis, global ecology simulation and online world services remain deferred.

## Mission state and rewards

Candidate lifecycle:

```text
Offered -> Accepted -> Deployed -> Objective secured -> Extraction -> Debrief -> Closed
                         |                                  
                         +-> Defeated / Abandoned -> Debrief -> Closed
```

One mission attempt has a stable attempt ID. Encounter instances and reward settlements are identified within it. Changing scene, reloading a checkpoint or replaying a debrief must not silently create a new eligible settlement.

Separate at least these outcomes conceptually:

- **Practice:** qualifying technique experience from genuine exchanges, with encounter budgets that survive a checkpoint reset if the resulting character progress is retained.
- **Investment resource:** proposed milestone/mission reward for deliberate character development. Do not make a raw hit count the only source of every progression system.
- **Equipment or materials:** mission rewards with their own acquisition rules; not proof of technique mastery.
- **Knowledge:** a revealed possibility retained according to the chosen character persistence policy; never automatically converted to learned power.

First slice can omit loot and award only one controlled development resource at a completed milestone. Its debrief still needs to demonstrate exactly-once settlement. Rewards for failed runs should be an explicit design choice, not an accidental side effect of where Save is called.

## Replay without compulsory chores

Try three authored contracts on the same geometry only after the initial loop works:

| Contract | Changed decision | Stable elements | Risk to watch |
| --- | --- | --- | --- |
| Recover record | Reach objective and return | Core route and enemy grammar | Becomes a short optimal farming script |
| Clear route | Choose order of two separated threat groups | Same map, different encounter priority | Encourages killing irrelevant enemies for completion padding |
| Investigate patrol change | Compare a telegraphed alternate enemy profile | Familiar objective/landmarks | Feels like punishment for prior specialization |

Optional objectives should offer a reason to vary play, not demand a particular talent. Do not label an objective optional if it supplies the only practical character-growth resource. No daily login schedules or escalating streak rewards are needed to validate repeatability.

## Encounter feasibility worksheet

Before authoring a new attack, write down its windup, active interval, recovery, facing behavior, reach, arc, poise effect and obstruction rule. For each intended response, evaluate travel time, player windup, range and commitment. A response requiring more time than the opening provides is not made fair by its tooltip.

For example, a Heavy's total commitment may exceed the enemy's recovery even if the hit lands before recovery ends. The relevant question is whether the enemy's next visible commitment permits escape or causes an unavoidable trade. Tests must include the whole exchange, not just whether the first contact succeeds.

Record encounter-level uncertainty: two enemy timelines can overlap, pathing can compress distance, collision can shorten dodge travel, and the camera can hide cues. Use these observations to adjust encounter composition and presentation before giving the player unconditional damage resistance.

## Future mission acceptance

A complete first mission should let the player leave the hub, understand the objective, encounter at least two distinct decisions, complete or fail cleanly, return, receive one consistent result, make a character choice, and replay without duplicated rewards or lost state.

Retain a mission only if a repeat visit with another legal development produces a different approach while remaining viable. If the only difference is how long identical attacks take to reduce HP, improve roles, openings or geometry before expanding the region.

No mission, enemy role extension, regional adaptation, reward transaction or hub behavior described here has been implemented or tested in this design pass.
