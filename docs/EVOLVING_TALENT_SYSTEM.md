# EvolvingRPG — Living Talent Tree & Character Progression

**Document type:** Long-term game-design goal and implementation charter  
**Status:** Design direction; not an instruction to implement the full system immediately  
**Last updated:** 2026-11-03  
**Suggested repository location:** `docs/EVOLVING_TALENT_SYSTEM.md`  
**Related documents:** `docs/VISION.md`, `docs/MILESTONES.md`, `AGENTS.md`  
**Engine:** Use the version actually declared by `ProjectSettings/ProjectVersion.txt` (currently `6000.6.7f1`). Do not change the Unity version because older planning documents specify a different release.

> **Core vision:** The game should not be about reaching the strongest version of a character. It should be about discovering what that character can become in a world that changes alongside them.

## 1. Why this system exists

The living talent tree is a defining feature of EvolvingRPG, not a conventional skill-tree menu added to a completed combat game.

A character begins with understandable, shared fundamentals, such as the Warrior's Light Attack, Heavy Attack, movement, and Dodge. Through **how the player fights**, **which abilities they practice**, **what they encounter**, and **where they deliberately invest**, that character develops a recognizable discipline and eventually a unique fighting identity.

**The destination should be visible in the gameplay, not merely in character-sheet numbers.** Two experienced Warriors should move, attack, recover, take risks, and exploit openings differently enough that an observer can recognize their specializations without reading a build screen.

The game should support deep, sustained growth in a chosen discipline without inevitably turning a veteran into a jack-of-all-trades character.

### Player promises

1. **Informed choices:** The player can understand currently available paths and meaningfully plan near-term investments.
2. **Surprising discoveries:** The player cannot see every possible future connection at character creation.
3. **Cause and effect:** Discoveries arise from coherent rules involving experience and prior decisions, not inexplicable random rewards.
4. **Ownership:** Experience may reveal opportunities, but it does not forcibly select talents or redefine a build.
5. **Distinct identities:** Long-term investment in a discipline remains valuable; everyone should not converge toward the same comprehensive build.
6. **Replay value:** What the player learned on one character can inform a new character without transferring that first character's mastery or power.
7. **Playability first:** Growth must enrich moment-to-moment decisions, combat rhythm, and visual character expression—not only damage multipliers.

## 2. Design decisions vs. questions still open

The following are **established design intentions** from our discussions, not necessarily implemented features:

| Established direction | Meaning |
| --- | --- |
| Partially revealed tree | Current and near-future paths are readable; deeper branches and interactions can emerge later. |
| Shared Warrior starting foundation | Players can start with familiar third-person action controls before developing specialties. |
| Experience + choices | Meaningful usage builds familiarity; player decisions establish specialization; both help reveal future options. |
| Depth over universal breadth | A deeply specialized character should not also acquire every other discipline's strongest capabilities. |
| Account-level knowledge, character-level ability | Previous discoveries inform future playthroughs but do not grant new characters talents, power, or mastery. |
| Evolving techniques and fighting styles | High-level growth can affect animations, stance, movement, timing, attack patterns, and interactions—not merely stats. |
| Meaningful ongoing growth | A chosen discipline should keep offering progress and discoveries without runaway numerical power inflation. |

The following are **proposals to prototype**, not locked decisions:

- Exactly how talent points are earned and how frequently.
- Specialization limits: major-focus capacity, deep-branch prerequisites, competing investments, or a combination.
- Whether and at what cost major evolutions can be respecialized.
- Exact mastery formulas, diminishing returns, and long-term scaling.
- How many learned evolutions may affect an action simultaneously.
- Whether named combat styles are explicitly equipped, inferred from a character's build, or both.
- How much hidden discovery progress should be shown.
- When to use authored interactions versus constrained/generated combinations.

**Do not silently turn proposals into permanent design rules.** If a decision affects character identity, replayability, or progression irreversibility, surface the tradeoff for design review.

## 3. The basic progression loop

