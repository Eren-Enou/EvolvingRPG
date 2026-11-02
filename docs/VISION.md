# Game vision — Evolving third-person action RPG

## Core identity (non-negotiable)

> The game shouldn't be about reaching the strongest version of your character. It should be about discovering what your character can become, in a world that's changing alongside you.

A character's identity emerges through intentional choices, experiences, and adaptations. The next progression path may be unknown, but must remain discoverable by consistent, inspectable underlying rules; discovery should not be arbitrary.

## World structure

- Inspiration: *Phantasy Star Online Episodes 1 & 2* — central hub, atmospheric instanced mission environments, repeatable encounters, bosses, loot.
- Equipment inspiration: *World of Warcraft* — slot-based gear, stats, and interesting item effects.
- Third-person action combat, not stationary MMO rotations. Positioning, timing, and the creation/exploitation of openings should matter as much as character optimization.

## Combat fundamentals under discussion

- Precise, fluid, camera-relative movement, with mouse-look and adjustable third-person camera.
- Manual **Light Attack** and **Heavy Attack** as separate inputs; no mandatory fixed three-hit rhythm.
- Light actions are flexible; heavy actions require commitment and should meaningfully influence enemy stagger/guard.
- Stamina limits continuous exertion while **always regenerating**; regeneration accelerates when the character is less active. Ordinary walking/repositioning should be encouraged.
- Stagger/poise is a separate mechanic: hits destabilize combatants, communicated by animation and audio instead of requiring constant meter inspection.
- Enemy behavior reacts to player tactics during an encounter; long-term enemy populations may eventually develop persistent variations.
- For magic weapons, consider basic attacks charged by holding and releasing at any point, and heavy attacks as committed casts (exact interrupt behavior TBD).
- MMO-like active skills, cooldowns, and equipment enrich action combat without requiring dozens of concurrent meters.
- Attack range, facing, and geometry matter long term, but precise physical weapon hitboxes are deferred for early prototypes.

## Evolution and progression

1. **Base trees:** curated class/archetype foundations.
2. **Mastery:** indefinitely extensible vertical improvement in existing talents/abilities.
3. **Adaptations:** playstyle can influence *which* options are revealed, without taking away the player's agency to select them.
4. **Evolutions:** new interactions and specializations can emerge from previous choices and abilities.
5. **Enemies evolve too:** immediate reactions, encounter-level adaptation, and potentially persistent population-level variations.

Design rule: growth should create new ways of playing, not just numerical power. Character histories should matter, but discoveries should be fair and explainable once understood. Avoid an evolutionary system that merely invalidates the player's successful choices.

## Deferred decisions

- Precise weapon collision and hitbox implementation.
- Number of classes, attributes, equipment tiers, and item slots.
- Multiplayer/network architecture (assess as future ambition before committing to irreversible design).
- Procedural generation versus curated evolution rules.
- Permanent character choices, respec mechanisms, and difficulty scaling.
