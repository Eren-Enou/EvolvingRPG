# Continued development report — 2026-11-05

## Current continuation - observable combat trades (2026-11-06)

Started clean at `90cd558` on development/verified-character-depth; continuing on development/combat-trade-lab. Existing Unity 6000.6.7f1 was stopped on saved DevelopmentTest. No second Editor or scene/prefab/attack-asset edit was used.

First implemented slice: incoming applied damage/poise during an accepted attack is now observable in Recent attacks, alongside independent outgoing totals. The first incoming phase and player stagger/defeat are retained. CombatActions publishes ImpactReceived before synchronous interruption, so subscriber order cannot omit the stagger-causing hit. Existing costs, timing, damage, dodge and experience eligibility are unchanged. No received event is counted as an outgoing hit or experience candidate. Session fields are never saved and do not infer whether a trade was tactically worthwhile.

Updated AttackSnapshotChecks exercises actual ReceiveHit through production handlers: completed non-staggering trade, active/recovery exposure, receipt-before-interruption ordering, clamped lethal loss, immutable completed review, and post-completion/dodge/zero-impact exclusions. The full twelve-stage regression run started 2026-11-06T16:50:18Z and passed all stages with no failed/skipped suite. New checks executed successfully; no Unity compiler errors observed. Existing deprecated lookup warnings remain. This is a technical result, not physical-player or balance validation.

Changed: Assets/Game/Combat/CombatObservation.cs, CombatActions.cs and CombatActionReview.cs; Assets/Game/Editor/AttackSnapshotChecks.cs; docs/COMBAT_OBSERVATIONS.md, CHARACTER_DEVELOPMENT_PLAYGROUND.md and this report. Schema remains 2; saved character fields are unchanged. The phase field is optional for older synthetic observation constructors, defaulting to Ready; actual CombatActions observations supply the current phase.

Next work in this continuation: a controlled native comparison of the same guarded exchange with fresh versus nearly broken player poise, using runtime-only fixtures. This will establish the instrumented distinction between landed/completed and landed/interrupted without granting a new talent or deciding the experience policy. Final fixture and integrated results will be recorded below.

---

## Current follow-up - verified integration and inspectable experience (2026-11-06)

Started clean at `b1485d9`; branch `development/verified-character-depth`. User authorization supersedes the earlier offline-only restriction. No Unity process was running, so the installed, project-configured Unity 6000.6.7f1 was launched for this project. No second instance, batch mode, unsaved scene overwrite or personal-save access was used.

### First checkpoint

- Unity imported and compiled the previous pure-type extraction and identity-bound preview changes. The initial twelve-suite run, started `2026-11-06T09:29:51Z`, passed every stage. This includes native input, camera/strafe/jump, combat, technique, C1/C2/C3, ownership, Unity JsonUtility/file-store checks, panel lifecycle and Heavy experience. Preserved reports: ignored `Logs/ContinuousDevelopment/Baseline/`. Offline production-state checks also passed 10/10 cases, 9,082 assertions.
- Using the computer-use skill, the actual panel's Breaker preview was opened, canceled without spending, reopened and confirmed: one point spent, Breaker selected, Momentum unavailable. Fresh confirmation then produced a new identity, one point and Baseline. This disposable Play-mode inspection wrote no personal slot. Native fixture save/load coverage uses ignored Logs directories. UI load-latch behavior was not inferred from that initial button check; the final isolated load inspection is recorded below.
- Identified a readability gap: R overwrote `HeavyExperienceCollector.Latest` before the instructed R/Tab review. Implemented six bounded, sequence-linked Heavy result explanations beside recent attacks. New/duplicate/rejected/interrupted outcomes remain inspectable after reset. Fresh/load clear session explanations; no persistence, credit policy, points, modifiers or stamina change.
- Extended HeavyExperienceChecks verifies duplicate completion, interruption, bounded eviction, reset retention and replacement clearing, plus real per-attack explanations across all eight existing native cases. The updated isolated suite passed all four model groups and eight native cases with no captured runtime Console errors. Tests are authoritative production collectors and real native actions, not a copied simulation.

Changed so far: Assets/Game/Progression/HeavyExperienceCollector.cs, DevelopmentPanel.cs; Assets/Game/Editor/HeavyExperienceChecks.cs; AGENTS.md; docs/HEAVY_EXPERIENCE_HISTORY.md, CHARACTER_DEVELOPMENT_PLAYGROUND.md, MILESTONES.md and this report. No scene, prefab, attack asset, vendor, package or ProjectSettings change. Schema remains 2; new review strings are transient only.

At the first checkpoint, visual inspection and the final integrated regression were still pending; results follow below. Compilation emitted existing obsolete-API warnings (FindFirstObjectByType); no new compile errors were observed. Technical checks and agent UI inspection are not human gameplay-feel or balance validation. No standalone build or power-loss test performed.

### Second checkpoint and integrated results

Technique cards now read assigned base attack definitions instead of duplicating damage/poise/timing/advance numbers. They show impact, stamina, windup/active/recovery, miss recovery, reach/arc, advance and directional movement. The revealed return card reads its component's travel/recovery values. Numbers describe authored base behavior, not predicted guard resolution or contextual form. Inspection takes an immutable AttackSnapshot without running resolvers or acceptance.

