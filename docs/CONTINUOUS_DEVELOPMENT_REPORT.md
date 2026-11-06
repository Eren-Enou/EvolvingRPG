# Continuous development — relay loop checkpoint, 2026-11-06

## Result and evidence

RelayOutpost now connects a compact hub, authored relay arena, completion/failure/abandon, return report, retry, deliberate Heavy specialization and explicit persistence. After the user renewed full computer-control authorization, the scene was generated through the explicit Editor command and **all fourteen Unity regression suites passed**. The Windows development build succeeded and its startup rendered the hub. No second Editor was started, existing comparison scenes were not overwritten, and gameplay feel has not been validated.

- Branch: `development/continuous-playable-rpg`, from `8baacc1`. Fetch/ancestry inspection found all named prior development branches consolidated as ancestors, with no divergent work requiring merge. Main unchanged, one worktree.
- Starting offline baseline: 10/10 cases, 9,082 assertions. Current offline check: **11/11 cases, 9,111 assertions**, linking six actual production files without Unity stubs. Includes mission identity/terminal gates, reward replay protection, schema-2 migration and equal-funded independent Breaker/Momentum Warriors. Assertion counts include repeated invariants, not independent gameplay scenarios.
- Both runtime and Editor sources compile using the installed Unity **6000.6.7f1** compiler and real assemblies from its cached compilation graph. `tools/CompileUnitySources.ps1` writes only ignored Logs/SourceCompile outputs; it does not launch Unity or inject assemblies. Existing obsolete-API warnings remain. This establishes C#/API compatibility against that graph, not graph freshness, Unity import, serialization, physics or gameplay.
- Source/document whitespace checks passed. Raw checks over the newly generated Unity assets report trailing spaces on Unity's serialized empty fields; those canonical Editor outputs were retained intact. Automated checks did not access personal saves. The current fourteen-stage native run began 2026-11-06 at 19:06:12 UTC; all stages passed, including runtime/teardown error capture. Reports: `Logs/ExtendedChecks.txt`, `Logs/ExpeditionChecks.txt`, `Logs/CharacterSaveChecks.txt`. Original DevelopmentTest scene was restored after the run. This is current evidence, separate from historical thirteen-stage results.

## Launch from the Editor

1. Use the existing Unity 6000.6.7f1 instance. Stop Play and save scene edits yourself. Refresh/import changed scripts and resolve any actual Console errors.
2. Choose **Tools > EvolvingRPG > Create Relay Outpost Loop**. This explicitly copies DevelopmentTest and authors only the new scene/materials through Editor APIs, then closes its additive authoring scene and restores the previous active scene. An existing RelayOutpost is retained, never overwritten. Nothing executes on import.
3. Open the generated `Assets/Game/Scenes/RelayOutpost.unity`, press Play and click Game view to capture controls. The creation command keeps this scene if it already exists.
4. Walk toward the gold mission board and press **F**. Accept **Restore relay**. Defeat both defenders, navigate around the central obstacle to the blue relay, and press **F** to complete and return.
5. Read the report. Choose **Inspect character / invest / explicit A-B save and load**. Preview and confirm Breaker or Momentum. Earning the point never invests automatically. Explicitly save to A/B and confirm an intended overwrite.
6. Tab closes the panel; click Game view to resume. Return to the board to deploy again. F at the arrival beacon abandons a mission. Defeat returns to the hub report with retry available.

No new Input Actions asset wiring is needed: F is a game-owned Input System action. Builder references, materials and labels are assigned through Unity APIs. No paid assets are required. If authoring fails partway, preserve the partial scene and inspect the error; the builder does not silently delete or overwrite it.

## Controls and tuning

WASD movement, Q/E facing-preserving strafe, Space jump, LMB Light, RMB Heavy, Left Shift dodge, mouse look and wheel zoom keep their existing implementation/tuning. Escape releases the cursor; a deliberate Game-view click captures it without attacking. F interacts; Tab inspects development near the hub. R reset is disabled only in RelayOutpost to preserve mission lifecycle. Old sandboxes retain R/debug controls.

`ExpeditionLoop` holds station/opponent references and an Inspector interaction radius. `OwnedCharacter` exposes initial fixture points, save folder and safe development radius. The new builder sets these to 0, RelayPrototype and 6m; old defaults remain 1, CharacterDevelopmentPrototype and 2m. Movement, attack, dodge and camera parameters remain on existing components/assets. Stamina costs/regeneration are unchanged.

Hub geometry is around x=55; the arena remains around x=0. Travel teleports the **same live Warrior** and notifies Cinemachine of the target warp. Decorative station cubes have no solid collider, so the arrival beacon cannot trap the capsule. This is one scene with explicit zone transitions, not asynchronous scene loading or a production instance service. Native tests verified deployment and return; ordinary route/camera behavior remains a manual acceptance item.

## Mission, reward and character rules

