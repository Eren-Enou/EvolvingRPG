# Melee miss feedback

This V0.2 slice makes positioning tests easier to interpret. Completed attack misses now retain a geometry explanation in `CombatActions.Message` and the latest training observation shown by the existing HUD. No extra scene components or input bindings are required.

## Try it

Open `Assets/Game/Scenes/CombatTest.unity`, enter Play mode, and click the Game view. Use LMB or RMB:

- Swing far from the dummy: `no target in reach`.
- Swing near the dummy while facing away: `target outside attack arc`.
- Put a graybox wall between player and dummy, within attack range: `strike obstructed`.
- Strike a defeated dummy before its reset: `target unavailable`.

R still resets the dummy and F6 clears observations. Successful hits retain normal hit feedback. Dodges still report `evasion unverified`; miss feedback does not establish an enemy encounter, successful evasion, mastery eligibility, or any progression reward.

## Meaning and limitations

The detector records the most specific failed geometry gate observed across the attack's active window. If any target was hit, the completed action is a hit, even if other candidates were blocked. With several targets or changing positions, the explanation does not identify a particular target or claim to be the final frame's geometry.

Queries still use the existing configured target/obstruction masks, chest-height sphere, facing cone, closest collider point, and ray obstruction test. The hit geometry, attack balance, movement, camera, and hit deduplication rules are unchanged. There are no new searches outside the original overlap query.

The overlap buffer holds 64 colliders. When a query fills it and no target has been hit, feedback conservatively reports `query capacity reached; result incomplete` rather than claiming no target exists. A full buffer can be exactly full or truncated; the diagnostic cannot distinguish those cases. This does not enlarge the query or change hit behavior.

The observation's `MissReason` is set only for completed attacks with zero target hits. Acceptance, interruption, resolved hits, and completed dodges use `None`. History retains the enum value, so later movement or target reset does not rewrite an earlier explanation.

## Files

- Added `Assets/Game/Combat/MeleeMissReason.cs` and Unity-generated `.meta`: small reason enum and shared presentation strings.
- Updated `Assets/Game/Combat/MeleeHitDetector.cs`: snapshot overload returning query feedback, with existing overloads retained.
- Updated `Assets/Game/Combat/CombatActions.cs`: aggregate active-window feedback and retain it on completed misses.
- Updated `Assets/Game/Combat/CombatObservation.cs`: immutable completed-miss reason and summary.
- Extended `Assets/Game/Editor/AttackSnapshotChecks.cs`: isolated range, facing, wall, defeated-target, query-capacity, successful/deduplicated hit, completion-history, and dodge separation assertions.
- Updated `Assets/Game/Editor/CombatPlayModeChecks.cs`: existing range/facing/wall integration checks now assert the completed reason too.
- Added this guide and linked it from the combat and progression documentation.

## Validation

Unity 6000.6.7f1 compiled successfully. All eight isolated snapshot/feedback Play-mode check groups passed, with no runtime Console errors captured. Run **Tools > EvolvingRPG > Run Attack Snapshot Checks** with saved scenes and Play mode stopped; its report is `Logs/AttackSnapshotChecks.txt`.

The combined **22-check combat/input Play-mode suite also passed in one uninterrupted run**, including the new completed-miss assertions and the existing movement, jump, camera, dodge, impact, stamina, and observation checks. No runtime Console errors were captured. Its report is `Logs/CombatPlayModeChecks.txt`; run **Tools > EvolvingRPG > Run Combat Play Mode Checks** with Unity focused to repeat it. These functional checks do not validate gameplay feel.
