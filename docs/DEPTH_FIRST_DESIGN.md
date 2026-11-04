# Depth-first development of combat and character identity

Design refinement following the user's request to focus the proposed paths on depth. Documentation only; Unity is not to be run for this assignment. The established vision and talent charter remain unchanged. Specific mechanisms below are proposals requiring comparison and player feedback.

Start here before selecting more content from the broad [design atlas](GROWTH_DESIGN_ATLAS.md). [HEAVY_DISCIPLINE_DEPTH_STUDY.md](HEAVY_DISCIPLINE_DEPTH_STUDY.md) turns these principles into one worked investigation using existing Heavy techniques.

## What depth should mean in EvolvingRPG

A familiar action should remain useful while the player becomes more deliberate about its timing, starting position, facing, target state and consequences. Character development can give that action another controlled application. Encounter variation then asks the player to judge when that application is appropriate.

A deeper fighter does not need to press more buttons to demonstrate growth. They may approach the same guard differently, commit at a different moment, recognize an unsafe opening, or choose a different result after creating an advantage. The character's invested mechanics and the human player's understanding both matter, and must be evaluated separately.

Use a practical question: **what does this development make the player consider that they could previously ignore, and what do they still have to respect?** If its only answer is a larger number, a longer checklist or a shorter wait, it has not yet established tactical depth.

Depth includes ease of intentional expression. Removing unreliable input or misleading feedback can reveal existing decisions. It need not make execution more difficult. Preserve readable anticipation, useful fundamentals and the ability to investigate a mistake without frame-by-frame guesswork.

## Six properties to look for

| Property | Evidence of depth | Warning sign |
| --- | --- | --- |
| Context matters | The useful response changes with observable state | One evolved attack wins regardless of range, opening or nearby threats |
| Choice matters | Two responses have credible reasons in a tested situation | The "choice" is an automatic proc or mandatory setup routine |
| Consequences persist | Where an action ends and when control returns affect the next exchange | A hit always resets the encounter into the same safe position |
| Rules transfer | Understanding one exchange helps interpret another | Every enemy needs an unrelated secret exception |
| Identity persists | A specialty solves varied problems through its own strengths | Every weakness is patched until specialties converge |
| Information is usable | The player can see why a decision is relevant | Depth lives entirely in invisible thresholds and debug logs |

These are evaluation lenses, not a score to maximize. More interacting variables can make a fight less readable. A lower button count does not by itself prove depth either.

## Distinguish three sources of improvement

**Player understanding:** the human learns to bait commitment, read distance, preserve an escape route and recognize when a recovery window is insufficient. It belongs to the player and can transfer to a fresh character.

**Character capability:** an investment changes an actual action property or grants a defined application. It belongs to that character, has prerequisites/costs, and survives according to the character-save policy.

**World opportunity:** a different composition, route or threat creates another use for an existing capability. It belongs to authored encounters/world state. It should not be presented as a newly learned talent when no character mechanic changed.

All three can enrich the game. Report them honestly. A player getting better with unchanged data is not proof that mastery rewards work. A new enemy is not proof that the talent tree grew. A statistical improvement is not proof that the player gained another tactical choice.

## Use a development spine

For the next proof, keep one weapon family, Heavy as the focus, Light/Dodge/movement as supporting fundamentals, the two existing opponent roles and a small set of known spaces. Keep ordinary Heavy and the other current technique available as separate comparisons. Do not ask one test character to hot-swap all specializations during a fight.

Proposed spine:

| Layer | What grows | What remains recognizable | Evidence before the next layer |
| --- | --- | --- | --- |
| Foundation | Player understands reach, commitment and consequence | Existing Light, Heavy, dodge and camera controls | The player can explain a hit, miss or interruption |
| Discipline | One chosen Heavy profile favors a way of creating openings | Same Heavy input and visible committed strike | A credible benefit and liability appear against both opponent roles |
| Familiarity | One bounded refinement improves use of that profile | Its central commitment and limitations | The refinement matters without making judgment unnecessary |
| Application | One contextual development changes how an opening can be used | The selected discipline's mechanical identity | Sometimes take the opportunity, sometimes decline it |
| Integration | Fundamentals combine with that application naturally | No compulsory extra sequence or new meter | Walking, Light, dodge and withdrawal remain meaningful |
| Expression | Stance, travel and recovery communicate development | Timing and collision remain truthful | An observer can recognize the action and its exposure |
| Further depth | One demonstrated unresolved decision receives an authored extension | The discipline retains a reason to exist and a reason to be cautious | The extension improves a specific decision rather than fixing everything |

