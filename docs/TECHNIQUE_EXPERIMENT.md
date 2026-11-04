# V0.4 A — Heavy technique comparison

This is the first independently playable experiment toward character evolution. The user gave positive feedback on the opponent foundation and requested continued development. It compares three authored Heavy actions against the same opponents, without adding mastery, discovery, investments, saves, or a talent tree. Names, timings, and tradeoffs remain proposals.

## Launch

Use the existing Unity **6000.6.7f1** Editor. Open **Assets/Game/Scenes/TechniqueTest.unity**, press **Play**, and click the Game view. No manual wiring or package installation is required. If missing on another checkout, wait for script import or choose **Tools > EvolvingRPG > Create Technique Sandbox**. The builder copies the saved EnemyTest into a new scene and refuses to overwrite an existing TechniqueTest or variant asset.

| Control | Experiment behavior |
| --- | --- |
| 1 / 2 / 3 (number row) | Select Baseline / Breaker / Momentum while ready |
| RMB | Execute the selected Heavy |
| LMB / Shift / Space | Existing Light / directional dodge / jump |
| WASD / Q-E / mouse / wheel | Existing movement, strafe, camera look and zoom |
| R | Reset encounter; retain the currently selected technique |
| F1 | Toggle detailed opponent diagnostics; compact view starts enabled here |
| F6 / Escape | Clear observations / release cursor |

Switching during commitment, stagger, defeat, or while the action component is disabled is rejected without queueing. A swing already started cannot change its definition. New Play sessions start on Baseline. Debug selection grants no ownership and establishes no future respec, specialization, or active-slot policy.

The guarded fighter is on the left, the fast fighter on the right. Opponent behavior, health, guard, damage, stagger, and openings match EnemyTest. Use R between comparisons. Original MovementTest, CombatTest, and EnemyTest scenes remain intact.

## Hypothesis and starting parameters

**Hypothesis:** meaningful technique evolution can change positioning and commitment choices while preserving the recognizable RMB Heavy action. Planted disruption should reward preparing a guard break; committed advancement should help close distance but expose an inaccurate approach to a longer miss recovery. Test whether these tradeoffs produce choices rather than one universally preferable upgrade.

| Parameter | Baseline | Breaker | Momentum |
| --- | --- | --- | --- |
| Windup / active / recovery | 0.50 / 0.18 / 0.65 s | 0.65 / 0.18 / 0.85 s | 0.40 / 0.16 / 0.45 s |
| Damage / poise | 45 / 40 | 45 / 60 | 40 / 25 |
| Forward windup travel | 0 | 0 | 1.2 m, collision-limited |
| Extra miss recovery | 0 | 0 | 0.55 s |
| Total commitment on hit / miss | 1.33 / 1.33 s | 1.68 / 1.68 s | 1.01 / 1.56 s |
| Stamina cost | 20 | 20 | 20 |
| Placeholder blade color | Orange | Purple | Teal |

All three use the baseline 2.5 m reach, 70-degree arc, and zero free movement during commitment. Breaker can break the fresh guarded opponent's 60-poise threshold in one impact; baseline requires accumulated hits. Momentum takes more hits to break that guard and commits its forward travel to the initial facing. Obstacles and enemy bodies can shorten that travel.

**Stamina is intentionally unchanged.** Maximum, regeneration, baseline Light/Heavy/dodge costs, and the two variant Heavy costs retain the current testing policy. Balance resource gain/use later, including possible build effects, after interruption behavior is understood.

## Compare in Play mode

1. Start with Baseline. Fight each opponent and note which openings let Heavy land safely.
2. Reset, select Breaker, and compare frontal guard breaks, preparation time, and whether its longer recovery leaves useful follow-up choices.
3. Reset, select Momentum, and attack from just outside ordinary reach. Compare approach commitment, collision, and exposure to a telegraphed strike. Mouse movement must not steer the advancing character.
4. Deliberately miss with Momentum. Its recovery should last longer than on a hit. A guarded hit still counts as contact; a wall-blocked or out-of-range swing does not.
5. Try switching during a swing and dodge. The selection stays unchanged until a fresh selection while ready. Repeat jump, strafe, camera, and rapid clicking checks.

Record the selected technique, opponent/state, starting distance, hit or miss, any interruption, and whether the tradeoff changed a decision. Retain/revise/discard each proposal from that feedback. Automated checks do not establish combat feel or approve the next mastery rules.

## Inspector tuning and implementation

