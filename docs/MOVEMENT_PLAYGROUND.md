# V0.1 movement playground

V0.1 WASD, jumping, and mouse camera controls were subsequently player-tested successfully. V0.1.1 adds dedicated **Q/E strafing**; see [STRAFING_CONTROLS.md](STRAFING_CONTROLS.md) for behavior, Inspector tuning, validation, and the exact new file list.

## Launch in Unity

The Unity project root is `EvolvingRPG/` inside this repository (the folder containing `Assets`, `Packages`, and `ProjectSettings`). The inspected project uses **Unity 6000.6.7f1 / URP**, rather than the 6.3 LTS version named in the vision. No version or package changes were made for this implementation.

1. Use the already-open Editor. Allow script compilation to finish.
2. In the Project panel, open `Assets/Game/Scenes/MovementTest.unity` by double-clicking it. Save your current scene first if you have unsaved work.
3. Open the Console, then press **Play** and click inside the Game view.
4. **WASD** moves relative to the camera; mouse movement orbits the camera. **Mouse wheel** changes distance, **Left Shift** sprints, and **Space** jumps using Starter Assets bindings.
5. **Escape** releases the cursor and suppresses movement. Click the Game view to capture it again. Switching away from Unity also releases the cursor; click to resume after returning.
6. Stop Play mode to return to editing.

No scene wiring, package installation, layer changes, or Build Profile changes are required. If the scene is missing on another checkout, use **Assets > Refresh**, then **Tools > EvolvingRPG > Create Movement Playground** after compilation. The builder also creates the scene on its first import if it does not exist. It saves only this new scene through Unity's scene/prefab APIs, restores the previously active scene, and closes the generated scene. It refuses to overwrite an existing MovementTest and waits until outside Play mode. Keep the generated scene and all `.meta` files in version control.

## What is implemented

- Imported `PlayerArmature`, `MainCamera`, and `PlayerFollowCamera` prefab instances. Vendor controller code, animations, input actions, gravity, jump, collision, camera-relative movement, and smooth facing are reused unchanged.
- Scene overrides set movement to 4 m/s, sprint to 6 m/s, rotation smooth time to 0.08 s, and acceleration/deceleration rate to 18. These are starting values for player testing.
- `PlaygroundMouseLook` supplies mouse delta scaled by sensitivity to `StarterAssetsInputs` before the vendor controller runs. The prefab's automatic look input is disabled on this instance to avoid double input. Mouse delta is not multiplied by frame time. Keyboard movement remains supplied by the prefab's PlayerInput actions.
- `ThirdPersonCameraZoom` smoothly changes the requested Cinemachine distance between 1.25 and 7 metres. Cinemachine independently pulls the camera inward around geometry and restores distance afterward. The imported camera's procedural noise is disabled in this scene.
- Cinemachine Third Person Follow obstacle avoidance uses Default-layer geometry, ignores the Player tag, has a 0.25 m camera radius, immediate collision correction, and a 0.2 s return damping. Near clipping is 0.1 m and field of view is 55 degrees. Character grounding uses Default-layer geometry; the imported character is on layer 8 with the Player tag.
- A bounded 40 x 40 m graybox provides open movement space, two large blocks, a low jump block, a 2.5 m wide corridor with a covered section, and a stationary capsule for circling. The capsule has no gameplay behavior.

## Tuning in the Inspector

| Scene object / component | Parameters |
| --- | --- |
| Player / Third Person Controller | Move Speed, Sprint Speed, Rotation Smooth Time (lower turns faster), Speed Change Rate, Jump Height, Gravity, Top/Bottom Clamp |
| Player / Playground Mouse Look | Sensitivity (degrees per mouse pixel; default 0.12), Invert Y |
| PlayerFollowCamera / Third Person Camera Zoom | Distance, Minimum/Maximum Distance, Zoom Per Notch (per 120 Windows scroll units), Smooth Time |
| PlayerFollowCamera / Cinemachine Third Person Follow | Shoulder Offset, Damping, obstacle collision filter/radius/return damping |
| PlayerFollowCamera / Cinemachine Camera | Field of View, Near Clip Plane |