```text
Play the character
       |
       v
Use techniques meaningfully in real encounters
       |
       +----> Gain proficiency/mastery in practiced techniques
       |
       +----> Record relevant experiences and interactions
       |                 |
       |                 v
       |           Reveal a hint or new talent possibility
       |                 |
       v                 v
Earn progression resources / talent choices
       |
       v
Player deliberately invests in selected developments
       |
       v
Actions, combat style, and available tactics change
       |
       v
New situations and cross-discipline interactions become possible
       |
       +------------------------> Continue discovering and specializing
```

The loop must contain actual **player decisions**. Never automatically purchase a newly discovered evolution because an event counter reached a threshold.

### Three progression systems with different jobs

**Mastery — getting better at what one already does.** Practice in meaningful contexts gradually improves a technique's proficiency, effectiveness, control, or efficiency. Mastery may continue growing deeply over time, with careful scaling rather than unrestricted exponential damage growth.

**Evolution — changing what an action can do.** Player-selected developments alter execution, mobility, posture, risk, target interactions, situational effects, or available follow-ups. An evolution should create a new tactical decision, not just a larger number.

**Discovery — learning what might become possible.** Choices, relevant experiences, encounters, and acquired techniques expose additional branches or connections. Discovery changes the **set of known options**, not the currently invested build.

**Fighting style — the visible expression of accumulated specialization.** It is an outcome of mastery and evolution, not a replacement for those systems. A style may eventually alter stance, movement, attack animation sets, timing, poise interactions, and combo opportunities. Whether styles are explicit selectable configurations is still undecided.

## 4. Start with the Warrior, then let identities emerge

The initial Warrior is our baseline melee action-RPG character: camera-relative movement, strafing, jumping, directional dodging, Light Attack, and Heavy Attack. New Warriors begin with broadly comparable controls and opportunity, rather than selecting a complex endgame subclass immediately.

Their development should diverge through behavior and choice.

| Possible identity | Plays around | Changes the character | Gives something up |
| --- | --- | --- | --- |
| **Breaker** | Committed Heavy Attacks; guard and poise disruption | Weightier strikes, distinct stagger follow-ups, planted stance | Mobility, recovery flexibility, or vulnerability when attacks miss |
| **Duelist / Flowing Fighter** | Evasion, spacing, precise retaliation | Reactive counters, directional attacks, movement-linked strikes | Some direct stagger output or resilience |
| **Relentless Fighter** | Sustained offensive pressure, exploiting short windows | Momentum-based follow-ups and attack transitions | Stability if interrupted or forced to disengage |

These names are **illustrative emergent identities**, not mandatory subclasses or finalized trees.

A player can explore adjacent techniques, including meaningful hybrids, without obtaining the complete high-end power of every specialty. For example, a Breaker may learn a situational evasive counter while remaining fundamentally a Breaker.

### A crucial distinction: skill usage should be qualitative

Heavy Attack usage alone should not make every Heavy Attack user progress identically.

- Frequently **breaking guards** may reveal disruption techniques.
- Often **landing a Heavy Attack after a well-timed dodge** may reveal counterattack interactions.
- Frequently **interrupting a dangerous enemy windup** may reveal interception techniques.
- Consistent, ordinary successful use still builds baseline Heavy Attack familiarity.

This should be based on meaningful combat outcomes, not an optimal strategy of pressing one button against an inert dummy thousands of times.

## 5. A partially revealed living talent map

The player sees enough to form plans, but not every long-term destination.

### Visibility and ownership are separate dimensions

**Visibility / knowledge:**

| Visibility state | Player experience |
| --- | --- |
| **Undiscovered** | The node or connection is absent from the map. The player has no false promise of a specific reward. |
| **Foreshadowed** | A silhouette, thematic clue, or incomplete connection suggests a possible direction. Exact effects may remain unknown. |
| **Revealed / known** | The name, effect, and understandable prerequisites become available for inspection. |

**Acquisition / use, for a revealed talent:**

