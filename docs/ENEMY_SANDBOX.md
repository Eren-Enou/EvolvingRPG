# V0.3 first opponents

The user gave positive feedback on this playstyle foundation and requested continued work. The next isolated experiment is [Heavy technique comparison](TECHNIQUE_EXPERIMENT.md) in a separate TechniqueTest scene; EnemyTest remains the opponent reference. F1 can now toggle detailed opponent diagnostics without changing gameplay.

The user accepted corrected jumping and the current combat timing/balance as a working baseline, then requested the next goal. This sandbox introduces meaningful targets and incoming threats before progression experiments. Stamina costs, maximum, and active/idle regeneration are unchanged by explicit request; frequent actions are useful for interruption testing. Resource balance and any build-dependent evolution remain future playtest work.

## Launch and controls

In the already-running Unity **6000.6.7f1** Editor, open **Assets/Game/Scenes/EnemyTest.unity**, press **Play**, and click the Game view. No manual component wiring, extra packages, paid assets, or baked NavMesh are required. If the scene is absent on another checkout, wait for script import or choose **Tools > EvolvingRPG > Create Enemy Sandbox**. The builder creates only the new scene and refuses to overwrite it.

WASD, Q/E strafe, Space jump, LMB Light, RMB Heavy, Left Shift dodge, mouse-look, and wheel zoom retain their baseline behavior. Escape releases the cursor. **R resets the full encounter**: player health/poise/position, enemies, their encounter memory/counters, and player observations. It interrupts the current player action. Stamina continues its existing regeneration; reset does not grant a stamina refill. F6 clears only player observations.

Start between two lanes. The **guarded fighter is on the left**, the **fast fighter on the right**. Approach one at a time; within 8 metres they engage. Both can engage if you bring them together. Defeated enemies stay down until R. Player defeat blocks movement/actions while leaving camera and reset available.

## What to test

| Opponent | Intended decision | Starting behavior |
| --- | --- | --- |
| Guarded | Break poise with Heavy, circle its guard, or punish recovery | 140-degree frontal guard reduces HP damage to 20% while taking full poise. Slower turn/approach; long committed strike. |
| Fast | Bait a quick strike, reposition during its committed facing, exploit a short opening | No guard, lower HP/poise, faster approach/strike, shorter recovery. Heavy can break its poise in one hit. |

Enemy facing locks when windup starts; there is no last-moment auto-aim. Ground sectors show reach and arc. **Yellow = windup, red = active strike, green = recovery opening, purple = stagger, gray = defeated.** White flashes indicate a received hit. Procedural weapon sweeps are placeholders, not physical weapon hitboxes. The visible shield exists only while guard is available.

Incoming hits are checked throughout the active window, once per swing, using range, arc, and obstruction. Walls block strikes. Dodges use position and offer **no invulnerability**. Hits can still land during a dodge if the player remains inside the strike. A missed enemy strike is not automatically recorded as a successful evasive dodge; attribution/discovery rules remain deferred.

Player health and poise introduce meaningful interruption: ordinary damage does not cancel an action, but a poise break interrupts it and briefly blocks movement/actions. Defeat also interrupts. Enemy poise breaks cancel their current attack and create a stagger opening. Recovery and stagger expose enemies to **1.25x HP damage**; this does not multiply poise damage. Applied-hit receipts distinguish guarded and opening hits.

### Starting parameters (proposals)

| Parameter | Guarded | Fast |
| --- | --- | --- |
| HP / poise threshold | 240 / 60 | 160 / 30 |
| Speed / turn rate | 2 m/s / 120 deg/s | 3.6 m/s / 180 deg/s |
| Windup / active / recovery | 0.85 / 0.16 / 1.15 s | 0.45 / 0.12 / 0.70 s |
| Damage / poise impact | 18 / 20 | 12 / 12 |
| Strike reach / arc | 1.9 m / 80 deg | 1.9 m / 80 deg |
| Stagger duration | 1.2 s | 0.85 s |

Player starts with 150 HP, a 40-poise threshold, and 0.65-second stagger. Both sides recover poise after two seconds without a hit at eight points/second. Existing attack and dodge assets are reused unchanged.

## Small, inspectable encounter reactions

These are authored deterministic responses, not persistent learning or an inferred player identity:

- Three consecutive low-poise hits (at most 15 poise each) within the six-second memory window identify repeated Light pressure. A subsequent guarded hit has its already-reduced HP damage halved again; full poise still lets the player break that brace. The fast fighter instead steps backward during the first 0.4 seconds of its next recovery.
- Two consecutive hits from the same flank (beyond 70 degrees off the front) increase tracking turn speed by 50% outside committed actions. Windup/active facing remains fixed.
- Memory expires after six seconds without a received hit and clears on encounter reset. HUD text explains the currently visible response. Enable `Log Transitions` on `SandboxEnemy` for optional Console state logs; it is off by default.

