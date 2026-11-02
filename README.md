# Evolving RPG — Unity project starter

**Status:** Planning + first playable movement prototype. This folder is a documentation/repository scaffold, **not** a complete Unity project. Create the Unity project in Unity Hub first, then copy these files into its root (merge the `Assets` folder).

## Start here

1. Install **Unity Hub** and **Unity 6.3 LTS** with Windows Build Support if you intend to make Windows builds.
2. In Unity Hub choose **New Project → Universal 3D (URP)**. Create an empty project, e.g. `EvolvingRPG`.
3. Back up or commit that untouched project before adding packages.
4. Import Unity Technologies' free **First Person + Third Person | Character Controllers** package (compatible with Unity 6.3 LTS): https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526
5. If prompted, let Unity enable the **Input System**, restart the editor, and re-import the package if import was interrupted.
6. Open the package's **Playground** demo scene to verify movement/camera before editing it. Then create your own `Assets/Game/Scenes/MovementTest.unity` scene based on or using the provided prefabs.
7. Copy these starter docs and folders into the Unity project root.
8. Use `docs/MILESTONES.md` to complete movement/camera testing before combat.

## Design compass

> The game isn't about reaching the strongest version of your character. It's about discovering what your character can become in a world that's changing alongside you.

## First build: V0.1 Movement Test

**Build:** One character in a graybox test arena, camera-relative WASD, smooth turning, mouse-look, adjustable camera distance, basic collision, and movement around obstacles. A jump and basic dodge are optional until we have assessed the movement feel.

**Don't build yet:** precise weapon hitboxes, fully realized combat animations, enemy AI, inventory, procedural talents, multiplayer, hub, or missions.

## Version control

Commit `Assets/`, `Packages/`, `ProjectSettings/`, and their `.meta` files. Do **not** commit `Library/`, `Temp/`, `Obj/`, build directories, and other generated caches. Keep Unity and package versions recorded. Use Git LFS later for large binary assets such as textures, models, audio, and cinematics.

## Documents

- `docs/VISION.md`: enduring design philosophy and established gameplay direction.
- `docs/MILESTONES.md`: phased implementation plan, starting with acceptance criteria for V0.1.
- `AGENTS.md`: instructions for Codex or another coding agent working in the repository.