| Acquisition state | Player experience |
| --- | --- |
| **Locked** | Known, but prerequisites not yet met; indicate achievable steps. |
| **Available** | Eligible for investment; show cost and tradeoffs. |
| **Learned** | Invested and owned by this character. |
| **Active / configured** | Currently affecting the relevant action or fighting style, if the final system adopts limited active configurations. |

These are not one flat sequence: an already revealed talent might be locked, available, learned, or active.

**Interface rule:** Never disguise a locked talent as undiscovered, or imply a learned talent is active if it is not. Show meaningful explanations in ordinary language.

### Player-facing information

For a selectable talent, present:

- What gameplay behavior changes, preferably with a simple demonstration/preview when practical.
- Activation conditions and expected limitations.
- Investment cost and prerequisites.
- Tradeoffs (exposure, recovery, range, stamina, commitment, incompatibilities).
- A clear distinction between mastery improvements and behavior-changing evolutions.

For a foreshadowed talent, present an honest directional clue, not fake certainty about an undiscovered reward.

## 6. Discovery must feel earned, not arbitrary

A proposed discovery evaluator can consider four sources:

1. **Current character build:** learned talents, chosen evolutions, active techniques.
2. **Demonstrated practice:** validated usage patterns and successful techniques.
3. **Combat context:** enemy states, terrain or encounter conditions, defensive reactions, stagger interactions.
4. **Character history:** durable records of noteworthy experiences or combinations, including events that may become relevant much later.

Example: **Retaliatory Momentum** could be revealed after the character develops an advancing Heavy Attack, practices evasive movement, and meaningfully uses both together. Upon discovery, it becomes a *choice*, not a free passive bonus. Its effect could enhance the next advancing Heavy Attack after a successful evasive dodge, with increased recovery if the enhanced strike misses.

This is **an illustrative discovery rule**, not the finalized combat formula.

### Discovery quality rules

- Prefer contextual accomplishments to raw action-count grinding.
- Support clear feedback such as "Your evasive technique is beginning to influence your advancing strikes."
- After discovery, explain generally what unlocked it; the exact internal thresholds need not all be displayed.
- Avoid irreversible hidden traps: missing an early encounter should not permanently invalidate a future character fantasy.
- Do not require obscure out-of-game knowledge to make core progression viable.
- Allow uncommon combinations to surprise players, but never use completely unbounded random effect generation.
- Protect combat mastery: a clever new enemy or countermeasure must not endlessly punish a player's successful specialization.

## 7. Specialization without jack-of-all-trades builds

**Central requirement:** A character can acquire broad familiarity and knowledge, but *deep specialized power* must involve consequential choices and opportunity costs.

A recommended direction to test is:

- **Broad fundamentals:** Core attacks, movement, basic proficiency, and some supporting techniques remain accessible.
- **Focused development:** Important evolutions require investment, prior technique proficiency, and/or specialization prerequisites.
- **Deep commitment:** Advanced style-defining evolutions compete for limited developmental commitment or create substantial tradeoffs, so one character cannot reach every discipline's peak.
- **Meaningful hybrids:** A limited number of cross-discipline interactions can create distinctive combinations without providing unrestricted access to all top-tier outcomes.

**Do not assume** that simply limiting hotbar slots solves this problem. If every character can learn every major specialization and switch between complete peak builds effortlessly, character identity and replay value will erode.

Equally, avoid punishing experimentation too early. The player needs reasonable ways to explore unfamiliar directions. The exact respec policy remains a design question; minor experimentation and major identity changes may warrant different treatment.

### Depth rather than infinite breadth

Long-term growth should deepen a chosen discipline through new technique applications, refined mastery, contextual interactions, and occasional new style behaviors.

It must **not** imply infinite unique animations, infinite fully authored talents, or unbounded percentage growth. Our practical goal is an extensible system whose additions remain meaningful, testable, and balanced.

## 8. Make existing actions evolve physically

The most important differentiator is that the talent system can transform a familiar input into a richer action over time.

**Example — Heavy Attack:**