The HUD shows each opponent's HP, poise, phase, received impact, last strike, and observed pattern. Player observations retain per-action identity, actual receipts, and interruption. No XP, mastery, talents, unlocks, style classification, or persistence is awarded.

## Inspector configuration

Select an opponent root:

- `SandboxEnemy`: player reference, guard archetype, movement/turn speed, awareness/leash, strike reach/arc/timing/impact, pattern threshold/memory, transition logging.
- `EnemyVitality`: HP, poise threshold, stagger, poise recovery, frontal guard arc/damage multiplier, and opening damage multiplier.
- `EnemyFeedback`: body, weapon pivot, shield indicator, and ground-sector references.

Select Player for `CombatVitality` HP/poise/stagger parameters. Existing `CombatActions`, attack assets, and `CombatStamina` retain their tuning locations. Do not tune stamina during the current interruption investigation.

## Known limitations and next decisions

Enemies use collision-aware direct pursuit and a home leash, with no navigation/pathfinding. They can get stuck behind walls; the central wall is useful for obstruction testing, not a navigation benchmark. There is no attack-slot coordination, so overlapping enemies can both strike. Color/shape/HUD feedback and simple sweeps substitute for proper animation, audio, and hit reactions. Reset during a jump can retain a brief part of the native motor's vertical motion; it does not modify vendor velocity internals.

Playtest questions: Can you read commitment before a strike? Can you distinguish the two recovery windows? Are guard breaks and player stagger understandable? Does flanking remain useful when the opponent adjusts tracking? Do repeated-pressure responses create choices without erasing successful playstyles? Stress-click freely to identify interruption edges; stamina balance is deliberately deferred. These timings, multipliers, memory thresholds, and response rules remain experiments.

## Files changed

- Added runtime `Assets/Game/Combat/HitReceiver.cs`, `CombatVitality.cs`, `EnemyVitality.cs`, `SandboxEnemy.cs`, `EnemyFeedback.cs`, and `EnemySandboxHUD.cs`, each with Unity-generated `.meta`.
- Added `Assets/Game/Editor/EnemySandboxBuilder.cs` and `EnemyPlayModeChecks.cs`, with `.meta`.
- Added Unity-generated `Assets/Game/Scenes/EnemyTest.unity` and `.meta`.
- Updated `TrainingTarget.cs` to implement the receiver boundary; its behavior/serialized fields remain intact.
- Updated `MeleeHitDetector.cs` for generic receivers, attacker exclusion, and source position. Updated `HitData.cs` with source context and an incoming-impact constructor.
- Updated `CombatObservation.cs` for generic target attribution and guarded/opening receipt flags. Training summaries remain distinguishable.
- Updated `CombatActions.cs` for optional vitality, external stagger/defeat interruption, and rejection while disabled/incapacitated. Updated `CombatMovement.cs` to stop movement while staggered/defeated.
- Updated `AttackSnapshotChecks.cs` for the generic receiver set, and project/milestone/path documentation for the accepted baseline and V0.3 assignment.
- Updated `MeleeMissReason.cs` and its snapshot assertions/documentation to say `no target in reach`, since receivers now include opponents as well as training targets.

`MovementTest`, `CombatTest`, Starter Assets, camera/input scripts, shared player attack assets, `CombatStamina`, Packages, ProjectSettings, and the user-provided talent charter are preserved.

## Automated validation

Use **Tools > EvolvingRPG > Run Enemy Play Mode Checks**, with scenes saved and Play mode stopped. It opens EnemyTest and performs isolated guard, hit-resolution, commitment, obstruction, player interruption, adaptation, and reset checks, followed by a short normal-frame smoke check. The report is written to ignored `Logs/EnemyPlayModeChecks.txt`. Existing combat and snapshot checks remain available in the same menu. Functional checks do not establish opponent gameplay feel; the user performs interactive playtesting.

Unity **6000.6.7f1** compiled successfully. All **11 isolated enemy check groups**, the **normal-frame enemy smoke check**, all **26 baseline combat/input checks**, and all **8 snapshot/feedback check groups** passed. No runtime Console errors were captured by those checks. The normal-frame smoke exercised approach, telegraph, incoming player damage, feedback/HUD callbacks, and encounter reset. The initial enemy check caught attacker self-detection after generic receivers were introduced; the detector now excludes its own receiver, and the passing regression explicitly verifies the attacker takes no self-hit.

Local reports: `Logs/EnemyPlayModeChecks.txt`, `Logs/CombatPlayModeChecks.txt`, and `Logs/AttackSnapshotChecks.txt`; compilation/play logs are in `Logs/ObservationValidation.log`. A visual Editor check confirmed the HUD and colored placeholder bodies display. EnemyTest is left open outside Play mode for the player's interactive test. The original two scene hashes and stamina/attack asset hashes were unchanged.
