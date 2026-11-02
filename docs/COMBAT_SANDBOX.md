# V0.2 combat foundations

## Launch

Use the already-running Unity Editor (**6000.6.7f1**, the project's current version). Open `Assets/Game/Scenes/CombatTest.unity`, press **Play**, and click the Game view. No manual scene wiring, paid assets, or additional packages are needed. `MovementTest` remains the movement-only playground.

If CombatTest is missing on another checkout, allow script import to finish or use **Tools > EvolvingRPG > Create Combat Sandbox**. The builder copies the saved MovementTest into a new scene, wires components through Unity's API, restores the previously active scene, and refuses to overwrite an existing CombatTest. Existing scene edits are not saved by the builder. Keep the new scene, data/material assets, and all `.meta` files in version control.

## Controls and action rules

| Input | Behavior |
| --- | --- |
| WASD | Existing camera-relative movement and turning while idle |
| Q/E | Existing facing-preserving strafe; combines with W/S |
| LMB | Light attack: quick windup and recovery, limited repositioning |
| RMB | Heavy attack: longer windup/recovery, movement locked, larger damage/poise impact |
| Left Shift | Directional dodge; this replaces sprint **only in CombatTest** |
| Space | Existing jump while idle; attacks/dodges require grounding and suppress jumping until recovery ends |
| Mouse / wheel | Existing orbit / zoom, including during actions |
| R | Reset target health, accumulated poise, and hit feedback |
| Escape / click | Release / capture cursor; the click used to recapture does not attack |

Each click starts one attack. Holding a button does not repeat. There is no mandatory combo, buffered follow-up, or attack/dodge cancel during commitment. Simultaneous fresh inputs prioritize Dodge, then Heavy, then Light. Invalid inputs are ignored rather than queued. Facing is held throughout an action; position and facing at commitment matter. Misses still cost time and stamina.

Dodges snapshot the current movement direction, including Q/E strafing, and preserve facing. With no movement input, dodge goes backward. Travel is 3 m over 0.25 s on unobstructed flat ground, followed by 0.35 s recovery. Collision can shorten travel. No invulnerability is implemented. Holding Shift cannot repeat dodges, and stamina limits repeated presses. Jumping, mouse-look, and zoom retain the Starter Assets foundation.

## Starting tuning values

These are starting points for player feedback, not settled balance decisions.

| Parameter | Light | Heavy |
| --- | --- | --- |
| Windup / active / recovery | 0.12 / 0.10 / 0.22 s | 0.50 / 0.18 / 0.65 s |
| Total commitment | 0.44 s | 1.33 s |
| Damage / poise impact | 15 / 12 | 45 / 40 |
| Stamina cost | 8 | 20 |
| Range / full cone angle | 2.2 m / 90 degrees | 2.5 m / 70 degrees |
| Movement speed multiplier | 0.35 | 0 |

Edit `Assets/Game/Combat/Data/Light.asset` and `Heavy.asset` in the Inspector for timing, damage, poise, stamina, range/arc, movement restriction, and feedback color. Those ScriptableObjects define the behavior without changing input code. Tune assets outside Play mode; asset edits made during Play mode can persist.

On **Player / Combat Actions**, tune dodge distance, duration, recovery, and cost (25 stamina). Input System button actions are exposed there. On **Player / Combat Stamina**, tune capacity (100), continuous active regeneration (12/s), and faster idle regeneration (30/s). Ordinary repositioning is free; attacks/dodges spend stamina. All regeneration continues during actions, consistent with the vision.

On **Training Target / Training Target**, tune health (250), poise threshold (60), poise recovery delay/rate (1.5 s / 10 per second), stagger duration (0.8 s), and defeated-target automatic reset (3 s; 0 disables automatic reset). Heavy twice quickly breaks the default poise threshold. While already staggered, further hits deal damage without accumulating a second break. Defeated targets reject damage until reset. The collider stays solid throughout.

## Feedback and architecture

The procedural practice blade visibly winds up and sweeps. Light is cyan and Heavy is orange. A ground sector shows the approximate attack reach and facing; it becomes brighter during the active window. The target flashes in the attack color, visibly wobbles more from larger poise hits, turns yellow when staggered, and tilts/darkens on defeat. This is placeholder feedback, not a finished weapon/animation system.

The debug overlay shows controls, action phase/progress, hit/miss status, stamina, target health/poise, and the last damage/stagger result. There are no gameplay menus.

- `AttackDefinition` contains attack data; `CombatActions` owns the timed state and one-hit-per-target set.
- `MeleeHitDetector` performs range/cone/height and solid-obstacle checks. `TrainingTarget.ReceiveHit` resolves health/poise separately. Multiple colliders cannot multiply damage within one swing.
- `CombatStamina` owns spending and continuous regeneration.
- `CombatMovement` temporarily supplies action movement restrictions or the dodge velocity to the existing Starter Assets motor. It does not implement another movement/gravity/collision system.
- Update order: Mouse Look -100, Dedicated Strafe -50, Combat Actions -40, Combat Movement -25, vendor controller 0, Combat Movement Restore +25, Strafe Controller Restore +50. Nested restoration preserves movement inputs/tuning and facing before animation/camera evaluation. Keep the restoration companions enabled. Future Update-based combat readers should run after +50.
- Target and player feedback components own only visuals; `CombatSandboxHUD` owns debug display and target reset input.

## Validation and player checklist

Unity **6000.6.7f1** compiled the runtime and Editor scripts successfully. All **19 automated Play-mode checks passed** in the existing Editor, with no runtime Console errors captured:

- Light damage/poise and one hit per swing; Heavy windup/damage/movement commitment; held-button and attack/dodge-cancel rejection.
- Range/facing misses, walls blocking hits, multiple target colliders counting only once, and poise threshold producing stagger.
- Q+Shift lateral dodge and Q+W+Shift normalized diagonal dodge on clear ground, fixed travel distance, held-Shift chain prevention, and dodge collision against a wall.
- Insufficient-stamina rejection, WASD/Space jump regression, mouse-look/zoom during Heavy, R reset, defeated-target hit rejection/automatic reset, and continuous active stamina regeneration.

Unity returned to Edit mode after the checks. All 23 external asset GUIDs referenced directly by CombatTest resolved to existing project/package assets. MovementTest's SHA-256 remained `1CCDCE3F228AE74BD8303CE00626FAA7C7C851B60BEEA656F41C5B356E1A42A5` before and after development. Existing movement scripts, Packages, and ProjectSettings have no changes from this task. Existing package warnings were not changed. These results do **not** validate subjective combat feel.

To rerun checks, save any open scene edits, leave Play mode, and select **Tools > EvolvingRPG > Run Combat Play Mode Checks**. It opens CombatTest in the existing Editor, uses isolated virtual input devices and temporary runtime objects, then returns to Edit mode. Keep Unity focused while it runs. It never launches a second Editor. The ignored local reports are `Logs/CombatSandboxCreation.txt` and `Logs/CombatPlayModeChecks.txt`.

For player acceptance:

1. Face the green target from the starting position. Click LMB, then RMB after recovery. Compare windup, recovery, motion restriction, blade sweep, health loss, and poise gain.
2. Repeatedly click and hold attacks. Confirm no free spam, mandatory combo, or cancellation during recovery. Check stamina spending and regeneration.
3. Move out of range, face away, and attack with a wall between you and the target. Confirm misses. Circle the target with Q/E and reposition for another strike.
4. Dodge forward, backward, sideways, and diagonally, including Q/E+W+Shift. Confirm facing stays stable and the camera remains controllable. Try dodging toward a wall.
5. Hold Shift and repeatedly press it. Confirm held input does not repeat, recovery prevents immediate chains, and low stamina rejects actions.
6. Jump with Space while idle. Orbit and zoom during Heavy and dodge. Release/reacquire the cursor without accidentally attacking.
7. Land Heavy attacks close together to stagger the target. Reset with R, defeat it, and observe automatic reset. Check the Console throughout.

## Known limits and open decisions

- Attacks use a simple chest-height sphere/cone query and line-of-sight check, not animated weapon hitboxes. The reach guide is approximate; the nearest collider surface determines range. Queries support up to 64 nearby colliders per sample, sufficient for this one-target sandbox.
- Starter Assets locomotion animations remain unchanged. The procedural blade and target reactions communicate actions; there are no authored attack/dodge clips, hit stop, sound design, camera shake, knockback, or body lean yet.
- Actions are grounded-only. Space input during an action is suppressed; holding Space until recovery can allow a jump afterward. Unobstructed dodge distance is tuned for flat ground; slopes, steps, and collisions can alter the result.
- The player has stamina but no health or incoming-damage system. Dodge has no invulnerability, and stagger affects only the stationary training target. There is no enemy AI, equipment, multiplayer, or talent system.
- Target reset restores the target only, not player stamina. Wait to refill stamina naturally.
- Player choices still needed: Light repositioning amount; Heavy cancellation policy; dodge distance/recovery/cost; whether sprint needs a new binding; and eventual animations/air-action rules. Nothing here fixes the long-term combat or character progression design.

## Exact files introduced or updated

All paths are relative to the Unity project root. Every new Unity asset/folder also has its Unity-generated `.meta` file.

- New `Assets/Game/Combat/` folder and `Data/` subfolder.
- New scripts in that folder: `AttackDefinition.cs`, `HitData.cs`, `TrainingTarget.cs`, `MeleeHitDetector.cs`, `CombatStamina.cs`, `CombatActions.cs`, `CombatMovement.cs`, `CombatMovementRestore.cs`, `TrainingTargetFeedback.cs`, `CombatActionFeedback.cs`, `CombatSandboxHUD.cs`.
- New data: `Assets/Game/Combat/Data/Light.asset`, `Heavy.asset`, `PracticeBlade.mat`, `PracticeBody.mat`, `RangeGuide.mat`.
- New `Assets/Game/Editor/CombatSandboxBuilder.cs` and `CombatPlayModeChecks.cs`.
- New `Assets/Game/Scenes/CombatTest.unity`.
- New `docs/COMBAT_SANDBOX.md`; updated `docs/MILESTONES.md` and `AGENTS.md` to record the explicitly authorized V0.2 scope.

Existing movement scripts, MovementTest, camera scripts, vendor assets, Packages, and ProjectSettings are preserved. Prior uncommitted V0.1.1 changes are retained.