Additional files: Assets/Game/Progression/OwnedCharacter.cs (`TryPreviewHeavy`), Assets/Game/Editor/DevelopmentChecks.cs (production preview tests), DevelopmentPanel.cs (cards), docs/CHARACTER_IDENTITY_PATH.md (current preparation), and the playground documentation. No attack definition was changed. The test mutates only an unsaved runtime clone, restores owner references in finally, and destroys the clone. It verifies current tuning, immutable earlier preview values, unknown/effect rejection and unchanged character DTO, stamina, selected attack, observation count and opportunity-consumption count. It runs during the live revealed-but-unlearned development fixture; existing C3 tests separately cover armed opportunity previews and acceptance.

The final integrated run started `2026-11-06T09:38:38Z` and **passed all twelve stages**, including the added native/model checks, with no failed/skipped stage. Each suite's runtime capture reported no errors. Reports were preserved under ignored `Logs/ContinuousDevelopment/FinalIntegrated/`; current summaries remain Logs/ExtendedChecks.txt, DevelopmentChecks.txt and HeavyExperienceChecks.txt. The Editor log is Logs/ContinuousDevelopmentUnity.log. No compiler errors were found; existing CS0618 deprecated Unity lookup warnings remain. All added automated tests were executed. No claim is made that historical Console contents are empty, or that warnings are resolved.

The Heavy-miss inspection fixture displayed its matching rejection explanation beneath the completed attack. Two brief OS-injected Tab presses did not close that inspection panel, even after a Game-view click. The frame-based virtual Input System panel suite passed on both integrated runs. This discrepancy is recorded as an unresolved OS-helper/input-path observation, not hidden or declared a physical-keyboard regression. Native R retention was checked across all eight Heavy cases. Human keyboard/feel testing remains required.

### Final visible inspection and handoff

The final Heavy experience inspection fixture reran its four model groups/eight native cases successfully, then opened its genuinely experienced Breaker at safe spawn. Baseline, Breaker and Momentum cards were visually inspected in the small Game view: values match their assigned definitions, including 60 Breaker poise, 1.20 m Momentum advance and 0.55 s Momentum miss recovery. Text wraps within the scroll panel; no extra Inspector configuration or scene rebuild was required. This is a technical readability inspection, not accessibility or player acceptance.

The panel showed its resolved save directory under ignored Logs/HeavyExperienceChecks, confirming that subsequent UI load operations used disposable fixture records. A fresh-character confirmation was armed on A, then the actual Load B button was clicked: B's identity/Baseline/one point appeared and the Fresh button returned to its unconfirmed state. A Momentum investment preview was opened on B, then Load A was clicked: A's Breaker identity returned and B's pending preview disappeared. No UI save/overwrite button was clicked. These checks close the previous manual load-latch/preview gate without touching personal slots.

Unity was left **stopped on DevelopmentTest**. All source changes are limited to game-owned C# and project documentation. git diff --check passes; protected scene/prefab/attack-asset/vendor/package/ProjectSettings paths remain unchanged. Commits: `2ffb032` records retained explanations; the next checkpoint records authored technique previews and final verification (see branch log/handoff). New active mechanics remain deferred to the existing three player experiments. No new automated test remains unexecuted; physical-keyboard behavior, gameplay fairness, balance, standalone builds and accessibility remain outside this technical result.

### Launch and inspect

Open Assets/Game/Scenes/DevelopmentTest.unity in the existing Editor and press Play. Existing WASD/Q/E, Space, mouse/wheel, LMB/RMB, Shift and R remain. Complete or interrupt a Heavy, press R, then Tab and scroll to Recent attacks. Its experience explanation should remain beneath that exact attack. Repetition must not grant points; fresh/load clear session review while supported saved facts remain. No scene rebuild or new Inspector wiring is needed.

The next smallest milestone is a player comparison of completed contact, a useful interrupted trade and repetition, using the retained explanation to judge the existing rule. The existing three experiments in docs/CHARACTER_IDENTITY_PATH.md remain the next design path. This presentation improvement prepares fairness observations without accepting new eligibility or power rules.

---

## Latest follow-up - offline character-state safeguards

Started clean at `13ceb84` on `development/heavy-experience`; continued on `development/offline-character-checks`. Implementation checkpoint: **768e96f**. This assignment used direct files and noninteractive CLI only. No computer-use tools, screenshots, UI automation, Unity launch/batch mode/Editor control, scene/prefab changes or personal-save access occurred. Unity results in the historical section below do not verify this follow-up.

### Implemented and reproduced

The first offline run compiled actual production state code and reproduced `Owns("") == true` on a fresh character: **8/9 cases passed, 9,070 assertions, exit 1**. The unselected primary ID was empty. Ownership queries now reject null/empty IDs. Existing purchase/configuration validation already rejected empty IDs, so this was not a demonstrated power exploit.

Investment previews now capture character GUID, revision and development ID in an immutable transient value. Confirmation rejects another character even at the same revision; existing revision checks reject intervening history/knowledge/configuration changes. DevelopmentPanel uses this command through OwnedCharacter's safe-spawn boundary. It clears pending confirmations when observing a different character identity; fresh/load also clear the fresh-character confirmation latch.

This is defense against stale/replayed/external replacement intent, not a reproduced ordinary button-path purchase bug: those paths already cleared investment previews. The revision-only overload remains for trusted diagnostics. This is not authentication or a session nonce: restoring an identical character/revision still represents the same model intent, while the panel clears pending intent on explicit load.