- Every deployment has a unique attempt bound to a character ID and stable contract ID. Accepted/in-progress/terminal/settled gates reject invalid sequencing, unknown roles, stale or duplicate callbacks and settlement for a different character.
- **Abandoned Relay:** defeat guarded and fast defenders, then interact near the relay. First restoration grants one point per Warrior. Repeat completions grant none.
- **Priority Interdiction:** defeat the fast fighter and reach the same relay; guarded fighter is optional. Same tuning, different objective pressure, no currency grant. Natural-route usefulness remains untested.
- Fresh loop Warriors start with Baseline, zero fixture points and no inherited character knowledge, experience or talents. The first earned point explicitly buys either Breaker or Momentum. One Heavy discipline/no respec remains an experimental restriction.
- Existing verified-evasion discovery and separate retaliation learning/activation are preserved. The once-only reveal fixture remains an additional point. No empty defensive talent or new owned modifier was added.
- Defeat/abandon grants no completion point. Completed combat experience/discovery remains in memory across returns, without automatic save. The menu suspends player input, not world simulation.
- The return report snapshots elapsed expedition time and newly demonstrated facts/qualifying exchanges before encounter reset changes session diagnostic text. This is presentation only: no bonus, automatic investment or additional reward rule.

## Persistence and recovery

Schema **3** adds initial fixture budget and first relay milestone to the same envelope as the wallet. Schema 1 migrates with empty historical facts, then schema 2 migrates with its original one fixture point and **no inferred mission completion**. Reads never rewrite. Explicit saves retain the previous supported record as backup. Unknown future schema/content/history and unexpected mission fields in legacy records remain protected. No history/wallet merging or automatic repair was introduced.

New loop A/B records live under `Application.persistentDataPath/RelayPrototype`; the panel displays the full path. Old sandbox files remain in CharacterDevelopmentPrototype. Native checks redirect saves into unique ignored Logs folders before any save/load.

Only deliberate hub saves persist. Mid-mission save/load/create/development is rejected. Attempts, commitments, temporary retaliation opportunities and encounter positions are never saved. Closing during a mission or loading an older record rolls back to the last explicit save, including unsaved rewards/investments. A saved milestone cannot pay again after reload. Recovering a pre-mission backup restores the complete prior character, not a previous wallet combined with a new milestone.

The mission-state restriction is explicit, not just a consequence of arena distance: an optional same-actor `ICharacterDevelopmentBoundary` blocks development/save/load/configuration while the attempt is active, even at hub coordinates. Old standalone scenes have no boundary and retain their existing safe-spawn checks. The native suite passed its controlled move back to hub coordinates during an active attempt with opponent-phase restrictions disabled, isolating this invariant from distance or enemy readiness.

## Native tests and build

From **one saved, stopped scene**, choose **Tools > EvolvingRPG > Run Extended Regression Checks**. Fourteen suites include preserved movement/combat/ownership/history/trade checks and the new relay suite. Expanded real JsonUtility/file replacement checks cover schema-1/2 migration, schema-3 milestone/wallet roundtrip, repeat loads and whole-record recovery. Reports are in ignored Logs; failure, skip, incomplete marker or Console errors fail acceptance.

**Run Relay Expedition Checks** runs only the new native suite. It explicitly creates/opens the scene and enters Play, then checks focused F input for board/relay, input suspension/restoration, deployment gates, native dodge and actual Heavy collision/impact against controlled stationary defenders. Later failure/repeat/priority cases use clearly labeled terminal-hit fixtures; those cases do not establish attack reach, AI difficulty or a naturally playable route. It checks equal-funded A/B Warriors, no attempt/action inheritance, hub fall recovery, and runtime/teardown errors. On success it restores the previous scene. All execution is explicit; nothing is imported and run automatically.

To rebuild, choose **Build Relay Windows Development Player**. This uses BuildPipeline with explicit RelayOutpost scene options and a new timestamped ignored Builds directory, writes `Logs/ExpeditionBuild.txt`, and does not modify build-scene settings, automatically launch a player or install modules.

**Actual build:** started 2026-11-06 19:08:08 UTC, succeeded in 00:02:11.510 with 0 errors, 4 existing obsolete-API warnings, 226,510,604 total bytes. Executable: `Builds/Relay-20261106-190808-388/EvolvingRPG.exe`. A separate explicit startup check launched this player, observed the hub HUD/Warrior/station visuals, then closed it normally. `Logs/RelayPlayerStartup.log` retains the startup/exit log. URP logs unused stripped post-processing shaders at informational level; installed package source confirms those passes call LoadShader with LogType.Log. No gameplay exceptions were found. This was a startup smoke, not a full player-route test.

Unity automatically serialized graphics/URP build-filter metadata, input preloaded-resource metadata and a UnityConnect setting during the build. The tree was clean before these operations, their exact diff was retained in ignored `Logs/RelayBuildAutomaticSettings.patch`, and only those known automatic settings changes were restored afterward. No package, shared attack asset, third-party source or preexisting scene change is part of this checkpoint.

Current native result: all fourteen stages passed. Relay checked focused F interactions, menu ownership, actual Heavy collision/impact against stationary controlled defenders, proximity-gated completion, once-only funding and explicit Momentum investment. Separately labeled terminal-hit fixtures verified repeat/priority policy, defeat/abandon/retry, an equally funded independent Breaker character, A/B restoration, hub fall recovery and teardown. JsonUtility/File.Replace tests passed schema-3 milestone/wallet consistency, schema-1/2 migration, non-writing reads, protected unknown records and complete-record backup recovery. Controlled integration results do not establish natural encounter difficulty.

