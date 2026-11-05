# Development milestones

**Approach:** Validate one mechanic at a time. Each milestone should produce something playable. The vision document is the long-term compass, not the first milestone's scope.

**Long-term progression requirements:** `EVOLVING_TALENT_SYSTEM.md` defines the established talent-system direction and labels mechanics that remain proposals. `PROGRESSION_DEVELOPMENT_PATH.md` maps that charter to small validation experiments. The continued assignment implemented **V0.4 D: character-owned development and minimal versioned saves**, after the focused input gate passed. C1/C2/C3 remain preserved comparisons. See [CONTINUED_DEVELOPMENT_REPORT.md](CONTINUED_DEVELOPMENT_REPORT.md) and [CHARACTER_DEVELOPMENT_PLAYGROUND.md](CHARACTER_DEVELOPMENT_PLAYGROUND.md). All economy/commitment rules remain experimental; player feel and full mastery integration are not complete. Preserve the engine version declared by `ProjectSettings/ProjectVersion.txt` rather than changing it to match the historical V0.1 version target.

## V0.1 — Camera + movement (first task)

**Goal:** A controllable character whose positioning feels good.

- [ ] Unity 6.3 LTS / URP project opens without Console errors.
- [ ] Starter controller package demo works with keyboard and mouse.
- [ ] Separate `MovementTest` scene, with a floor, obstacles, a narrow corridor and open space.
- [ ] Camera-relative WASD movement and responsive change of direction.
- [ ] Mouse-look works comfortably while moving.
- [ ] Mouse wheel adjusts camera distance within reasonable limits.
- [ ] Camera does not pass through walls or become unusable near obstacles (use existing Cinemachine functionality when possible).
- [ ] Character collision prevents walking through obstacles.
- [ ] Inspector-exposed tuning values for movement speed, rotation speed, camera sensitivity/distance (where practical).
- [ ] Test and report what feels awkward: strafe, quick turn, close camera, circling target, movement past obstacles.

**Acceptance test:** Launch Play mode, move around two or more obstacles, circle a stationary dummy, switch directions rapidly, and change camera distance without encountering Console errors or fighting the controls.

**Not in V0.1:** Combat, stamina, enemy AI, equipment, talents, mission framework.

## V0.2 — Light / heavy / dodge combat

**Accepted working baseline:** the user confirmed corrected jumping and described combat timing/balance as decent for continued development. `CombatTest` remains the baseline sandbox. See `COMBAT_SANDBOX.md` for implementation and checks. This is permission to proceed, not a claim that balance is final.

**Stamina tuning deferred by user request:** current regeneration is deliberately permissive for frequent-action/interrupt testing. Do not change costs or regeneration yet. Later compare sustained attack/dodge patterns and downtime, with build-dependent resource effects treated as experiments rather than fixed rules.

- Separate manual Light and Heavy inputs.
- Attack duration/commitment defined by data rather than hardcoded input branching.
- Directional dodge and basic stamina spending + regenerative recovery.
- Simple range/cone hit detection as temporary implementation.
- Debug UI for stamina and action states.
- Actor-local training observations distinguish accepted actions, resolved hits, misses, interruption, poise breaks, and completed dodges. These are playtest diagnostics, not progression awards; see `COMBAT_OBSERVATIONS.md`.
- Attack execution and observations retain immutable parameters committed at acceptance; Inspector edits affect subsequent swings. See `ATTACK_SNAPSHOTS.md` for the implementation and isolated checks.
- Completed attack misses retain range/facing/obstruction feedback for positioning playtests; see `MELEE_MISS_FEEDBACK.md`. The expanded 26-check combat suite previously passed after fixing repeated jump input (see `JUMP_INPUT_FIX.md`); eight isolated snapshot/feedback groups also passed. The player reports fluid combat and subsequently confirmed jumping is fine. These are historical results, not checks performed in the current documentation-only assignment.
- Stationary, resettable training target with health, accumulated poise/stagger, and distinct Light/Heavy hit feedback.
- Preserve jumping, camera controls, and dedicated Q/E strafing. Left Shift becomes dodge in the combat scene; the movement-only scene retains its original controls.
- No enemy AI, equipment, talents, or multiplayer in this milestone.