The save DTO and discovery enum were extracted into standalone game-owned source files without changing namespace, type names, field types or enum values. Store algorithms are unchanged. Save schema remains **2**, with existing non-mutating schema-1 migration and protected unknown-content policy. The preview is never persisted. No new reward, automatic purchase, refinement, stamina change or combat modifier was added. History changes revision without changing build revision or accepted combat snapshots.

### Checks actually executed

Executed `dotnet run --project tools/CharacterStateChecks/CharacterStateChecks.csproj --configuration Release --no-launch-profile` with installed SDK **6.0.301**, runtime **6.0.6**, and ignored local CLI home. No dependencies were installed/downloaded; package sources are cleared. Final result: **10/10 cases, 9,082 assertions, exit 0**, including 3,000 seeded command steps. Repeated invariants are not thousands of independent gameplay scenarios.

The harness directly links five production files: CharacterDevelopmentState.cs, CharacterSaveData.cs, DevelopmentPurchasePreview.cs, HeavyExperienceCatalog.cs and DiscoveryKnowledge.cs. No Unity stubs or copied domain implementation. Cases cover identity, empty/default/cross-character/stale/replayed confirmations, ownership/knowledge/activation separation, once-only grants, history validation/deduplication, non-mutating migration, illegal DTOs, unknown-content fallback and array isolation. Portable JSON roundtrip uses **System.Text.Json**, not Unity JsonUtility. Fixtures are in-memory and never access personal saves. Exact command, source paths and prerequisites: tools/CharacterStateChecks/README.md.

Also passed `git diff --check` and static checks for protected-path scope, unique extracted type definitions and unique new meta GUIDs. Compiler outputs/cache are ignored.

**Not executed:** Unity project compilation; all Editor suites, including the added DevelopmentChecks assertions; panel integration; native input/physics/receipt timing; Unity JsonUtility; file-store I/O/backups; standalone build; player testing. OwnedCharacter, DevelopmentPanel and CharacterSaveStore are excluded from the offline compilation. .NET 6 success does not establish Unity assembly/runtime/lifecycle/serialization compatibility. Historical file-store tests were not rerun. No gameplay feel, balance or fairness validation is claimed.

### Changed files

Paths below are relative to the repository root:

- Assets/Game/Progression/CharacterDevelopmentState.cs - empty-ID ownership fix and bound preview API.
- Assets/Game/Progression/DevelopmentPurchasePreview.cs and .meta - transient immutable intent.
- Assets/Game/Progression/OwnedCharacter.cs - bound command through existing safe-development checks.
- Assets/Game/Progression/DevelopmentPanel.cs - capture/confirm bound intent and reset pending confirmation state.
- Assets/Game/Progression/CharacterSaveData.cs and .meta - unchanged DTO extracted from CharacterSaveStore.cs.
- Assets/Game/Progression/CharacterSaveStore.cs - DTO definition removed; store logic unchanged.
- Assets/Game/Combat/DiscoveryKnowledge.cs and .meta - unchanged enum extracted from RetaliationDiscovery.cs.
- Assets/Game/Combat/RetaliationDiscovery.cs - enum definition removed; discovery logic unchanged.
- Assets/Game/Editor/DevelopmentChecks.cs - empty/default/cross-character/replay assertions added, not run in Unity.
- tools/CharacterStateChecks/{Program.cs, CharacterStateChecks.csproj, NuGet.Config, .gitignore, README.md} - explicit offline harness; no import/startup execution hook.
- AGENTS.md; docs/MILESTONES.md; docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md; docs/CHARACTER_IDENTITY_PATH.md; this report - constraints, contracts, verification gates and next experiments.

### Manual Unity verification - pending, not executed

No new Inspector wiring or scene rebuild is intended. Save your current work and use the existing Editor, without launching another instance against this project.

1. Allow scripts to import and compile. Resolve any new compile/Console error before accepting the slice.
2. With Play stopped and current scene saved, run **Tools > EvolvingRPG > Run Extended Regression Checks**. This existing twelve-suite runner includes the new development model assertions. Inspect fresh reports and Console; older passing logs are not this run's evidence. Automated file fixtures use isolated ignored Logs locations.
3. Open **Assets/Game/Scenes/DevelopmentTest.unity**, press Play and click Game view. At spawn press **Tab**, preview Momentum, cancel, preview again and confirm. Expect one point spent once, Momentum selected, no unsolicited evolution. Another primary must remain unavailable. Close Tab, press **R**, reopen Tab and verify ownership remains.
4. Create a fresh experimental character through confirmation. Expect a new ID, one point, Baseline and no prior investment/save confirmation. Start a fresh-character confirmation, then load a disposable test character: the next Fresh click must request confirmation again. Use only slots whose existing contents you deliberately intend to replace; do not overwrite personal progress for this check. Prefer the isolated automated save fixtures. Cross-character model rejection is covered offline; panel lifecycle still needs Unity.
5. Check **WASD, Q/E, Space, mouse look/wheel, LMB/RMB, Shift**, dodge while strafing, obstacle collision and existing opponents. Complete a live Heavy, **R**, **Tab**: facts remain descriptive and repetition grants no points. Report feel separately from technical success.
6. Exit Play and inspect for new Console errors. Save/reload, discovery/return and experience integration must pass on this head before claiming verification.

### Next smallest milestone