These layers are not mandatory rank gates. They describe an investigation order. The player need not complete a named checklist to deserve ordinary character growth.

## Specialize the problem the character is good at

Breaker-like development can deepen **creating a disruption opening through prepared commitment**. Momentum-like development can deepen **closing or returning into an opening through committed travel**. A future pressure discipline could deepen **choosing when to sustain contact and when to disengage**.

Do not equate these with finalized classes. The useful question is whether each discipline has a repeatable decision pattern. For early work, develop the Momentum question and keep Breaker as an independent reference. Pressure remains an alternative research direction, not another branch to ship simultaneously.

An advanced disruption fighter might become better at selecting the moment and shape of a break while still paying for planted preparation. An advanced advancing fighter might gain a more deliberate way to use travel while still needing a safe line and accepting miss exposure. Growth can modify a drawback, but a specialty should not gradually acquire every other specialty's strength.

## Preserve a meaningful boundary around the specialty

Write down the intended strength, unresolved decisions and residual limitations before an evolution is added.

| Discipline reference | Strength to deepen | Limitations that deserve continued respect | A dangerous convergence path |
| --- | --- | --- | --- |
| Planted disruption | Reliable creation of poise/guard openings through commitment | Preparation, limited travel, vulnerability to another threat | Adding advancing reach, free steering, universal stability and short recovery together |
| Committed advancement | Deliberate return to range or closing a gap | Travel line, collision, lower disruption, miss exposure | Auto-correcting distance, auto-aiming, canceling misses and matching Breaker's poise |
| Sustained pressure, later | Remaining engaged through deliberate short actions | Interruptions, target changes, when to stop | Automatic combo continuation, unlimited resource sustain and escalating universal damage |

These are proposed boundaries to test, not a rule that every specialty must always be bad at several things. A development that only adds penalties is not desirable depth. Seek a noticeable new capability with understandable remaining consequences.

A supporting hybrid can reduce one local difficulty without erasing the core problem. A disruption fighter might gain a controlled retreat after creating a break; that need not grant advancing preparation or instant Heavy cancellation. Compare the hybrid against spending the same commitment within the primary discipline. The opportunity cost must be real and visible.

## Deeper control has to preserve commitment

Control can be more meaningful than another damage multiplier: an authored travel choice, a bounded turn during preparation, or a deliberate recovery position. Each can also remove too much risk.

Evaluate an added control in three stages:

1. What uncertainty or expression problem does it address? A bad camera or unreliable input is a baseline defect; do not sell its fix as a talent.
2. Which part of the action becomes controllable, and when does that freedom end? Define the phase, bounds, intent source and visible cue.
3. Which consequences remain after the choice? Retain final facing/travel commitment, collision and an honest recovery policy.

For an early experiment, acceptance-time choices are easier to reason about than mid-swing branching. If later a discipline needs an intentional decision window, author that window explicitly. Immutable action parameters can describe a conditional policy; a late talent unlock still must not rewrite a running action. The current code does not supply a general branching-action framework, and none should be built for this design alone.

Do not introduce late correction, partial cancel, resource refund and follow-up protection in one "control mastery" node. Its apparent responsiveness could come from eliminating consequence rather than improving deliberate expression.

## Applications should create a decision after an advantage

The important part of a guard break or verified evasion is what the player can choose next. An automatic reward on success can be pleasant feedback, but it does not necessarily deepen that decision.

For a contextual application, write down:

- The advantage actually established by combat evidence.
- What existing choices remain available without the development.
- The new or changed choice supplied by the development.
- Why the player might decline it now.
- Its target, expiry, cost, failure policy and observable preparation.

Example: a verified evasion can leave time to walk, use Light, commit to Momentum or withdraw. A learned return application may make a particular Momentum approach possible. A second attacker, bad line, short window or unhelpful ending position can make declining it correct.

An opportunity timer must not turn this into a reflex test of always pressing RMB before the icon disappears. If people reliably decline a valid opportunity for understandable tactical reasons, that can be evidence the cost matters. It is not automatically evidence the reward is too weak.

## Handle the conditional movement problem explicitly

The first retaliation proposal makes a future Momentum travel farther under a condition. Once enabled, an ordinary-looking Heavy press could produce different displacement. That may be a usable learned relationship, or it may damage the player's sense of control. The current F2 practice toggle is a comparison tool, not a solution to per-action intent.

Compare one of these policies at a time:

