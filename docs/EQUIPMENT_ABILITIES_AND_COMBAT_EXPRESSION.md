# Equipment, abilities, stamina and combat expression

Future design candidates. Nothing here changes current assets, costs, regeneration, inputs or animation. Start with [the design atlas](GROWTH_DESIGN_ATLAS.md); implement only a selected experiment from [the backlog](DEVELOPMENT_EXPERIMENT_BACKLOG.md).

**Depth-first hold:** the near-term [Heavy discipline study](HEAVY_DISCIPLINE_DEPTH_STUDY.md) fixes equipment, resources and active-ability count. The candidates below remain dormant until a specific missing decision earns expansion. Do not use independent gear slots to patch every limitation of a developing specialty.

## Protect the source of character identity

Character investment should define learned applications and deep technique. Equipment can affect reach, tempo, protection or situational opportunity. A single item should not supply another discipline's peak development while bypassing that character's investments.

Conversely, items should not be irrelevant decoration. A weapon with a visibly different handling profile can make the same learned technique ask a different spacing question. Introduce this only after that technique is understood with one stable weapon.

Proposed ownership boundary:

| Source | Primary job | Avoid |
| --- | --- | --- |
| Fundamental actions | Shared playable language | Charging currency for basic WASD, jump or recognizable attacks |
| Learned development | Changes in tactical application and style | Replacing every identity choice whenever a higher-level item drops |
| Mastery | Familiarity and tested bounded control improvements | Unbounded damage/recovery scaling that makes the authored action irrelevant |
| Weapon | Physical handling and a small readable set of stats | Invisible reach changes unsupported by visual contact |
| Armor | Protection and a visible handling tradeoff | Creating a universally best defensive stat with no action consequence |
| Active ability | One deliberate situational tool | A compulsory cooldown rotation that eclipses manual attacks |

## Minimal equipment experiment

Do not begin with a full slot set, rarity ladder, random-affix generator and crafting economy. First compare two authored weapon profiles on one mesh family in a non-rewarding test: standard and longer-reaching. Let the longer profile have a credible cost, such as longer commitment, with its animation and reach indicator updated to match.

A later actual-loot slice can have one weapon slot and one armor slot, two authored choices per slot, a compact equip comparison and a stash with an explicit capacity. These are scope proposals, not the final gear layout. Start with deterministic rewards so behavior can be compared before drop-rate tuning.

Before adding loot, establish item instance identity, definition identity, inventory ownership and exactly-once reward settlement. Do not equip an item by modifying a shared AttackDefinition. A character action resolver derives a new snapshot from selected legal content.

## Example equipment cards

Names and effects below are candidates for separate tests, not content promised for the first mission.

| Candidate | New decision | Proposed benefit and drawback | Required validation |
| --- | --- | --- | --- |
| Standard practice blade | Stable comparison | Current authored reach/commitment | Remains a viable reference after adding alternatives |
| Long-hilt blade | Keep more distance before committing | Modest extra reach, slower preparation; matching visible blade | Actual range agrees with contact presentation; does not trivialize every enemy reach |
| Reinforced coat | Accept some pressure while positioning | More physical mitigation, a separately authored handling cost | Does not turn damage reduction into permanent stagger immunity |
| Light field gear | Avoid rather than absorb pressure | Less mitigation; a bounded handling advantage only if its tradeoff remains meaningful | Does not make dodge spam optimal or erase all commitment |
| Situational charm, much later | Prepare for a particular threat | One narrow effect with no direct peak-talent grant | Item trigger cannot recursively trigger itself or mint practice |

Do not tune weapon, armor, mastery and stamina simultaneously. Equipment experiments begin with mastery effects off, then test an explicit combination. If an item changes animation duration, gameplay phases and motion must agree; a tooltip cannot justify a hit before the visible swing.

## Loot that prompts decisions

The first reward should answer "Would this change how I approach the next encounter?" rather than "Is the green number larger?" Show changed behavior and costs in comparable terms. Avoid ten nearly identical drops and a salvage chore before a second weapon is worthwhile.

Candidate reward cadence: completing the initial relay contract offers a choice between two known authored items after the progression-loop comparison is already stable. The player can inspect both and defer equipping. Do not require an equipment pick to acquire the first talent; distinguish the two decisions in the debrief.

Later random drops should retain predictable routes toward a desired family. A discovery must not depend on an extremely rare item without an alternate coherent route. A historical connection involving an item can reveal optional content, but removing or replacing it should not erase the fact that the character experienced that connection.

No auction market, item durability tax, crafting profession network or randomized affix economy is needed for the first complete loop. Add an economy only when there are enough real decisions for its resources to support.

## Add one active ability before a hotbar

A candidate first ability is a deliberate **Set Footing** action for a disruption-oriented Warrior. Proposed test: spend a short exposed preparation to create a brief, single-use reduction in incoming poise impact during the next Heavy windup. No damage immunity, no HP reduction, no effect during dodge, no refund if the strike misses or is interrupted. The extra setup must cost a real opening; otherwise it becomes a mandatory pre-cast macro.

This is not a selected ability. Compare it with a simpler alternative: no new button, but an invested contextual Heavy behavior that already rewards a prepared position. If the button adds upkeep rather than a decision, discard it.

First active-ability design card must specify:

- Input action, remappable binding and focus behavior. Current number-row keys are technique debug selectors; do not silently turn them into gameplay hotbar slots.
- Eligible start phases, movement limits, cost, preparation, effect interval, recovery and cooldown.
- Source and target of the effect, exact consumption point, and reset/death behavior.
- What happens on interruption, failed activation, lost prerequisites or configuration changes.
- Visual anticipation, active indication, expiry and cancellation feedback.