Manually verify this safeguard in Unity, then observe existing DevelopmentTest before choosing another active talent. docs/CHARACTER_IDENTITY_PATH.md retains exactly three proposals: fair descriptive experience, a deliberate familiarity refinement only if its isolated comparison earns it, and contextual use/decline of the existing return. Each includes the decision, foundation, liability, smallest comparison, support/revise/reject evidence, prerequisites and what can be prepared offline.

State contracts cannot decide whether useful trades deserve credit or whether a refinement creates a meaningful choice. Finite facts do not solve sustained mastery or recurring rewards. Do not build that economy from this test result. Final documentation checkpoint and push outcome are supplied in the handoff and branch log.

---

## Historical follow-up — durable Heavy experience

Started clean at `61436d2` on `development/combat-readability`; continued on `development/heavy-experience`. Unity 6000.6.7f1 was already open and stopped on DevelopmentTest. No second Editor, scene/prefab overwrite, vendor modification or personal-save write was used.

### Implemented and deliberately deferred

Selected **durable qualitative history without a new combat modifier**. DevelopmentTest now records completed live Heavy impact, guard break and windup interruption facts for Baseline and the character's owned discipline. Each fact can occur once per technique per character. Ordinary/evolved Momentum share a history. Recorded outcomes come from frozen pre-impact receipts and require full player recovery and an unchanged enabled opponent generation. A genuine approach hit before the enemy's first strike is eligible, addressing the old practice trial's reactive-only bias. Idle/returning/foreign targets, dummies, zero impacts, misses, interrupted actions and stale/disabled opponents are rejected.

Facts do not award points, reduce recovery, change discovery or gate purchases. Experience changes character history revision only, never build revision or an accepted snapshot. The existing HeavyPractice points and optional recovery trial remain unchanged in PracticeTest. This is not infinite mastery and does not establish a reward economy. Full-recovery credit can exclude useful trades; that remains a player-feedback question rather than proof of poor play.

Retention policy: completed facts enter the in-memory character on completion; R clears pending receipts but keeps facts. Explicit safe-spawn save writes the full character and fact set together. The fact set is also the lifetime credit budget, so no transient anti-replay counter can reopen credit after loading. Load replaces unsaved state, never merges it. Fresh characters start empty. Save failure retains in-memory facts and the prior disk record. Replaying an unsaved fact after rolling back can only restore that missing fact, never create additive currency.

Schema 2 adds stable versioned experience IDs. Schema 1 migrates in memory with empty history while preserving identity, revisions, purchases, knowledge, balance and configuration. No accomplishment is inferred from old purchases or discovery counts. Reading does not rewrite; explicit save writes schema 2 and retains the previous supported file as backup. Unknown experience/discovery/schema records and legacy records with unexpected populated history are protected from overwrite or silent rollback. Duplicate/inconsistent history is rejected. This is a one-way supported upgrade; older executables cannot read schema 2. It is not automatic repair, indefinite backup retention or anti-cheat protection.

The panel explains history separately from investment and the existing action review. Learned Heavy cards list demonstrated facts rather than a mandatory completion checklist. Combat feedback names newly demonstrated outcomes. The action review remains independent presentation; clearing it cannot erase experience.

### Checkpoints

- `755199a`: stabilize combat test input with a restored 60 Hz simulation clock and sampled focus diagnostics; production input/timing unchanged.
- `12ccfcb`: complete compiling history/migration slice and model/native tests; the twelve-suite run passed before the final inspection refinement.
- `b809ba9`: genuine-history inspection fixture, shared safe-panel entry and named fact feedback; final twelve-suite pass.
- The final documentation checkpoint is listed in `git log`.

### Actual checks and failures

The first prerequisite run missed a single Space tap; an unchanged isolated rerun and full eleven-suite rerun passed. A later variable-clock run missed Q+Shift. Inspection found sub-50 ms input windows susceptible to Editor stalls. The combat fixture now uses 60 Hz simulation, restores the previous clock and reports focus loss during sampled input. An initial over-eager focus guard rejected neutral startup frames; it was narrowed to sampled frames. No production jump, dodge or input rules were changed. These failures are documented rather than discarded as gameplay observations; their exact original cause was not isolated to one host condition.

After that correction, all twelve suites passed, including the 26 combat cases, real number-row event edges, movement/camera regressions, C1/C2/C3, opponents, old practice, snapshots/review, owned progression/save and panel lifecycle. The additional experience suite passed three isolated groups and eight native cases: true pre-first-strike initiative; R/repetition; interrupted impact; target reset before completion; miss; actual dummy contact; actual guarded-windup Breaker with A/B save/load; and repeated guard/interception after reload. The second saved character remained empty. No runtime errors were captured in those passing runs.

Save checks passed real file roundtrip/backup, repeated loading, schema-1 non-writing migration with unchanged build and empty history, explicit schema-2 save retaining the legacy backup, unknown-ID protection, malformed/future/inconsistent data, traversal/I/O failure and native history retention. All file tests used unique ignored Logs directories, not personal saves.

The final source state passed all twelve suites in the run started at `2026-11-05T21:17:34Z`, with no failed or skipped stages. The inspection fixture also passed and its genuinely recorded Breaker history was visually inspected in the small Game view: three facts, selected/owned Breaker and separate empty Baseline history were readable. Unity was then left stopped on DevelopmentTest. Reports: `Logs/ExtendedChecks.txt`, `Logs/HeavyExperienceChecks.txt`, `Logs/CharacterSaveChecks.txt` and the existing suites; copied captures under ignored `Logs/ExperienceValidation`. Detailed Editor activity remains in `Logs/FocusedDevelopmentUnity.log`. Historical errors/warnings remain in that log; a clean captured run is not a claim that historical Console output is empty.