Manual checks still required: normal full hub/mission/return cycle; movement/strafe/jump/dodge/camera after every menu/transition; target/objective readability; camera warp/collision; defeat/abandon/retry; save, stop/restart Play, explicit reload; repeat restoration with no extra point; equal-funded Breaker/Momentum comparison. Feel, fairness and the second contract's decision value require player feedback.

Ordinary Editor Play rendered the initial hub HUD and station visuals; Editor was left stopped on RelayOutpost. Automation-generated Tab presses did not visibly open the menu in the brief Editor/player observations; the cause is not established. The focused native Input System Tab/F checks passed. Confirm physical Tab/F during player testing before treating ordinary keyboard/menu acceptance as established; do not infer a gameplay defect or change the controller solely from injected OS-key behavior. The startup smoke did not save or load any personal character records.

On the next inspection, Windows Security displayed a Firewall permission prompt for evolvingrpg.exe over the stopped Editor. It was left untouched; security permission dialogs require the user's manual action under the computer-use skill. This is a separate observed obstruction to further UI testing, not proof of the cause of the earlier input observations. No firewall rules or Windows security settings were changed. Dismiss the prompt manually before resuming ordinary UI/input checks; the prototype implements no multiplayer.

**Follow-up after manual dismissal:** the prompt is gone and UI verification resumed. A bounded, explicit, read-only `RelayInputObservation` command observed the running hub while OS-automated Tab/F presses were attempted. `Logs/RelayInputObservation.txt` records focus=True, safe=True, input=True, inspect/interact enabled with one resolved control each, and enabled keyboard interface RawInput. It received **zero keyboard state events and zero Tab/F action callbacks** during those attempts. This isolates that observation to absent delivery into Unity's Input System; it does not establish a controller rejection or validate physical keyboard delivery. No gameplay input handling was changed. Physical controls and the natural route still need player acceptance. The observer records only Tab/F presence and aggregate focus/menu/device state, never other key values, saves, input injection or device mutations; it stops after 30 seconds or Play exit. Run it through Tools > EvolvingRPG only when needed; it has no global shortcut.

`tools/CompileUnitySources.ps1` now selects a cached compilation graph with a matching Editor assembly response file, so the newer Windows-player build graph is not accidentally selected after a build. Reproduced the prior selection failure, applied the filter, and runtime/Editor source compilation passed afterward. Unity imported the observer and its explicit native observation completed without captured exceptions. These follow-up changes are Editor/tooling only; the previously verified runtime build remains the gameplay baseline.

## File changes

- New `Assets/Game/Expeditions/ExpeditionAttempt.cs`, `ExpeditionLoop.cs`, `ExpeditionSign.cs` and metadata: lifecycle/settlement, runtime interactions/return/report/camera warp, placeholder labels.
- New `Assets/Game/Editor/ExpeditionSceneBuilder.cs`, `ExpeditionChecks.cs`, `ExpeditionBuild.cs` and metadata: explicit authoring, native checks and Windows build command.
- Generated `Assets/Game/Scenes/RelayOutpost.unity` and metadata, plus ARRIVAL/DEVELOPMENT/MISSION_BOARD/RELAY/SAVE materials and metadata under `Assets/Game/Expeditions`, authored by Unity through the explicit builder.
- `Editor/ExtendedChecks.cs`: relay stage; `Editor/CharacterSaveChecks.cs`: migration, milestone/wallet, backup tests and legacy fixtures.
- `Combat/EnemySandboxHUD.cs`: opt-out R reset switch, enabled by default for old scenes.
- `Progression/CharacterDevelopmentState.cs`, `CharacterSaveData.cs`, `CharacterSaveStore.cs`: initial budget, one-time milestone, schema 3 and protected migration.
- `Progression/OwnedCharacter.cs`: configurable fixture/folder/safe radius and accurate messages; `DevelopmentPanel.cs`: loop/menu coexistence and funding/reset wording.
- `tools/CharacterStateChecks/Program.cs`, `CharacterStateChecks.csproj`, `README.md`: mission linkage, model/economy tests and documentation.
- New `tools/CompileUnitySources.ps1`: isolated cached-reference compilation.
- `AGENTS.md`, `docs/MILESTONES.md`, `CONTINUOUS_DEVELOPMENT_STATE.md` and this report: scope/checkpoint/pending gates.

## Next dependencies and open decisions

Confirm ordinary physical controls and the full natural route in player testing. Test whether priority targeting changes exposure, route or target order using the existing geometry. Compare equally funded Warriors on approach, missed attacks and recovery decisions. Add another application of an existing discipline only when player observations justify it; technical success alone does not authorize more owned modifiers.

Milestone/reveal funding, specialization restrictions, failure retention and save boundaries are provisional design policies. Equipment, new enemies, account mastery, talent breadth, multiplayer and enemy persistence remain deferred. The active goal is not complete.
