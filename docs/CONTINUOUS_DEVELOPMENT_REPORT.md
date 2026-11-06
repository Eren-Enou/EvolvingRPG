# Continuous development — relay loop checkpoint, 2026-11-06

## Result and evidence

Source now connects a compact hub, authored relay arena, completion/failure/abandon, return report, retry, deliberate Heavy specialization and explicit persistence. This is an implementation checkpoint: **Unity scene generation, native integration and a Windows build remain pending.** Computer control was stopped with physical Escape; subsequent work used source/CLI only. No second Editor was started, existing scenes were not overwritten, and gameplay feel has not been validated.

- Branch: `development/continuous-playable-rpg`, from `8baacc1`. Fetch/ancestry inspection found all named prior development branches consolidated as ancestors, with no divergent work requiring merge. Main unchanged, one worktree.
- Starting offline baseline: 10/10 cases, 9,082 assertions. Current offline check: **11/11 cases, 9,111 assertions**, linking six actual production files without Unity stubs. Includes mission identity/terminal gates, reward replay protection, schema-2 migration and equal-funded independent Breaker/Momentum Warriors. Assertion counts include repeated invariants, not independent gameplay scenarios.
- Both runtime and Editor sources compile using the installed Unity **6000.6.7f1** compiler and real assemblies from its cached compilation graph. `tools/CompileUnitySources.ps1` writes only ignored Logs/SourceCompile outputs; it does not launch Unity or inject assemblies. Existing obsolete-API warnings remain. This establishes C#/API compatibility against that graph, not graph freshness, Unity import, serialization, physics or gameplay.
- `git diff --check` passed. Automated checks did not access personal saves. Historical thirteen-stage native successes belong to the earlier baseline; they do not verify these changes. The prepared Extended Checks now has fourteen stages and has not run since these changes.

## Launch from the Editor

1. Use the existing Unity 6000.6.7f1 instance. Stop Play and save scene edits yourself. Refresh/import changed scripts and resolve any actual Console errors.
2. Choose **Tools > EvolvingRPG > Create Relay Outpost Loop**. This explicitly copies DevelopmentTest and authors only the new scene/materials through Editor APIs, then closes its additive authoring scene and restores the previous active scene. An existing RelayOutpost is retained, never overwritten. Nothing executes on import.
3. Open `Assets/Game/Scenes/RelayOutpost.unity`, press Play and click Game view to capture controls. This asset does not exist until the builder runs.
4. Walk toward the gold mission board and press **F**. Accept **Restore relay**. Defeat both defenders, navigate around the central obstacle to the blue relay, and press **F** to complete and return.
5. Read the report. Choose **Inspect character / invest / explicit A-B save and load**. Preview and confirm Breaker or Momentum. Earning the point never invests automatically. Explicitly save to A/B and confirm an intended overwrite.
6. Tab closes the panel; click Game view to resume. Return to the board to deploy again. F at the arrival beacon abandons a mission. Defeat returns to the hub report with retry available.

No new Input Actions asset wiring is needed: F is a game-owned Input System action. Builder references, materials and labels are assigned through Unity APIs. No paid assets are required. If authoring fails partway, preserve the partial scene and inspect the error; the builder does not silently delete or overwrite it.

## Controls and tuning

WASD movement, Q/E facing-preserving strafe, Space jump, LMB Light, RMB Heavy, Left Shift dodge, mouse look and wheel zoom keep their existing implementation/tuning. Escape releases the cursor; a deliberate Game-view click captures it without attacking. F interacts; Tab inspects development near the hub. R reset is disabled only in RelayOutpost to preserve mission lifecycle. Old sandboxes retain R/debug controls.

`ExpeditionLoop` holds station/opponent references and an Inspector interaction radius. `OwnedCharacter` exposes initial fixture points, save folder and safe development radius. The new builder sets these to 0, RelayPrototype and 6m; old defaults remain 1, CharacterDevelopmentPrototype and 2m. Movement, attack, dodge and camera parameters remain on existing components/assets. Stamina costs/regeneration are unchanged.

Hub geometry is around x=55; the arena remains around x=0. Travel teleports the **same live Warrior** and notifies Cinemachine of the target warp. Decorative station cubes have no solid collider, so the arrival beacon cannot trap the capsule. This is one scene with explicit zone transitions, not asynchronous scene loading or a production instance service. Ordinary route/camera behavior is unverified.

## Mission, reward and character rules

- Every deployment has a unique attempt bound to a character ID and stable contract ID. Accepted/in-progress/terminal/settled gates reject invalid sequencing, unknown roles, stale or duplicate callbacks and settlement for a different character.
- **Abandoned Relay:** defeat guarded and fast defenders, then interact near the relay. First restoration grants one point per Warrior. Repeat completions grant none.
- **Priority Interdiction:** defeat the fast fighter and reach the same relay; guarded fighter is optional. Same tuning, different objective pressure, no currency grant. Natural-route usefulness remains untested.
- Fresh loop Warriors start with Baseline, zero fixture points and no inherited character knowledge, experience or talents. The first earned point explicitly buys either Breaker or Momentum. One Heavy discipline/no respec remains an experimental restriction.
- Existing verified-evasion discovery and separate retaliation learning/activation are preserved. The once-only reveal fixture remains an additional point. No empty defensive talent or new owned modifier was added.
- Defeat/abandon grants no completion point. Completed combat experience/discovery remains in memory across returns, without automatic save. The menu suspends player input, not world simulation.

