# Development milestones

**Approach:** Validate one mechanic at a time. Each milestone should produce something playable. The vision document is the long-term compass, not the first milestone's scope.

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

**Current assignment:** a separate playable `CombatTest` sandbox building on the player-tested V0.1/V0.1.1 movement foundation. See `COMBAT_SANDBOX.md` for implementation and acceptance steps. Timing and movement feel remain subject to player testing.

- Separate manual Light and Heavy inputs.
- Attack duration/commitment defined by data rather than hardcoded input branching.
- Directional dodge and basic stamina spending + regenerative recovery.
- Simple range/cone hit detection as temporary implementation.
- Debug UI for stamina and action states.
- Stationary, resettable training target with health, accumulated poise/stagger, and distinct Light/Heavy hit feedback.
- Preserve jumping, camera controls, and dedicated Q/E strafing. Left Shift becomes dodge in the combat scene; the movement-only scene retains its original controls.
- No enemy AI, equipment, talents, or multiplayer in this milestone.

## V0.3 — First meaningful enemy

- A guarded enemy and a fast enemy with different openings.
- Stagger and vulnerability interactions.
- AI behavior inspection/debug logging.
- Enemies react to repeated attacks and positional patterns within an encounter.

## V0.4 — Initial character evolution

- Small handcrafted base talent tree.
- Trigger/condition/effect talent definitions with explicit deterministic rules.
- One mastery track and 2–3 discoverable adaptations.
- Talents alter the rhythm/choices of play, not just damage numbers.

## V0.5 — First mission loop

- Simple hub/test lobby -> instanced mission -> rewards -> return.
- Save a character's persistent talents, equipment prototype, and recent discovery flags.
- Basic enemy-variant persistence, if the encounter adaptation test is already fun.

## Later

Expanded weapon types (including charging magic attacks), equipment/itemization, content variety, procedural/combined evolutions, co-op feasibility testing, visual polish, extensive worldbuilding.