## V0.3 — First meaningful enemy

Separate `EnemyTest` sandbox; see `ENEMY_SANDBOX.md` for controls, Inspector parameters, implementation boundaries, and player acceptance questions. Existing `CombatTest` and `MovementTest` scenes are preserved.

Implemented for first playtest: guarded and fast collision-aware opponents, committed telegraphs/strikes/recovery, player/enemy vitality and stagger, guard/flank/opening resolution, encounter-local repeated-pressure/flank reactions, reset, and debug feedback. Unity compilation, 11 isolated enemy groups plus a normal-frame smoke, 26 combat regressions, and eight snapshot groups pass. Opponent feel and response fairness await the user's playtest; no progression is enabled.

The user describes this as an interesting playstyle foundation and a good start, and requested continued work. Preserve it for technique comparisons. This positive feedback supports the next experiment without treating opponent balance or every response rule as final.

- A guarded enemy and a fast enemy with different openings.
- Stagger and vulnerability interactions.
- AI behavior inspection/debug logging.
- Enemies react to repeated attacks and positional patterns within an encounter.

## V0.4 — Initial character evolution

**Current playable slice: V0.4 D, ownership and safe-boundary save experiment.** DevelopmentTest starts with Baseline and one test point, offers a confirmed choice of owned Breaker or Momentum, uses real verified evasive Momentum exchanges to reveal a possibility, and separates learning from activating Retaliatory Momentum. A once-only reveal fixture funds the later investment. Safe-spawn configuration, distinct character IDs, bounded knowledge, two explicit versioned slots, backup recovery and protected unknown/future records are implemented. No empty defensive talent is sold. Prior scenes and stamina are preserved.

**Extended depth/readability follow-up:** DevelopmentTest now offers a six-attack session review of committed form, applied impacts, displacement and separate miss/linked recovery costs. Actual HIT feedback reflects resolved guard/poise outcomes. Unknown discovery IDs protect save/backup records without silent rollback. The focused eleven-suite regression runner adds movement/camera coverage and repeatable panel review inspection. This does not add a talent or make the older proficiency trial durable. [NEXT_COMBAT_PLAYTESTS.md](NEXT_COMBAT_PLAYTESTS.md) prioritizes three concrete comparisons before selecting further power; player feel remains unvalidated.

**Character-experience follow-up:** DevelopmentTest records bounded Heavy contact/guard-break/interception facts after completed live actions, including an approach hit before an opponent's first strike. Facts survive R and explicit save/reload, give no stat bonus/points, and cannot duplicate after replay. Schema 1 migrates to schema 2 with empty history and unchanged build. The older practice point/recovery trial is preserved separately. The focused runner now has twelve suites, including native history and migration checks. See [HEAVY_EXPERIENCE_HISTORY.md](HEAVY_EXPERIENCE_HISTORY.md) for retention and [CHARACTER_IDENTITY_PATH.md](CHARACTER_IDENTITY_PATH.md) for the implemented/missing boundaries and three next experiments. This is durable descriptive experience, not completed infinite mastery or an accepted familiarity modifier.

Focused combat (26 cases), full technique input, supported C1/C2/C3 regressions, ownership commands/native exchanges, save failure/recovery and menu lifecycle checks have passed in this session; see the continued report for exact results and limits. These establish functional behavior, not gameplay feel. Production mastery/reward persistence, release economy/respec rules, archive and mission integration remain deferred. The old HeavyPractice trial is not silently serialized into this save model. Playtest the complete choice/discovery/save flow before adding another combat development.

The following design-pass notes are historical scope records; the overnight assignment supersedes their documentation-only restriction for C1/C2/C3.

**Depth-first design refinement:** The user's latest steering prioritizes deeper applications of familiar actions over additional branches. [DEPTH_FIRST_DESIGN.md](DEPTH_FIRST_DESIGN.md) and [HEAVY_DISCIPLINE_DEPTH_STUDY.md](HEAVY_DISCIPLINE_DEPTH_STUDY.md) narrow the proposed research toward one Heavy lineage, contextual decisions, retained liabilities and readable control. New families/features remain dormant unless a demonstrated missing decision justifies them. This changes design priority only: V0.4 B is still the playable implementation, and no Unity or runtime work is authorized for this documentation assignment.