| Policy | How intent is expressed | Tradeoff / concern |
| --- | --- | --- |
| Conditional form is armed | Clear linked opportunity cue; choosing Heavy accepts that known form | Simple inputs, but ordinary Heavy is unavailable during the window unless a separate rule exists |
| Existing directional intent opts in | A clearly defined forward movement intent at acceptance requests the longer form | Preserves an ordinary option, but held movement can opt in unintentionally; strafe/backward cases need design |
| Safe-location selected handling | A fixed selected form determines conditional travel behavior for this character | Predictable identity, but less moment-to-moment choice |

No policy is selected here. Avoid adding a new modifier key to hide the issue. Camera movement must not steer the action implicitly, and pressing into a wall must not grant more power. If the difference in travel is not readable and intentional, redesign the benefit before adding more trigger conditions.

The [existing C3 specification](CONTEXTUAL_DISCOVERY_EXPERIMENT.md) remains the candidate comparison. Its input-intent policy is now an explicit open gate, not a solved feature.

## Familiarity refines within the decision envelope

A bounded recovery improvement can make an existing application more reliable. It also moves timing thresholds. Preserve the authored miss penalty, test the full enemy response and avoid stacking a second recovery modifier before understanding the first.

Do not let every rank alternate among damage, poise, speed, range and efficiency until the discipline becomes universally superior. Use one clearly bounded refinement in the first track. Further numerical refinements need their own decision threshold review.

A mastery history can continue growing while a refinement approaches its safe bound. That is mathematically straightforward, but the marginal benefit may become imperceptible. If later ranks are only a number on a record, describe them as history; do not imply another meaningful combat transformation occurred.

## Discover further applications through a deeper relationship

Early discoveries can recognize a coherent relationship between known actions. Later discoveries should expose a more specific application of the invested discipline, rather than simply require a larger version of the same counter.

Candidate structure:

```text
Relevant character investment
 + verified relationship in combat
 + authored, repeatable context
 -> clue or knowledge
 -> deliberate choice, if eligible
 -> changed application of the same discipline
```

Repeated ordinary use still builds familiarity. Not every normal reward should demand a new contextual accomplishment. A highly specific optional discovery can be demanding without making the base discipline unusable until it occurs.

Examples of deeper questions worth investigating: Did advancement help recover range without losing the next safe position? Did a disruption opening create a choice between pressure and escape? Did a short action preserve enough time to commit later? The current evidence cannot prove all of these. Keep unsupported intention/causality claims in player feedback, or author the additional evidence only when a selected rule needs it.

Do not award a hidden "tactical restraint" talent merely because the player did not press a button. They may have been reading, distracted or out of range. A journal can describe a verified encounter outcome without claiming to know the player's intent.

## Repetition should reveal relationships

Use a stable opponent and space long enough to understand the discipline. Then alter one relevant context: starting distance, opponent recovery, a safe travel line, or another visible threat. The action can remain unchanged while its value changes.

Only after the player understands that relationship should character evolution add another application. Otherwise a new node may be compensating for a context the player never learned to read.

Repeated missions can deepen familiarity with a place. Different route, objective priority or position at engagement may change the same action's usefulness. More enemies, tighter timers and larger health pools are not required to establish that effect.

Preserve a familiar comparison contract when the region develops. A changing world should expand the questions asked of a specialty, not permanently remove every situation in which it feels strong.

## Extend the same decision through the session loop

Depth can connect preparation, an exchange and the return to the hub without adding a new subsystem at each stage.

**Before deployment:** inspect what a contract actually asks and choose a legal handling form. This is a character decision with a clear opportunity cost. Do not require a completely different build for every mission or reveal the objectively best selection as a mandatory checklist.

**Before engagement:** choose a line and distance that suit the selected commitment. A route can make this easier or harder using familiar geometry. No special talent gate is needed for every useful route.

**During the exchange:** decide when to spend an opening and where to finish. A successful contact can still leave a poor next position. A short action can serve a Heavy-focused plan without becoming an unrelated build branch.

**Across the mission:** later, when mission health/reward policies exist, the cost of a trade may matter beyond that single opponent. This is not currently demonstrated by resettable arenas. Do not assume full health reset or persistent attrition without the assigned mission policy.

**At debrief:** reflect on a recognizable limitation or application. Offer a choice to deepen the method, explore a supported adjacent development, or keep the current form. The report should not prescribe a build based on action frequency.

**On a return visit:** the player can apply better understanding to familiar content, or see an announced authored context change. The same region can support another question without becoming a new biome.

A familiar encounter becoming easier through understanding can be a satisfying result. It does not need an immediate hidden counter to restore difficulty. Preserve some situations where the character's development feels effective, and offer further questions deliberately instead of making every success temporary.