## Persistence and recovery

Schema **3** adds initial fixture budget and first relay milestone to the same envelope as the wallet. Schema 1 migrates with empty historical facts, then schema 2 migrates with its original one fixture point and **no inferred mission completion**. Reads never rewrite. Explicit saves retain the previous supported record as backup. Unknown future schema/content/history and unexpected mission fields in legacy records remain protected. No history/wallet merging or automatic repair was introduced.

New loop A/B records live under `Application.persistentDataPath/RelayPrototype`; the panel displays the full path. Old sandbox files remain in CharacterDevelopmentPrototype. Native checks redirect saves into unique ignored Logs folders before any save/load.

Only deliberate hub saves persist. Mid-mission save/load/create/development is rejected. Attempts, commitments, temporary retaliation opportunities and encounter positions are never saved. Closing during a mission or loading an older record rolls back to the last explicit save, including unsaved rewards/investments. A saved milestone cannot pay again after reload. Recovering a pre-mission backup restores the complete prior character, not a previous wallet combined with a new milestone.

## Prepared tests and build — execution pending

From **one saved, stopped scene**, choose **Tools > EvolvingRPG > Run Extended Regression Checks**. Fourteen suites include preserved movement/combat/ownership/history/trade checks and the new relay suite. Expanded real JsonUtility/file replacement checks cover schema-1/2 migration, schema-3 milestone/wallet roundtrip, repeat loads and whole-record recovery. Reports are in ignored Logs; failure, skip, incomplete marker or Console errors fail acceptance.

**Run Relay Expedition Checks** runs only the new native suite. It explicitly creates/opens the scene and enters Play, then checks focused F input for board/relay, input suspension/restoration, deployment gates, native dodge and actual Heavy collision/impact against controlled stationary defenders. Later failure/repeat/priority cases use clearly labeled terminal-hit fixtures; those cases do not establish attack reach, AI difficulty or a naturally playable route. It checks equal-funded A/B Warriors, no attempt/action inheritance, hub fall recovery, and runtime/teardown errors. On success it restores the previous scene. All execution is explicit; nothing is imported and run automatically.

After native checks pass, choose **Build Relay Windows Development Player**. This uses BuildPipeline with explicit RelayOutpost scene options and a new timestamped ignored Builds directory, writes `Logs/ExpeditionBuild.txt`, and does not modify build-scene settings, launch a player or install modules. Build result remains pending.

Manual checks still required: normal full hub/mission/return cycle; movement/strafe/jump/dodge/camera after every menu/transition; target/objective readability; camera warp/collision; defeat/abandon/retry; save, stop/restart Play, explicit reload; repeat restoration with no extra point; equal-funded Breaker/Momentum comparison. Feel, fairness and the second contract's decision value require player feedback.

## File changes

- New `Assets/Game/Expeditions/ExpeditionAttempt.cs`, `ExpeditionLoop.cs`, `ExpeditionSign.cs` and metadata: lifecycle/settlement, runtime interactions/return/report/camera warp, placeholder labels.
- New `Assets/Game/Editor/ExpeditionSceneBuilder.cs`, `ExpeditionChecks.cs`, `ExpeditionBuild.cs` and metadata: explicit authoring, native checks and Windows build command.
- `Editor/ExtendedChecks.cs`: relay stage; `Editor/CharacterSaveChecks.cs`: migration, milestone/wallet, backup tests and legacy fixtures.
- `Combat/EnemySandboxHUD.cs`: opt-out R reset switch, enabled by default for old scenes.
- `Progression/CharacterDevelopmentState.cs`, `CharacterSaveData.cs`, `CharacterSaveStore.cs`: initial budget, one-time milestone, schema 3 and protected migration.
- `Progression/OwnedCharacter.cs`: configurable fixture/folder/safe radius and accurate messages; `DevelopmentPanel.cs`: loop/menu coexistence and funding/reset wording.
- `tools/CharacterStateChecks/Program.cs`, `CharacterStateChecks.csproj`, `README.md`: mission linkage, model/economy tests and documentation.
- New `tools/CompileUnitySources.ps1`: isolated cached-reference compilation.
- `AGENTS.md`, `docs/MILESTONES.md`, `CONTINUOUS_DEVELOPMENT_STATE.md` and this report: scope/checkpoint/pending gates.

## Next dependencies and open decisions

Native verification/repair and actual scene authoring come next, then the development build. Test whether priority targeting changes exposure, route or target order using the existing geometry. Compare equally funded Warriors on approach, missed attacks and recovery decisions. Add another application of an existing discipline only when player observations justify it; technical success alone does not authorize more owned modifiers.

Milestone/reveal funding, specialization restrictions, failure retention and save boundaries are provisional design policies. Equipment, new enemies, account mastery, talent breadth, multiplayer and enemy persistence remain deferred. The active goal is not complete.
