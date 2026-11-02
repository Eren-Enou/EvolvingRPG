# V0.1.1 dedicated strafing

## Play and tune

Open `Assets/Game/Scenes/MovementTest.unity` in the existing Unity Editor, press **Play**, and click the Game view.

- **Q / E:** move left/right relative to the character while holding their current facing.
- **Q/E + W/S:** move diagonally forward/back relative to that held facing. Diagonals share the same maximum speed as straight strafing.
- **Q + E:** cancel the additional lateral input. Facing remains held while either key is down; W/S still works when both are held. Stopping follows the configured deceleration.
- **A/D while Q/E is held:** add character-relative lateral input, clamped together with the other keys. Releasing Q/E returns the original camera-relative, turn-to-move WASD behavior.
- **Space, Shift, mouse-look, wheel zoom, and Escape/click cursor controls:** keep their existing functions. Sprint uses the existing sprint/move speed ratio, so diagonals do not add a sprint bonus.

On **Player > Dedicated Strafe**, adjust **Strafe Speed** (default 4 m/s) and **Responsiveness** (default 18; higher accelerates/decelerates faster). The two Input System actions expose Q/E bindings in the same Inspector. Tune outside Play mode to save changes. Keep the required **Strafe Controller Restore** companion enabled.

No manual wiring is needed in the updated scene. For a separate existing player, add **Dedicated Strafe** to the object with Starter Assets' **Third Person Controller** and **Starter Assets Inputs**; Unity adds the restoration companion automatically. **Tools > EvolvingRPG > Add Q-E Strafing to Movement Playground** upgrades an older MovementTest without recreating its environment. It preserves existing components and does not automatically save a scene that already had unsaved edits.

## Implementation boundaries

A/D originally turned toward camera-relative movement; they were not facing-preserving strafe controls. WASD-only execution is still handled entirely by the unchanged vendor controller.

The game-owned adapter runs before Starter Assets, converts normalized character-relative strafe input into the camera-relative input its motor expects, and temporarily supplies the strafe speed/response. The existing motor performs the only movement/collision integration and continues handling gravity, jumping, grounding, and animation.

Starter Assets has no public movement/rotation extension hook. A small companion therefore restores the held facing, original input, and original tuning immediately after its Update, before Animator and camera evaluation. The adapter never writes camera rotation, look input, or zoom. Its LateUpdate/OnDisable also restore any pending state. Vendor turn smoothing is temporarily set to zero while strafing so it does not accumulate turn momentum for release.

The update order is Mouse Look (-100), Dedicated Strafe (-50), Starter Assets (0), restoration (50), then animation/camera evaluation. Future combat components that inspect final character facing during Update should run after order 50. If movement expands significantly, introduce an explicit movement/facing policy at a game-owned motor boundary rather than stacking more adapters onto this vendor implementation.

Animations reuse the existing forward locomotion blend tree; dedicated side-step/backpedal clips are not included in this controls milestone.

## Tests

Unity **6000.6.7f1** compiled the runtime and Editor changes successfully. All **13 automated Play-mode integration checks passed** in the already-running Editor: Q, E, Q+W, E+W, Q+S, E+S, Q+E cancellation, Q+E+W, ordinary W/A/D turning, Q+Space jumping, and mouse-look/wheel zoom. The checks compared facing and camera rotation, direction of travel, and cardinal/diagonal speeds with the real Starter Assets motor. No runtime Console errors were captured. Unity returned to Edit mode afterward.

Existing package warnings were not changed. These checks verify behavior, not subjective movement feel; interactive acceptance remains yours.

To rerun the automated checks, open only MovementTest, exit Play mode, and use **Tools > EvolvingRPG > Run Q-E Strafing Play Mode Checks**. Keep Unity focused while the bounded checks run. They enter Play mode in the existing Editor, pair temporary virtual keyboard/mouse devices, create an isolated runtime floor, and return to Edit mode. Runtime positions and objects are not saved. The local ignored report is `Logs/StrafingPlayModeChecks.txt`.

For player acceptance:

1. Face a recognizable obstacle, then hold Q or E; verify sideways movement without turning. Orbit the camera independently and repeat.
2. Try Q+W, E+W, Q+S, E+S, and rapid Q/E reversals. Compare straight/diagonal speed and feel.
3. Hold Q+E, then add/release W. Verify no extra lateral input and predictable stopping.
4. Release Q/E while holding W/A/D; verify original turn-to-move behavior returns. Test Space and sprint with and without strafing.
5. Strafe beside walls and through the corridor while looking/zooming. Check collision, camera avoidance, cursor release/reacquisition, and the Console.

## Exact file changes

All paths below are relative to the Unity project root:

- Added `Assets/Game/Movement/DedicatedStrafe.cs` and its `.meta`.
- Added `Assets/Game/Movement/StrafeControllerRestore.cs` and its `.meta`.
- Added `Assets/Game/Editor/StrafingPlayModeChecks.cs` and its `.meta`.
- Updated `Assets/Game/Editor/MovementPlaygroundBuilder.cs` to include strafing in newly generated scenes and provide a non-destructive upgrade command.
- Updated `Assets/Game/Scenes/MovementTest.unity` through Unity Editor to attach the components and serialize their Q/E actions and tuning. Unity also persisted its camera preview transforms/lens values and URP additional light data on save.
- Added `docs/STRAFING_CONTROLS.md`; updated `docs/MOVEMENT_PLAYGROUND.md` to link the new controls.

Imported Starter Assets, existing input-action assets, camera scripts, Packages, and ProjectSettings are unchanged.

## Open questions for playtesting

- Should strafing stay at ordinary movement speed, or be slower during combat?
- Does the facing-relative W/S behavior while Q/E is held feel natural when the camera looks elsewhere?
- When combat arrives, should facing lock become an explicit mode, and should dedicated strafe animations be added?