## Keep the information load smaller than the decision space

The player can reason about distance, facing, recovery and a nearby threat without four extra meters if the bodies, poses and space communicate them. Add the minimum contextual cue needed for an earned application: whether it is armed, which exchange it concerns, and when it expires, in a form that does not cover the next threat.

Prefer one coherent armed-state indication to many independent temporary-bonus icons. Repeated rejection messages should not compete with anticipation cues. Let details remain inspectable after combat.

A deeper application should reduce ambiguity about the character's chosen action. It should not require watching a proc icon every frame to avoid unintended displacement. Input remapping, sensitivity and reliable controls are baseline usability; they are not earned mastery rewards.

## Equipment and active abilities must earn a narrow role

For the near-term depth study, keep the weapon, armor and resource model fixed. The existing Heavy profiles already vary preparation, poise, travel and miss exposure. Adding weapon reach and armor stability simultaneously would obscure which relationship the player is learning.

Later gear can emphasize one handling decision within the discipline. It must not supply all the countervailing advantages through independent slots. Review the combined action, not just whether each individual item sounds modest.

An active ability should create a situational decision that current fundamentals and selected applications cannot express adequately. Compare a no-new-button version before adding a cooldown tool. If an ability becomes a compulsory preparation step for every Heavy, it increases upkeep more than depth.

The long-term equipment and ability documents remain useful option inventories. They are not a parallel implementation checklist.

## Growth inside a lineage does not require widening the whole build

Separate the depth of a selected discipline from the number of disciplines the character can develop to a peak. Continued familiarity should not mint unlimited major-focus capacity. A veteran can keep discovering applications of an advancing commitment without automatically acquiring a disruption specialist's defining strengths.

Candidate arrangement to compare later:

1. **Foundational access:** every Warrior retains useful ordinary attacks and movement. No focus decision removes basic viability.
2. **Deep development eligibility:** deliberate character investment establishes which discipline can support its advanced applications. This is a meaningful learned commitment, not merely an equipped slot.
3. **Within-lineage repertoire:** a small authored set of applications becomes known through related experience. Discovery does not automatically purchase them.
4. **Current action form:** the selected legal form determines what a Heavy does. Alternative handling forms replace one another where they conflict; they do not stack silently.
5. **Contextual opportunity:** the current form can offer one bounded follow-up relationship when its conditions are met. It does not activate every known application in succession.

This arrangement is a hypothesis, not a new five-system framework or final slot policy. It distinguishes what a character may become from what a single action currently expresses. Implement only the layers used by the small slice.

The four-unit commitment fixture in the broader documents is a useful arithmetic demonstration, but does not resolve the long-term question. It must constrain learned deep commitments if that is the purpose; turning a peak off cannot simply free the capacity to learn every other peak. Likewise, owning all peaks and swapping them instantly at a hub would still undermine the charter's identity requirement.

Major-focus changes remain an explicit open decision. Compare the opportunity to explore with the consequence of rewriting a mature character. A long grind fee is not automatically meaningful depth; an irreversible early choice is not automatically meaningful agency. Any retraining, refund or requalification proposal needs a truthful preview, retained knowledge policy and player review before becoming a character rule.

Do not erase a legitimate history just because a focus changes. The character can remember an experience while no longer having the investment needed to perform its associated peak application. History, ownership and active eligibility must be distinguishable in saves and UI.

## Every new node should earn a specific role

Before authoring another development, classify its purpose:

| Purpose | Appropriate result | Common inflation to avoid |
| --- | --- | --- |
| Refinement | A tested bounded improvement within the existing application | Many compulsory percentage tolls before the technique feels complete |
| Alternative handling | Another way to perform the same discipline with a real tradeoff | A universally better successor that makes earlier forms obsolete |
| Contextual application | A new use of an established advantage | A passive proc attached to every possible combat state |
| Integration | A deliberate relationship with a supporting fundamental | A mandatory multi-button rotation or a universal weakness patch |
| Expression | More truthful and distinctive stance/motion/feedback | Charging progression for basic readability or presenting cosmetics as new combat power |

If the purpose is unclear, keep the idea in the workshop. If two nodes do the same job, merge them before writing separate discovery rules. If a node only exists to delay the interesting choice, compare removing it. A smaller tree can preserve a long character history without requiring a hundred shallow purchases.

Do not make every worthwhile development the child of every preceding one. A player can deepen a chosen application without buying unrelated handling preferences. Use prerequisites to express a genuine dependency, not force completion of the whole discipline menu.

## A mature build can remain recognizable