Variant definitions are at `Assets/Game/Combat/Data/Techniques/Breaker.asset` and `Momentum.asset`. Baseline directly references the original `Heavy.asset`; no duplicate baseline or shared-asset mutation is used. The player has `HeavyTechniqueSelector` with explicit references to the three definitions.

`AttackDefinition` adds two zero-default fields: `AdvanceDistance` and `MissRecovery`. Older actions therefore keep their existing behavior. `AttackSnapshot` captures both on acceptance alongside all existing values. Character-specific resolution can later supply an effective action before commitment; no general modifier framework is added here.

`CombatActions` integrates only the windup portion of each frame to derive forward travel speed. `CombatMovement` converts that committed world direction through the current camera yaw and supplies the existing Starter Assets motor. The vendor remains the only player movement integrator; there is no second CharacterController.Move, transform translation, steering, invulnerability, or extra jump handling. Geometry can shorten movement normally.

After the active window, recovery uses the committed base recovery plus `MissRecovery` only when no receiver accepted a hit. Interruption clears the pending advance. Observations retain the committed definition and values; the experiment awards no mastery or discovery credit.

## Limitations

Placeholder blade sweeps and colors distinguish proposals; proper animation, motion matching, weapon contact, audio, and hit-stop remain future work. Advancement runs during windup and can be interrupted by poise break; it is not a charge ability with steering. Hit sampling uses the existing cone/range query before this frame's motor movement, so a coarse frame at the reach boundary can differ from a fine frame; precise weapon collision remains deferred. The inherited opponent navigation/reset limitations are documented in `ENEMY_SANDBOX.md`.

No stable save IDs, mastery rewards, talent ownership, contextual evasion attribution, account archive, or persistence are implemented. In particular, a completed dodge still does not prove a successful evasion. After useful behavioral differences survive player feedback, the next experiment can test explicitly eligible Heavy mastery outcomes separately from dummy repetition.

## Changed files

- Added `Assets/Game/Combat/HeavyTechniqueSelector.cs` and `.meta`.
- Added `Assets/Game/Editor/TechniqueSandboxBuilder.cs`, `TechniquePlayModeChecks.cs`, and their `.meta` files.
- Added Unity-generated `Assets/Game/Scenes/TechniqueTest.unity` and `.meta`.
- Added the two variant assets and their containing folder `.meta` under `Assets/Game/Combat/Data/Techniques`.
- Updated `AttackDefinition.cs`, `AttackSnapshot.cs`, `CombatActions.cs`, and `CombatMovement.cs` for zero-default advancement/miss recovery and committed execution.
- Updated `EnemySandboxHUD.cs` with optional technique information, compact details, and F1; existing EnemyTest keeps its detailed default.
- Updated this guide, `AGENTS.md`, `MILESTONES.md`, and `PROGRESSION_DEVELOPMENT_PATH.md` to define the narrow current experiment.

Existing scenes, baseline attack assets, CombatStamina, movement/camera input scripts, Starter Assets, Packages, ProjectSettings, and the user-provided talent charter are preserved.

## Automated checks

Run **Tools > EvolvingRPG > Run Technique Play Mode Checks** with saved scenes and Play mode stopped. It opens TechniqueTest, checks data tradeoffs, selection/commitment, hit/miss recovery, Breaker's guard break, snapshot isolation and interruption/dodge separation, then exercises the native motor at 60 Hz and 15 Hz simulation steps and against a solid wall. It temporarily controls the simulation step and restores it afterward. Report: `Logs/TechniquePlayModeChecks.txt` (ignored local evidence).

Unity 6000.6.7f1 compiled successfully. All six isolated technique groups (including number-key binding resolution) and three normal-frame motor cases passed without captured runtime Console errors. The 26 baseline combat/input checks, 11 enemy groups plus their normal-frame smoke, and eight snapshot/feedback groups also passed after the shared execution changes. These verify functional contracts and motion integration; player testing is required to judge technique usefulness, timing, and balance.

The final input review caught and corrected number-row paths (`<Keyboard>/1`, `/2`, `/3` rather than property-style names). The regression explicitly checks that they resolve to keyboard controls and uses temporary Input System keyboard events to verify press edges, restoring bindings/devices afterward. The complete technique suite passed again after that correction. A visual Editor check confirmed the compact experiment HUD displays; physical-control feel and technique usefulness remain part of the player's test. Original scene, baseline asset, and stamina hashes are unchanged. TechniqueTest is left open outside Play mode.
