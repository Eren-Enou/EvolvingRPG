# Combat jump request fix

Player report: a single Space press caused continuous jumping; combat otherwise felt fluid. This feedback is not blanket acceptance of every combat mechanic.

## Cause and change

`CombatMovement` cached `StarterAssetsInputs.jump` before the native motor update and restored it afterward, even while idle. Starter Assets deliberately consumes that request by clearing it while airborne. Restoring the cached `true` resurrected the request, so landing eventually triggered another takeoff without a new press.

The combat adapter now leaves the consumed jump value alone. While committed to an attack/dodge it still sets jump to false, discarding blocked presses instead of buffering them until recovery. It continues to restore temporary movement/sprint/tuning/facing overrides as before. Native jumping, gravity, movement integration, and animation remain owned by Starter Assets.

Runtime change: `Assets/Game/Combat/CombatMovement.cs` only. Regression changes: `Assets/Game/Editor/CombatPlayModeChecks.cs`. No scenes, prefabs, input assets, third-party code, camera controls, or attack balance were changed for this fix.

## Regression checks

The former jump regression checked only that the character left the ground during a short 0.8-second sample. It could not detect reactivation after landing. Four new cases now cover:

- One Space tap: one takeoff, then grounded with the request consumed after three seconds.
- Holding Space: no automatic jump after landing.
- Release and press again: two deliberate takeoffs followed by landing.
- Space during Heavy: no jump during commitment or delayed jump afterward.

The tests count upward crossings above the floor across complete jump/landing cycles rather than checking only peak height. Before the fix, the new single-tap check reproduced **three jumps in three seconds**. The ignored `Logs/JumpRegressionBefore.txt` preserves that failing run.

Run **Tools > EvolvingRPG > Run Combat Play Mode Checks** with Unity focused, scenes saved, and Play mode stopped. The suite now contains 26 checks and writes `Logs/CombatPlayModeChecks.txt`.

After the fix, Unity **6000.6.7f1** compiled successfully and all **26 checks passed in one run**, including all four new jump cases. No runtime Console errors were captured. The report and `Logs/ObservationValidation.log` contain local validation evidence; player confirmation of the corrected jumping behavior remains pending.

## Manual retest

Let script compilation finish in the existing Unity Editor. Open `Assets/Game/Scenes/CombatTest.unity`, enter Play mode, and click the Game view. Tap Space once and wait several seconds after landing; the character should remain grounded. Hold Space through landing, then release and press it again. Also try a Space press during Heavy: it should be discarded. No manual configuration is required.