| Stage | What changes |
| --- | --- |
| Baseline | A single committed strike with basic recovery and stagger. |
| Practiced | Better control over its original commitment and recovery profile. |
| Specialized | Choose a driving strike, disruption strike, or precise counter technique. |
| Deep style | Different stance, startup/recovery relationships, directional transitions, and situational follow-ups. |
| Advanced interaction | Interacts with mastered Dodge, defensive stance, or another invested discipline. |

As a principle, Light Attack remains recognizable as a light attack and Heavy Attack remains recognizable as a heavy attack, even when their advanced applications become elaborate. Do not add mandatory key combinations merely to make endgame combat seem sophisticated.

**A good evolved attack changes at least one player decision:** when to commit, how to position, what opening to seek, which target state to exploit, or what drawback to accept.

Where possible, prefer reusable animation sets, blend trees, overrides, procedural pose adjustments, and authored transitions. Gameplay modularity does **not** automatically generate polished animations; animation coverage and readability must be separately planned and playtested.

## 9. Distinguish character progress from player knowledge

The game should support an **account-level Discovery Archive** as a long-term concept.

- The archive remembers known evolutions, meaningful discoveries, and general clues from any character.
- A new character can view this *information* and deliberately pursue a known direction.
- The new character does **not** inherit prior mastery values, learned talents, specialization points, or completed character-specific prerequisites.
- The archive must not automatically reveal every undiscovered possibility in the game.
- Players who want to explore a different branch should have a reason to create another Warrior.

If a player already knows of Retaliatory Momentum from Character A, Character B can plan toward it, but must still develop a relevant build and demonstrate the necessary capability before selecting it.

**Potential UI distinction:** "Known to you" vs. "Developed by this character".

## 10. Talent-tree user experience

The ultimate interface should feel like examining a character's emerging combat identity, not scrolling a huge flat list.

### A. Living Talent Map

- Organize around foundational actions/disciplines, with recognizable clusters.
- Reveal immediate paths clearly and deeper possibilities gradually.
- Support zoom, pan, focus-on-action, and sensible grouping/filtering as the network grows.
- Show invested nodes, known-but-unavailable nodes, foreshadowed connections, and recent discoveries distinctly.
- Let the player inspect what an evolution does to real gameplay and what has led toward it.
- Prevent visual growth from becoming unreadable clutter.

### B. Discovery Journal

- Record important hints, discovered paths, and explanatory context.
- Answer: "Why did this appear?" without requiring a spreadsheet of hidden counters.
- Preserve past discoveries and unusual historical connections.
- Keep optional exploration clues available for players deliberately pursuing a known fantasy.

### C. Current Combat Identity / Build View

- Show the player's primary investments, fighting-style tendencies, learned evolutions, and which are currently shaping actions.
- Show tradeoffs and conflicts when choosing alternatives.
- If active slots or major-focus limits are adopted, communicate them *before* investment.
- Distinguish a permanent discovery from an active tactical configuration.

The UI should make an experienced specialist visually legible while remaining understandable to a new player. It should not depend on continuous observation of several hidden meters during combat.

## 11. Technical architecture direction for Codex

**This section expresses extensibility goals, not a request to build a framework immediately.** Start with a handful of authored definitions and refactor only when repeated use cases justify it.

A future talent definition may describe:

- **Identity:** stable ID, display name, tags, intended discipline, version.
- **Knowledge:** visibility/reveal conditions and discovery explanations.
- **Investment:** prerequisites, costs, depth/commitment requirements, compatibility constraints.
- **Behavior:** triggers, contextual conditions, effects/modifiers, explicit tradeoffs.
- **Presentation:** icons, descriptions, VFX/animation references, preview hints.
- **Persistence:** whether data belongs to a character, an account archive, or a transient encounter.

Illustrative structure (not a required runtime schema):

```text
Talent: RetaliatoryMomentum
Parents: MomentumHeavyAttack, EvasivePractice
Reveal: demonstrated dodge-to-heavy interaction in meaningful combat
Unlock: prerequisite investments and sufficient specialization commitment
Trigger: after_successful_evasive_dodge -> next_heavy_attack
Effect: modify_forward_motion, modify_stagger_impact
Tradeoff: increased_recovery_on_miss
Knowledge scope: account archive after first discovery
Owned/active scope: individual character only
```