The core philosophy does not require the game to forcibly replace a player's favored technique every few missions. A player can remain a planted disruption fighter and discover further ways to prepare, exploit or reposition around a break. A useful old form can remain selected even after another application becomes known.

The near future should be inspectable: what this development changes and what it might connect to. The distant future can remain partly unknown. Do not require an early player to plan the full mature tree, or reveal two hundred silhouettes as a promise that every one will eventually matter.

Proposed mature-character experience:

- Established fundamentals and a recognizable primary discipline.
- A few owned, consequential developments whose costs can be explained.
- One or two meaningful adjacent relationships, rather than every discipline's peak.
- Ongoing familiarity/history with honest numerical bounds.
- New applications encountered gradually through authored situations and deliberate investment.

The quantities are descriptive hypotheses, not fixed slot counts. The important result is that additional history deepens an identity instead of dissolving it into a universal character.

When the player declines a newly revealed possibility, retain the value of their existing choices. Do not quietly make the next common enemy immune to the old form to force the purchase. Difficulty may still ask for better judgment, coherent equipment and preparation; it should not demand a hidden talent as an undisclosed admission ticket.

## Show the depth in the character's body

Planted weight, measured return travel and overextended recovery can explain a discipline better than a list of passives. Presentation should communicate what the character committed to and when another choice becomes available.

A later style change can express the same rule more richly without changing every action's statistical values. Conversely, if a new pose implies new control, timing or protection, either implement and test that meaning or change the pose.

Reusing an animation set is a production advantage only if the resulting contact remains truthful. A larger talent count does not justify less reliable timing cues.

## Avoid mistaking these for depth

| Pattern | Why it can look deep | What would make it genuinely useful |
| --- | --- | --- |
| Long prerequisite chain | Many visible steps | Each retained choice creates a recognizable application, not a toll |
| Rare hidden threshold | Hard to discover | Coherent clues and repeatable context, with a meaningful resulting decision |
| Tight input sequence | Demands execution | A timing/positioning choice with legible tradeoffs; no mandatory complexity tax |
| Many small passives | Lots of build combinations | A few interactions whose combined consequence is understandable |
| Situational bonus on every state | Constant micro-optimization | A small number of conditions that matter and can be read |
| Enemy counter to each specialty | Forces change | Counterplay within a specialty plus viable fundamentals |
| Endless rank count | Progress never stops | Honest bounds and a sustainable source of new authored applications |

An empty prerequisite can be removed. Two passives with the same effect can be merged. A conditional bonus nobody deliberately plays around can remain a numerical refinement or be discarded, rather than becoming another named branch.

## A focused content policy for the next proof

Near-term candidate: develop one Momentum lineage through one contextual application, with ordinary Heavy and planted Breaker as comparisons. Retain the guarded and fast opponent. Keep Light, jump, dodge, strafe and walking unchanged. Use existing spaces before adding enemies, equipment or regional rules.

The first proof should answer whether the discipline supports useful changes of judgment across several exchanges. Then connect its accepted parts to deliberate investment and persistence. Mission integration supplies a reason to repeat and reflect; it does not require another talent family.

Hold mobile Light, pressure, steering, recovery retreat, active abilities, wider sweeps, magic and item-triggered effects as dormant candidates. A dormant candidate needs a demonstrated missing decision before promotion. The [worked study](HEAVY_DISCIPLINE_DEPTH_STUDY.md) specifies the proof and promotion conditions.

This narrowing is a research priority proposal. It preserves the long-term charter, the two-choice first progression comparison, current implemented scenes and the user's ability to revise the direction after playtesting.

## The long-term limit remains honest

Depth reduces the amount of distinct content needed to create worthwhile decisions, but it does not make meaningful growth free or literally infinite. A finite authored system can eventually become familiar, solved or preferred in a stable form.

Continue in a discipline by introducing a new application only when existing mechanics leave an interesting unresolved question. Sometimes the best next addition is one encounter or clearer presentation. Sometimes a selected control extension is justified. Sometimes the current prototype has reached its authored depth and needs content work rather than another progression formula.

Indefinite mastery remains an extensibility goal. Its real cost is authoring, testing, readable expression, maintenance and a reason to choose. Prove that another meaningful development can be added without invalidating the old ones; do not claim that this proves infinite mechanical novelty.

## Validation and scope

All reasoning here is a proposal or a paper-design deduction. No new action, branch, input policy or specialization rule is implemented. No Editor setup is required. No Unity launch, compilation, gameplay tests or feel validation occurred. Runtime assets, controls, project settings and stamina remain unchanged.