These are technical tests and narrow UI inspection. No physical-player session, standalone build, power-loss simulation, accessibility evaluation or gameplay-feel/balance validation was performed. C1's conservative sparse-sampling limitations remain. Finite first-time history does not solve recurring mastery, mission reward settlement or offline save editing.

### Launch, inspect and playtest

Open `Assets/Game/Scenes/DevelopmentTest.unity` in the existing Unity Editor, press Play and click Game view. No new scene rebuild, package or Inspector wiring is needed. Controls stay WASD/Q/E, Space, mouse/wheel, LMB/RMB and Shift. Finish a Heavy against an approaching or attacking opponent, then R and Tab. Read **Character experience** and the learned Heavy card. Compare a miss, interruption, successful contact and repetition. With an owned Breaker, inspect guard break/interception. Experience itself changes no combat values.

Save explicitly to A, create/save a fresh character to B, then load A to compare independent history/builds. Loading older schema 1 starts experience empty, with an explanatory migration message. Existing safe-boundary restrictions and `Application.persistentDataPath/CharacterDevelopmentPrototype` location remain.

For technical checks use **Tools > EvolvingRPG > Run Extended Regression Checks** (Ctrl+Shift+F11) with scenes saved, Unity focused and Play stopped. **Run Heavy Experience Checks** (Ctrl+Shift+Alt+F7) runs only the new model/native sequence. **Inspect Heavy Experience Fixture** (Ctrl+Shift+Alt+F6) runs that sequence, then leaves a genuinely experienced Breaker open in the safe-spawn panel. Stop Play after inspecting. It uses isolated test saves under Logs.

### Actual changed files

- Added `Assets/Game/Progression/HeavyExperienceCatalog.cs`, `HeavyExperienceCollector.cs` and their `.meta` files.
- Added `Assets/Game/Editor/HeavyExperienceChecks.cs` and `.meta`.
- Modified `Assets/Game/Progression/CharacterDevelopmentState.cs`, `CharacterSaveStore.cs`, `OwnedCharacter.cs`, `DevelopmentPanel.cs`.
- Modified `Assets/Game/Editor/CharacterSaveChecks.cs`, `CombatPlayModeChecks.cs`, `ExtendedChecks.cs`, `OvernightChecks.cs`.
- Added `docs/HEAVY_EXPERIENCE_HISTORY.md` and `docs/CHARACTER_IDENTITY_PATH.md`.
- Updated `AGENTS.md`, `README.md`, `docs/MILESTONES.md`, `docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md` and this report.

All scenes/prefabs, attack definitions, combat execution, stamina, movement/camera scripts, third-party assets, Packages and ProjectSettings are unchanged from `61436d2`.

### Next smallest milestone

[CHARACTER_IDENTITY_PATH.md](CHARACTER_IDENTITY_PATH.md) distinguishes the working boundaries from missing ones and gives three bounded next experiments with support/revise/reject criteria: experience fairness including initiative/trades; a useful deliberately selected familiarity refinement using the old isolated trial first; and whether the same developed character makes different decisions across familiar return contexts. No extra combat development was implemented. The next dependency is player feedback on whether the history is understandable and fair and whether any refinement creates a useful choice. Do not turn first-time facts into a compulsory checklist or serialize the old point economy without a recurring reward/retention policy.

---

## Earlier follow-up — combat readability and protected knowledge

Started clean at `fc47760` on `development/character-ownership`; created `development/combat-readability`. The earlier ownership/save report is preserved below as history. No reset, scene overwrite, package change or second Editor instance was used.

Implementation checkpoints:

- `31eef00`: explicit focused regression runner; the existing ten-suite loop passed before runtime changes.
- `a7d4d4a` and `9a0bf83`: paired save/readability changes and tests. The latter is the complete runtime checkpoint: unknown discovery-rule protection, actual applied-impact messages and bounded attack review. The save-check additions reference the review API introduced by the latter commit; use the complete checkpoint when checking out this work.
- `70e1d6d`: final eleven-suite validation, mode-aware overlay and repeatable review inspection.
- The final documentation commit is available in `git log`; no history was rewritten.

### Implemented behavior

The HIT message previously displayed authored damage and poise even when a guard, depleted health or existing stagger reduced the actual consequence. It now displays the resolved receipt with guard/opening/poise-break information. This changes feedback, not combat values.

DevelopmentTest's panel now includes **Recent attacks**, a six-entry session review. It freezes the accepted action's name, advance and linked context, sums actual applied contacts, records interruption versus completion, measures elapsed simulation time and net horizontal displacement, and explains ordinary miss recovery separately from the linked-opponent penalty. Bystander contact and a reset opponent generation do not satisfy the linked return. Interrupted actions do not claim that unfinished recovery was paid.

R retains this review so the player can return to spawn and inspect it. Fresh-character creation and successful load clear it. It contains no durable mastery or rewards and cannot change discovery, ownership, balance or accepted actions. Completed entries retain text/scalars rather than live opponent references. Observed displacement is not path length or proof of why travel shortened; a completed hit is not proof of safety.