Separate these responsibilities conceptually:

1. **Combat events:** authoritative records of successful actions, enemy states, and outcomes.
2. **Mastery tracking:** progression for known techniques; anti-farming safeguards.
3. **Discovery evaluation:** deterministic, inspectable rules that reveal possibilities.
4. **Talent ownership and specialization:** investment, prerequisites, limits, and save data.
5. **Runtime action composition:** safely transforms the performed action from valid learned/active modifiers.
6. **Presentation:** maps, journal, indicators, and explanations.

Important engineering constraints:

- Maintain stable IDs and version-tolerant save data so later content edits do not silently corrupt characters.
- Provide developer diagnostics explaining *why* a node revealed or failed to reveal; player-facing messages can remain more thematic.
- Prefer deterministic behavior over unexplained RNG for talent prerequisites.
- Treat incompatible or recursive trigger/effect combinations explicitly; prevent unlimited feedback loops.
- Test changes to existing combat behavior as well as new talents.
- Keep user-created game code under `Assets/Game/`; do not rewrite imported Starter Assets unnecessarily.
- Preserve the existing input scheme and current Unity project version unless the assigned milestone explicitly requests changes.
- Defer procedural generation, elaborate account services, networking, and large UI frameworks until the core loop is proven.

## 12. Balance and evaluation standards

A talent is valuable if it changes how a player approaches an encounter, not because its tooltip is complex.

Questions for every significant evolution:

- Does it create a distinct tactical opportunity or constraint?
- Is the behavior recognizable during actual gameplay?
- Is there a credible reason not to take it on every build?
- Does it preserve counterplay, positioning, recovery, and risk?
- Is it understandable without reverse-engineering opaque thresholds?
- Can an alternative specialty excel in a different encounter or playstyle?
- Can we add more techniques to this discipline without trivializing existing ones?

**Failure modes to prevent:** mandatory action spam, hidden irreversible lockouts, linear numerical arms races, one optimal destination, eventually universal characters, unreadable tree sprawl, misleading discovery hints, effects that stack without limit, and animation changes that compromise hit clarity.

## 13. First playable validation: a tiny vertical slice

Do **not** build the full living tree when combat foundations are still being tested. The first progression prototype should demonstrate the complete loop with deliberately small content.

### Suggested scope

- **One class:** Warrior.
- **Core actions:** existing Light Attack, Heavy Attack, and Dodge.
- **One practice/mastery track:** Heavy Attack, with baseline proficiency gained through meaningful use.
- **Two visible development choices:** e.g., Breaker and Momentum Heavy Attack variants, each affecting the *feel* and tactics of the strike.
- **One discovery:** a dodge-and-Heavy interaction revealed after relevant play and investment.
- **One small talent-map UI:** learned, available, locked, and foreshadowed states.
- **One discovery notification/journal entry:** clear evidence that an action/choice created a new possibility.
- **One simple persistence path:** save and reload character mastery, investments, and discovery states.
- **One useful combat test opponent:** a defensible or telegraphing enemy, since an inert dummy cannot validate contextual skill usage well.

The two visible developments should be *alternative tactical decisions*, not merely +5% vs. +10% damage.

### Test scenario

1. Create a baseline Warrior and encounter a test enemy.
2. Apply Heavy Attack successfully and observe practice progress.
3. Earn a small, controlled test investment resource; select either Breaker or Momentum.
4. Confirm the selected evolution visibly changes the action and produces a different tactical outcome.
5. Use Dodge and Heavy Attack together in a meaningful combat context; receive a clue or reveal a new interaction.
6. Confirm the new option is discoverable but **not automatically purchased**.
7. Invest if eligible, see its effect, and save/reload without losing its state.
8. Compare a second fresh Warrior following a different investment path. They should differ in play, not only stats.

### Prototype acceptance criteria