Change values outside Play mode to persist them. Play-mode changes reset on exit unless copied back manually. Treat the zoom component's Distance as the desired distance; it owns Cinemachine's Camera Distance while playing. New solid graybox geometry should have a non-trigger collider and use the Default layer so grounding and camera avoidance both see it.

## Validation and player acceptance test

The existing Unity 6000.6.7f1 Editor compiled both the runtime and Editor assemblies successfully, reloaded them, instantiated the vendor prefabs, and saved the scene. Evidence is in the local ignored `EvolvingRPG/Logs/Editor.log` and `EvolvingRPG/Logs/MovementPlaygroundCreation.txt`. No second Editor was launched. No Unity Play-mode tests or movement-feel validation were performed.

The Editor log includes a package warning about Cinemachine's `HDRP-Editor-ref.asmref` having no target assembly definition. It did not block compilation; this URP prototype does not change the package.

Player testing remains required:

1. Optionally open the imported `Assets/Starter Assets/Sample/ThirdPersonController/Playground.unity` and confirm the vendor demo works with keyboard/mouse; then return to MovementTest.
2. Move around both large obstacles, turn the mouse while moving, and rapidly reverse W/S and A/D. Check that facing follows movement and diagonals do not move faster.
3. Circle the inert capsule with lateral/diagonal movement. Assess the turning behavior; this version faces movement direction and does not implement combat strafing or target lock.
4. Scroll repeatedly in both directions. Confirm distance clamps, then orbit at maximum and minimum zoom beside a block and each boundary wall.
5. Enter the corridor and covered section. Walk backward toward a wall while orbiting; confirm the camera pulls inward, avoids clipping, and returns smoothly after clearing the obstacle. Try steep up/down viewing angles.
6. Walk directly into obstacles and jump onto/off the low block. Confirm collision, grounding, and landing animations.
7. Press Escape while moving, release the movement key, click to resume, and test switching away from Unity and back. Confirm the cursor and movement recover normally.
8. Stop and restart Play mode. Check the Console for errors throughout and report awkwardness with quick turns, circling, close zoom, corridor occlusion, or acceleration.

## Exact files added

Repository-relative paths (the repeated `EvolvingRPG` folder is the Unity project):

- `EvolvingRPG/Assets/Game.meta`
- `EvolvingRPG/Assets/Game/Editor.meta`
- `EvolvingRPG/Assets/Game/Editor/MovementPlaygroundBuilder.cs`
- `EvolvingRPG/Assets/Game/Editor/MovementPlaygroundBuilder.cs.meta`
- `EvolvingRPG/Assets/Game/Movement.meta`
- `EvolvingRPG/Assets/Game/Movement/PlaygroundMouseLook.cs`
- `EvolvingRPG/Assets/Game/Movement/PlaygroundMouseLook.cs.meta`
- `EvolvingRPG/Assets/Game/Movement/ThirdPersonCameraZoom.cs`
- `EvolvingRPG/Assets/Game/Movement/ThirdPersonCameraZoom.cs.meta`
- `EvolvingRPG/Assets/Game/Scenes.meta`
- `EvolvingRPG/Assets/Game/Scenes/MovementTest.unity`
- `EvolvingRPG/Assets/Game/Scenes/MovementTest.unity.meta`
- `docs/MOVEMENT_PLAYGROUND.md`

Existing scenes, third-party assets, Packages, ProjectSettings, and pre-existing working-tree changes were preserved. No combat, enemies, progression, menus, or paid dependencies were added.

## Unresolved gameplay questions

- Should ordinary repositioning remain at 4 m/s, and should sprint/jump remain part of the baseline controls?
- How fast should direction reversal and facing feel before combat introduces committed actions?
- Which sensitivity, shoulder offset, zoom range, and corridor camera recovery feel comfortable in player testing?
- Will future combat use movement-facing, camera-facing strafing, or an optional target lock? This prototype makes no permanent combat decision.

V0.1 acceptance remains pending interactive player testing; milestone gameplay checkboxes have not been marked complete.