**Future design work, documentation only:** [COMBAT_EVOLUTION_LAB.md](COMBAT_EVOLUTION_LAB.md) maps the foundation into small combat-identity experiments. [CONTEXTUAL_DISCOVERY_EXPERIMENT.md](CONTEXTUAL_DISCOVERY_EXPERIMENT.md) specifies proposed V0.4 C as defensive evidence first, knowledge-only discovery second, and an optional effect comparison third. The user requested this design pass without running the program. These documents do not add runtime features or replace V0.4 B as the current playable slice; implementation, compilation and player validation remain pending.

**Expanded game-design path:** At the user's subsequent request for sustained concept development without Unity execution, [GROWTH_DESIGN_ATLAS.md](GROWTH_DESIGN_ATLAS.md) connects opening sessions, missions, specialization, equipment/abilities, world developments, UI and state contracts. [DEVELOPMENT_EXPERIMENT_BACKLOG.md](DEVELOPMENT_EXPERIMENT_BACKLOG.md) orders small future assignments with exit/stop conditions; [DESIGN_REVIEW_REGISTER.md](DESIGN_REVIEW_REGISTER.md) keeps unresolved policies explicit. This authorizes broader design only; it does not advance implementation status or settle balance/identity decisions.

**V0.4 A, technique comparison:** A separate `TechniqueTest` scene compares baseline Heavy with Breaker (planted poise impact / longer commitment) and Momentum (windup advance / lighter impact / extra recovery on miss). Keys 1/2/3 are debug selectors; RMB remains Heavy. See `TECHNIQUE_EXPERIMENT.md`. Stamina costs/regeneration and prior scenes remain unchanged. Names, parameters, and selection behavior are prototype proposals, not talent ownership or respec rules.

Ready for player comparison: Unity compilation, six isolated technique groups, three native-motor cases (60 Hz / 15 Hz simulation and wall collision), 26 baseline combat checks, 11 enemy groups plus a normal-frame smoke, and eight snapshot groups pass. These are functional checks; variant usefulness and tradeoffs await player testing.

**Current slice: V0.4 B, Heavy practice trial.** `PracticeTest` tests completed Heavy outcomes against recently attacking opponents, explicit credit/rejection reasons, one award per committed enemy strike, pending-action/reset protection, and one optional F2 recovery trial. R starts fresh; no saves/unlocks/discoveries are enabled. See `HEAVY_PRACTICE_EXPERIMENT.md`. Credit rules, threshold, and proficiency effect are proposals. Source identity and pre-impact enemy context are captured in receipts; the effect resolves per actor before commitment without changing shared attack assets or stamina.

Planning reference: `PROGRESSION_DEVELOPMENT_PATH.md` is aligned with `EVOLVING_TALENT_SYSTEM.md` sections 13–14. After combat/enemy readiness, proposed slices compare baseline Heavy with two behavior-changing variants, validate one meaningful-use mastery track, reveal one contextual interaction, and connect deliberate investment to minimal UI and character save/reload. Names, formulas, thresholds, and specialization/respec mechanisms remain subject to design review and playtesting.

- Small handcrafted base talent tree.
- Trigger/condition/effect talent definitions with explicit deterministic rules.
- One mastery track and 2–3 discoverable adaptations.
- Talents alter the rhythm/choices of play, not just damage numbers.
- Include minimal versioned character persistence in the first complete progression prototype; integrate it into the mission loop in V0.5. A fresh Warrior inherits no mastery, purchases, or character-specific prerequisites. A full account archive is deferred.

## V0.5 — First mission loop

- Simple hub/test lobby -> instanced mission -> rewards -> return.
- Save a character's persistent talents, equipment prototype, and recent discovery flags.
- Basic enemy-variant persistence, if the encounter adaptation test is already fun.

## Later

Expanded weapon types (including charging magic attacks), equipment/itemization, content variety, procedural/combined evolutions, co-op feasibility testing, visual polish, extensive worldbuilding.