An additional compatibility gap was identified in save handling: an unknown nonempty `discoveryId` was treated as invalid data, allowing fallback to an older known backup. It is now protected content. Load refuses interpretation with a diagnostic, and neither the original nor a protected backup can be overwritten. Existing supported schema-1 saves need no migration. Malformed supported records still use the documented recovery path; there is no automatic repair or refund.

The reused C1 overlay also described DevelopmentTest as having no evolution effects. It now has owned-mode wording. The standalone C1 comparison keeps its original diagnostic-only label.

The new explicit regression runner uses the existing focused Editor, switches only saved scenes, rejects incomplete/failed/skipped stages, checks each suite's completion marker and restores the original active scene on success. It now includes the movement-only Q/E suite. It is not a general additive-scene-layout preservation tool. Nothing runs automatically on import.

### Validation and limits

The existing ten suites passed before runtime changes. After the save/review changes, all eleven focused suites passed, including:

- 26 combat checks for attack commitment, input edges, dodge, jump, camera, geometry and interruption.
- Full technique checks including number-row events and native travel at 60/15 Hz; guarded-impact message assertion uses the actual receipt.
- C1 conservative classification/query/lifecycle/native checks; sparse 15 Hz samples remain unverified, as intended.
- Existing enemy, HeavyPractice, snapshot and C2 discovery suites.
- New snapshot/review checks for actual impact, immutable labels after authoring edits, interrupted/unfinished recovery, miss explanations, bounded history, idempotent subscription, clear and disposal.
- All nine owned native retaliation cases now compare review data to committed form, observed travel/duration and the real ordinary/linked recovery rules, including walls, bystanders, reset and defeated targets.
- Save tests now cover unknown discovery IDs in both latest and backup records, plus existing corruption/future/unknown-primary/economy/I/O cases. Native load/fresh checks verify review isolation as well as absence of transient opportunities. Tests write only under Logs.
- Panel Tab open/close/reopen and exit-with-panel-open checks.
- Thirteen movement-only cases covering Q/E, normalized forward/backward diagonals, opposing-key cancellation, WASD turning, jump, mouse look and zoom.

A native spawn Heavy miss was also generated by the explicit review inspection fixture. Its panel text was visually inspected at approximately 590 x 505 Game-view size: the completed miss, actual zero impact, committed/displaced distance and recovery fields wrap legibly in the opaque scroll panel. This is narrow UI inspection, not gameplay-feel validation. The original inspection shortcut conflicted with TextMeshPro; it was moved to Ctrl+Shift+Alt+F12 without changing Editor preferences.

The final overlay/inspection/runner changes passed all eleven suites in the focused run started at `2026-11-05T17:09:27Z`; `Logs/ExtendedChecks.txt` records every stage and overall completion, with no skipped stages. Local captured reports are under ignored `Logs/ExtendedValidation`; detailed Editor activity is in `Logs/FocusedDevelopmentUnity.log`. Earlier failed/diagnostic history is not proof of a current regression. No standalone build, crash/power-loss simulation, physical-player session, accessibility evaluation or balance validation was performed. Existing obsolete-API/package warnings remain; warnings are not represented as a clean global Console history.

### Launch and controls

Open `Assets/Game/Scenes/DevelopmentTest.unity` in the existing Unity 6000.6.7f1 Editor, press Play and click Game view. No new Inspector setup, scene rebuild, package or asset import is required. Normal WASD/Q/E/Space/mouse/wheel/LMB/RMB/Shift controls and existing investments remain unchanged. R resets the encounter; at spawn Tab opens development. Scroll to **Recent attacks** after a fight. Existing A/B save/load is explicit and retains the previously documented safe-boundary restrictions.

For technical checks with Play stopped and scenes saved, use **Tools > EvolvingRPG > Run Extended Regression Checks** (Ctrl+Shift+F11). Keep Unity focused and leave input alone until completion. **Inspect Action Review Fixture** (Ctrl+Shift+Alt+F12) runs a real miss and panel checks, then leaves Play running with the review visible; stop Play afterward. Neither fixture writes user saves.

### Changed files in this follow-up

- Added `Assets/Game/Combat/CombatActionReview.cs` and `.meta`.
- Added `Assets/Game/Editor/ExtendedChecks.cs` and `.meta`.
- Modified `Assets/Game/Combat/CombatActions.cs`, `EvasionEvidenceHUD.cs`.
- Modified `Assets/Game/Progression/OwnedCharacter.cs`, `DevelopmentPanel.cs`, `CharacterSaveStore.cs`.
- Modified `Assets/Game/Editor/AttackSnapshotChecks.cs`, `TechniquePlayModeChecks.cs`, `RetaliationChecks.cs`, `CharacterSaveChecks.cs`, `DevelopmentPanelInputChecks.cs`.
- Updated `AGENTS.md`, `README.md`, `docs/MILESTONES.md`, `docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md` and this report.
- Added `docs/NEXT_COMBAT_PLAYTESTS.md`.

All scene/prefab files, existing attack definitions, stamina, movement/camera scripts, third-party assets, Packages and ProjectSettings are unchanged from `fc47760`.

### Ideas and next dependency

[NEXT_COMBAT_PLAYTESTS.md](NEXT_COMBAT_PLAYTESTS.md) contains three prioritized proposals, each with its player decision, existing foundation, consequence, smallest comparison and support/revise/reject criteria:

1. **Spend an opening or preserve the exit:** compare taking/declining extended return across open space, obstruction and the existing second threat.
2. **Give a guard break a purpose:** compare pressure, repositioning and withdrawal; verify the paper concern about repeated Breakers before changing balance.
3. **A shorter alternative return:** consider an isolated replacement handling form only if the first comparison demonstrates a valuable missing expression. Existing neutral ordinary Momentum may already solve it.

No new combat talent was selected: current feedback does not establish a missing mechanic. The implemented review improves the ability to test those questions. Player judgment about return intent, readable consequences and credible alternatives is the next dependency. HeavyPractice remains a session-only trial because durable mastery still needs an explicit replay/budget and migration policy. No speculative mastery serialization, release respec policy or breadth expansion was introduced.

---

## Earlier ownership/save handoff (preserved history)

## Recoverable state

Started with a clean working tree at `4bdf2e0` on `development/overnight-character-evolution`. Work continues on `development/character-ownership`; no previous checkpoints were reset.

Implementation checkpoints:

- `bb610ba`: record the passed focused input gate and scope ownership.
- `9d0c84f`: character-owned Heavy discipline, contextual knowledge, separate learning and activation.
- `8fe3816`: versioned explicit saves, validation and protected backup recovery.
- `ecaf2f2`: focused panel input checks, readable confirmations, safe Play-mode teardown.

This report is the final documentation checkpoint. Use `git log` for its commit identifier. The prior C1/C2/C3 scenes remain comparisons.

## What is playable

`Assets/Game/Scenes/DevelopmentTest.unity` adds a character-owned development experiment using the existing opponents, combat definitions, evidence classifier and native movement. Shared attack assets do not hold purchases or character balances.

A fresh character has a unique identity and one experimental point. Baseline is free. An explicit preview and confirmation spends the point on either Breaker or Momentum and selects that Heavy. This creates an actual opportunity cost. One-primary ownership, the two possible grants and absence of respec are prototype rules awaiting feedback, not permanent character policy.

Momentum knowledge grows through the existing verified evasive exchanges. Three distinct qualifying enemy commitments reveal Retaliatory Momentum and grant one evolution point once. Reveal does not purchase or activate it. The player must separately learn the revealed development and activate it with compatible Momentum. Verified evasion supplies the defensive experience prerequisite; there is no purchasable empty defensive fixture. Debug investment/technique/trial switches cannot grant owned power in this scene.

The compact panel explains eligibility, learned versus active power and tradeoffs. Its background is opaque, cards wrap and scroll, and confirmations remain above the scrolling area. Development commands require a ready, living, unstaggered player within 2 m of spawn and idle or defeated active opponents. Opening the panel suspends player input only; the world continues.

Explicit A/B saves preserve identity, legal ownership, paid costs, balance, knowledge and configuration. Schema 1 uses stable IDs and validates prerequisites and economy on load. It does not serialize enemies, accepted attacks, pending exchanges or retaliation opportunities. A loaded active evolution needs a new verified evasion to arm.

Writes flush and validate a same-directory temporary file before replacing the destination and retaining its prior version as `.bak`. A malformed latest record can recover a valid backup without rewriting the damaged original. Unsupported schemas refuse loading; unknown primary/configuration IDs retain the original record, select Baseline, deactivate evolution and block overwrite pending reviewed migration. Failed loads retain the current in-memory character. There is no automatic refund, repair, migration or autosave.

## Actual validation

All checks below ran in Unity 6000.6.7f1. Editors were used sequentially; no second Editor was launched against an open project. The final Editor is stopped on DevelopmentTest.

| Gate | Actual result |
| --- | --- |
| Focused prerequisite input | Existing 26-case combat suite passed; focused technique suite passed, including number-row event edges. The original focused capture remains in `Logs/FocusedUnity.log`. |
| Existing regression coverage | Exclusive batch runs passed supported C1 evidence/query/lifecycle/native checks, technique/motor checks, enemy checks, practice checks, immutable snapshots, C2 discovery and nine C3 native retaliation cases. Batch reports explicitly skip focused number-row edges; they do not replace the focused run. |
| Ownership model | Fresh identity, costs, stale/duplicate commands, single primary, once-only reveal grant, knowledge/learning/activation distinction, compatibility and character isolation passed. |
| Native owned loop | Actual three-exchange discovery, deliberate purchase and activation, blocked debug bypass, nine owned retaliation cases and fresh reset passed. |
| Save model/files | Real JSON roundtrip and File.Replace backup; repeated-load balance integrity; independent A/B characters; corrupt/future/missing-content protection; invalid balance/configuration, traversal and I/O failure handling passed. |
| Native owned save/load | Busy commands rejected; legal build, knowledge and balance restored; independent second character retained; no accepted action or opportunity resumed. |
| Focused panel input | Input System Tab events opened, closed and reopened the panel while correctly suspending/restoring movement and look. Exit with panel open passed without runtime teardown errors. |
| Visual UI action | Inspected the small Game view and confirmed an actual mouse-driven Momentum preview/confirmation changed balance from 1 to 0 and locked the competing primary. No user save was written. |
| Final checks | After the UI/lifecycle fixes, focused combat (26 cases), owned development/save/native retaliation and focused panel exit checks passed again. Unity compiled the final code. |

Local reports are ignored generated files under `Logs/`, with final copies under `Logs/ContinuedValidation`. Native file tests use isolated temporary directories under Logs, not the player's persistent save directory. The later batch technique report includes its explicit focused-input skip; the original focused Editor log supplies that earlier gate's evidence.