- [ ] A basic action's mastery visibly progresses through valid play, not idle clicking alone.
- [ ] Talent investment is a conscious player choice and respects prerequisites.
- [ ] At least two choices change attack behavior enough to warrant different tactics.
- [ ] One initially unseen or hinted connection becomes revealed through a reproducible action/history rule.
- [ ] The UI distinguishes reveal, eligibility, ownership, and activation correctly.
- [ ] Mastery, investments, and discovery flags survive save/reload.
- [ ] A new character does not inherit the old character's power or talent purchases.
- [ ] Developer diagnostics can explain why a discovery did or did not trigger.
- [ ] Existing movement, camera, dodge, and combat fundamentals still work.
- [ ] The player playtests the feature; code compilation alone is not proof of design success.

**Account-wide archive:** Validate the distinction conceptually in this vertical slice if convenient, but a full multi-character account system is **not** required to pass the first prototype.

## 14. Development sequence and handoff guidance

Use `docs/MILESTONES.md` as the source of truth for the *currently assigned* implementation milestone. This document describes the destination, not a license to skip movement/combat validation.

Recommended staged development after the combat foundation is stable:

1. **Technique variation experiment:** test baseline Heavy Attack against two behaviorally different variants.
2. **Mastery experiment:** prove meaningful-use tracking without trivial spam farming.
3. **Discovery experiment:** reproduce one revealed cross-technique interaction from known prerequisites and combat history.
4. **Choice + save experiment:** select, persist, compare, and inspect the resulting builds.
5. **Specialization limits experiment:** verify that meaningful hybridization is possible but peak branches cannot all be acquired.
6. **Style expression experiment:** add a distinct stance, animation/recovery profile, or combat rhythm appropriate to one developed identity.
7. **UI and archive growth:** expand navigation, history, and eventual information-only account knowledge.
8. **Scale cautiously:** grow curated techniques and validated compositional rules only after the small loop is fun.

### What Codex should do when assigned the first progression task

- Read `docs/VISION.md`, `docs/MILESTONES.md`, this document, and inspect the *actual* combat implementation.
- Propose the smallest implementation consistent with the currently tested combat mechanics.
- Identify which mechanisms already exist and which are needed for the prototype.
- Clearly separate established design intentions from suggestions that require player feedback.
- Implement only the assigned slice, preserving current input feel and third-party packages.
- Provide explicit Unity Editor instructions, diagnostic output, file-change summary, and a manual playtest checklist.
- Never claim gameplay feel is validated without the player's actual testing.

**Suggested future Codex assignment (only after combat is sufficiently testable):**

> Create a minimal EvolvingRPG progression vertical slice using `docs/EVOLVING_TALENT_SYSTEM.md` as the design target. Implement one Heavy Attack mastery track, two behavior-changing talent choices, one deterministic, context-driven discovery, and minimal debug UI/persistence. Preserve existing movement and combat. Document exact tests and leave the full procedural talent engine, account archive, and large talent map for later milestones.

## 15. Questions for continued design discussions

These should remain visible and unresolved until prototype evidence or explicit design decisions settle them:

1. What prevents a veteran from mastering *every* discipline: limited major specializations, depth prerequisites, active commitments, permanent choices, or a hybrid?
2. How do we permit experimentation without making character identity disposable through instant full-build swapping?
3. What constitutes valid/meaningful technique use for each discipline, including solo play and repeated missions?
4. What progress hints are useful without reducing every discovery to an exact threshold checklist?
5. Which discoveries are truly new techniques, and which are new modifiers of an existing technique?
6. How do evolved animations/stances communicate changed mechanical commitment and hit timing?
7. How can very deep mastery remain rewarding without damage numbers invalidating the encounter design?
8. What progression information should the account archive display to subsequent characters?
9. How are equipment, enemy adaptations, and future magic disciplines allowed to influence discovery?
10. Which compatibility rules permit exciting hybrids without creating uncontrollable talent interactions?

---

## The design goal in one sentence

**Give players a shared, understandable combat beginning; let meaningful ability use and deliberate investment produce distinct, deeply specialized fighting identities; reveal new possibilities through the character's history; and preserve the joy of discovering what a character can still become.**