Cooldown begins on successful acceptance, never a rejected press. A prepared opportunity is not a queued attack. No ability should silently grant unrestricted Heavy recovery cancellation. Any future cancellation window is a separate experiment with clear costs and animation support.

Add a second active ability only when it creates an independent situational choice. Avoid multiplying buttons to make an experienced character appear complex. A recognizable specialist can use familiar inputs differently.

## Stamina: reserved future experiment

**Current instruction remains: do not tune stamina.** High regeneration supports repeated-action and interruption investigation. These are designs for later comparisons, not proposed file edits now.

Maintain the vision's always-regenerating direction: accepted actions spend a known amount, and regeneration continues at a positive rate, increasing while less active. Walking and repositioning should feel useful rather than another form of expensive exertion. Define precisely which phases count as active; input held without an accepted action is not itself proof of exertion.

Useful future model:

```text
change over an interval = integrated regeneration - accepted action costs
```

For a repeating action with cost C and cycle duration T, C / T is its average expenditure rate. Compare that with active regeneration, then include realistic pauses, hits, interruptions and walking. A sustainable average does not prove good moment-to-moment decisions; a player can still be starved at the wrong time or ignore the meter entirely.

Test three questions separately:

1. Does uninterrupted attacking eventually require a meaningful pause?
2. Can deliberate walking/repositioning restore enough flexibility for a new decision?
3. Does an emergency dodge remain a choice rather than an always-available escape or a guaranteed impossible input?

Possible later parameters are maximum reserve, active regeneration, recovery regeneration, each action cost and optional build-specific modifiers. Change one major variable at a time. Do not reduce regeneration and increase every cost in the same trial.

Inspect failure incentives: wasting stamina should not force the player to stand idle beside an enemy; repeated harmless dodges should not be the best resource recovery strategy; a build should not stack cost reduction and regeneration until its intended drawback disappears. Establish ordering and a floor for effective costs when such modifiers actually exist.

No universal target such as "exactly three attacks then dodge" is selected. Different build rhythms can be valid if each preserves meaningful decisions. Infinite mastery must not eventually reduce every resource cost to zero.

## Poise, stagger and damage trades

Keep HP consequence distinct from action destabilization. A low-damage hit can matter because it interrupts a commitment; a heavy hit can matter without guaranteeing a stagger every time. The current applied receipts are a useful starting point.

Before armor or abilities alter poise, compare: threshold, accumulation recovery, stagger duration, post-stagger vulnerability and source attribution. An increase in poise threshold changes which trades are possible; it is not merely a defensive stat.

Do not make repeated stagger the only viable offense. A later elite may need a readable recovery/resistance phase, but that phase must not erase all value from a disruption specialty. Alternatives include a brief explicit resistance period with smaller but still useful disruption, or different break opportunities. These need independent tests and clear feedback.

The proposed Set Footing ability interacts directly with incoming poise and should therefore be tested only after ordinary trades are understood. Do not use a protective ability to hide an enemy whose attack cannot be read.

## Presentation is part of the mechanic

Every action needs anticipation, contact, consequence and recovery. Build presentation around the same committed timing used for gameplay. Input acceptance should produce immediate anticipation, even for a slow Heavy; responsiveness does not require removing commitment.

| Mechanic | Placeholder now / future requirement | Risk to test |
| --- | --- | --- |
| Light vs Heavy | Distinct simple sweep and impact; later distinct full-body anticipation and recovery | A fast visual on a slow mechanic feels disconnected |
| Planted vs advancing Heavy | Visible feet/torso weight and committed displacement | Advancement reads as a teleport or hidden range bonus |
| Guard contact vs HP opening | Different sound/pose/impact cue | A guarded hit looks like an unexplained damage failure |
| Poise break | Clear interruption and exposed posture | Small flashes fail to explain why a strike stopped |
| Dodge | Readable start, travel and recovery with collision | Animation implies invulnerability the mechanic does not provide |
| Discovery | Non-blocking clue and retained journal entry | Popup obscures another threat or falsely promises a purchase |

Hit-stop and camera shake are future optional presentation experiments. Define whether they alter simulation time, input sampling, action expiry and multi-actor timing before adding them. Start with bounded feedback that does not change authoritative timing; include intensity controls and avoid forced camera motion that breaks mouse look.

Do not solve missing animation coverage with a universal animation speed multiplier. Different phases may need separate authored poses and transitions. Gameplay determines when an attack is active; presentation must express that contract reliably.

## Content scaling rules

One new weapon family introduces more than a mesh: action profiles, facing/travel behavior, collision assumptions, animations, hit reactions, audio, equipment comparisons and discovery interactions. A magic family with hold/release basics also requires clear interruption and release behavior. It is a later experiment, not a drop-in label change on the melee action.

Defer elemental reaction matrices and extensive status effects. Begin any future status with one source, duration, application/expiry rule, stacking policy and presentation. Do not let a periodic effect generate unlimited mastery or retrigger on itself.

Keep a presentation coverage checklist per supported technique and body/weapon configuration. A technique with no readable anticipation is incomplete even if its data composes successfully. Expand families only after the first family supports a small complete loop and distinct characters.

All equipment, ability, economy and presentation additions above are proposals. No packages, assets, code, inputs or scenes changed for this document, and no Unity validation occurred.