One real failure was found during UI validation: closing Play mode with the panel open could reactivate PlayerInput after Unity had unregistered it. The owner now verifies enabled/registered input before restoration, and the dedicated exit check captures the corrected behavior. The earlier diagnostic remains in historical logs; fresh focused captures pass. Windows-injected helper keystrokes did not reliably reach Unity runtime Raw Input, so focused Tab checks inject through the existing Input System rather than weakening production input handling.

These are technical checks and a narrow UI inspection. They do not establish player feel, balance, accessibility or hardware-input acceptance. No standalone player build was produced. Conservative evasion sampling remains unchanged; sparse 30/15 fps evidence can remain unverified under its existing maximum sample-gap rule.

## Launch and playtest

1. In the existing Unity Editor, open `Assets/Game/Scenes/DevelopmentTest.unity` and press Play. No package installation or Inspector wiring is required for the committed scene. If the scene is missing, stop Play and use **Tools > EvolvingRPG > Create Character Development Sandbox**; the builder refuses to overwrite an existing scene.
2. Click Game view. Use WASD, Q/E strafe, Space jump, mouse look/wheel zoom, LMB Light, RMB Heavy and Left Shift directional dodge. R resets the encounter and returns to spawn while preserving character development.
3. At the safe starting area, press Tab. Preview and confirm a primary discipline. Close Tab and click Game view to recapture the cursor. Compare Breaker and Momentum using separate fresh characters; fresh creation requires confirmation and replaces unsaved state.
4. On the Momentum character, evade a genuine committed threat, then apply Momentum to that same opponent within 1.5 s after full dodge recovery and finish Heavy recovery. Repeat against three distinct commitments. Read the clue/reveal feedback. Harmless or unverified dodges do not qualify.
5. Press R, then Tab. Purchase the revealed Retaliatory Momentum and explicitly activate it. On a subsequent verified evasion, forward movement intent plus RMB requests the extra 0.4 m advance; neutral/backward/lateral intent retains ordinary Momentum. Missing the linked opponent adds 0.25 s recovery, in addition to ordinary miss behavior where applicable.
6. At spawn, save deliberately to A or B, confirming the action. After exiting/re-entering Play, open Tab and load the desired slot. Loading explicitly replaces unsaved state and resets the encounter. There is no automatic load. Save a fresh second character to the other slot to compare independently.

The panel displays the resolved directory: `Application.persistentDataPath/CharacterDevelopmentPrototype`, currently `%USERPROFILE%\AppData\LocalLow\DefaultCompany\EvolvingRPG\CharacterDevelopmentPrototype`. Corrupt original records are protected; save recovered data to another healthy slot instead of expecting automatic repair.

Repeatable Editor checks are in **Tools > EvolvingRPG**: **Run Character Development Checks**, **Run Character Save Checks**, **Run Development Panel Input Checks** (Ctrl+Shift+F9, leaves Play running) and **Run Development Panel Exit Checks** (Ctrl+Shift+F10, exits Play). These run test encounters and should only be invoked when no personal Play session or unsaved scene work is in progress.

## Actual changed files

New runtime files, with their `.meta` files:

- `Assets/Game/Progression/CharacterDevelopmentState.cs`
- `Assets/Game/Progression/CharacterSaveStore.cs`
- `Assets/Game/Progression/OwnedCharacter.cs`
- `Assets/Game/Progression/DevelopmentPanel.cs`
- `Assets/Game/Progression.meta`

Modified combat adapters:

- `Assets/Game/Combat/ContextualDiscovery.cs`
- `Assets/Game/Combat/RetaliationDiscovery.cs`
- `Assets/Game/Combat/RetaliatoryMomentumTrial.cs`

New Editor files, with their `.meta` files:

- `Assets/Game/Editor/DevelopmentSandboxBuilder.cs`
- `Assets/Game/Editor/DevelopmentChecks.cs`
- `Assets/Game/Editor/CharacterSaveChecks.cs`
- `Assets/Game/Editor/DevelopmentPanelInputChecks.cs`

Modified check integration: `Assets/Game/Editor/DiscoveryChecks.cs`, `Assets/Game/Editor/RetaliationChecks.cs`, `Assets/Game/Editor/OvernightChecks.cs`.

New scene: `Assets/Game/Scenes/DevelopmentTest.unity` and its `.meta`.

Documentation: `AGENTS.md`, `README.md`, `docs/MILESTONES.md`, `docs/CHARACTER_DEVELOPMENT_PLAYGROUND.md`, and this report.

Earlier scenes, attack assets, movement/camera scripts, stamina tuning, third-party files, Packages and ProjectSettings are unchanged from the starting checkpoint. No broader game systems were added.

## Next smallest milestone

Player acceptance of this complete loop is the next dependency: compare two owned characters, assess whether the one-primary choice expresses identity, whether the discovery explanation is understandable, and whether learning versus activation is clear. Check ordinary/evolved Momentum commitment and whether manual save/reload preserves the expected character. Keep stamina permissive for interruption testing.

After feedback, refine the existing cards/causal explanations or demonstrated input defects before adding another evolution. A subsequent bounded experiment may connect the earlier HeavyPractice proficiency track to this character history, but only after deciding what useful experience should persist and how schema migration protects existing characters. That proficiency track is currently separate and is not saved here. Full mastery, permanent economy/respec policy, procedural evolution and broader content remain proposals or deferred work. More mechanics would outrun the current feedback evidence.
