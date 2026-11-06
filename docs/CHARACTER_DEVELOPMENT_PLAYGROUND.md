# Character development playground

Open **Assets/Game/Scenes/DevelopmentTest.unity** in Unity 6000.6.7f1. Press Play and click Game view. This isolated scene preserves all earlier comparisons. If missing, use **Tools > EvolvingRPG > Create Character Development Sandbox** with Play mode stopped.

At the starting area press **Tab** to inspect developments. The panel releases the cursor and temporarily suspends player input; it does not pause combat or grant invulnerability. Closing Tab restores input; click Game view to recapture. Development is allowed only within 2 m of spawn, while ready/unstaggered and with active opponents idle or defeated. Close the panel and press **R** to return to that state.

A fresh experimental character has one point. Preview and confirm **Breaker** or **Momentum**; purchase spends one point and selects that Heavy. Only one primary discipline may be owned. Baseline remains free and selectable. These restrictions are a test fixture, not final specialization/respec rules. Create another fresh character to compare; this replaces unsaved state and requires confirmation.

**Confirmation contract (offline follow-up; Unity integration pending):** an investment preview captures character identity, history revision and development ID. Confirming against another character or a changed revision refuses the command; preview again. History/knowledge changes can invalidate a preview without changing an accepted combat snapshot. Replaying a purchase cannot spend twice. Loading the identical character/revision is still the same model intent; the panel clears pending confirmations on load and clears them when it observes a different character. Fresh and load clear the fresh-character confirmation latch. No save field or schema change is needed. The revision-only command overload remains for trusted diagnostics, not panel confirmations.

For the Momentum path, perform verified evasion followed by applied same-opponent Momentum within 1.5 s of full dodge recovery and complete Heavy recovery. One qualifying exchange supplies a clue; three distinct enemy commitments reveal Retaliatory Momentum. The verified evasion itself supplies defensive experience: no empty defensive talent or investment switch is required or sold. Reveal grants one additional experimental point once per character. It does not learn or activate the effect.

Return with R, inspect with Tab, explicitly purchase the revealed return, then explicitly activate it. A subsequent verified evasion can arm its opponent-specific return window. Forward movement intent relative to current facing + RMB requests +0.4 m advance; neutral/backward/lateral intent keeps ordinary Momentum. Missing the linked opponent generation adds +0.25 s recovery, including bystander-only contact. Normal Momentum miss recovery also remains. No stamina, damage, poise or invulnerability change.

Normal WASD, Q/E, Space, mouse look/wheel, LMB/RMB and Shift remain. R resets the encounter and pending chains while preserving character development. Old debug selectors 1/2/3, F2/F4/F5/F8 cannot grant or change owned power in this scene. F3 still toggles the optional fast opponent through the evidence diagnostics. All tuning remains subject to player testing.

**Tools > EvolvingRPG > Run Character Development Checks** validates state commands and runs the real native discovery -> purchase -> activation -> retaliation chain. Reports: `Logs/DevelopmentChecks.txt`, `Logs/DiscoveryChecks.txt`, `Logs/RetaliationChecks.txt`. Focused combat/technique input checks were also run successfully before this implementation; automated correctness does not establish feel.

## Explicit local saves

The development panel provides slots **A** and **B**. Saving requires a confirmation; loading explicitly replaces unsaved in-memory state. There is no autosave or automatic load. Save before exiting Play mode, then enter Play again, open Tab at spawn and load the desired slot. Create another fresh character and save it to the other slot to compare paths.

Saves live under `Application.persistentDataPath/CharacterDevelopmentPrototype`, currently `%USERPROFILE%\AppData\LocalLow\DefaultCompany\EvolvingRPG\CharacterDevelopmentPrototype`. The panel shows the resolved directory. The schema contains character GUID, revisions, stable development IDs, paid costs, knowledge contributions, legal configuration and the checked experimental resource balance. It contains no enemy/Unity references, Time.time timestamps, pending discoveries, current attacks or temporary opportunities. Loading resets the encounter and requires a new verified evasion before an active return can arm.

A same-directory temporary file is flushed and validated before replacement; the previous supported record becomes `.bak`. Malformed latest data can load a complete valid backup with an explanation. The damaged original is preserved and cannot be overwritten automatically: save recovered data to the other healthy slot. Unsupported schemas are preserved without falling back to older power. Unknown primary/configuration IDs retain their saved ownership record, select Baseline, deactivate the effect and block saving pending reviewed migration. Invalid balances/prerequisites/configurations refuse activation. This is a local prototype, not anti-cheat protection or a general crash-proof guarantee.

Save errors retain the in-memory character and show a message. No automatic refunds, deletion or repair are performed. The supported schema-1 migration is described below; other schemas remain protected. Saves include the new bounded experience facts but not the earlier HeavyPractice points/recovery trial. This is not a completed production mastery economy.

**Run Character Save Checks** exercises temporary files under ignored Logs, not user saves. **Run Character Development Checks** also exercises native owner save/reload after actual discovery and combat, including two independent characters, busy-action rejection and absence of restored opportunities.

## Focused panel checks

With Unity focused and Play mode stopped, **Tools > EvolvingRPG > Run Development Panel Input Checks** (Ctrl+Shift+F9) injects Tab through Unity's Input System, verifies opening/closing and player-input ownership, then leaves the panel open for visual inspection. Stop Play mode afterward. **Run Development Panel Exit Checks** (Ctrl+Shift+F10) also exits with the panel open and captures teardown errors. These are technical input checks; Windows-injected helper keystrokes are not evidence of physical-player testing.

The panel uses an opaque background so diagnostic overlays cannot obscure investment cards. Purchase and save confirmations remain above the scroll area. A confirmed Momentum UI purchase was checked in the Editor; temporary UI test characters were not written to user save slots.

## Inspect commitment and consequences

After fighting, press R to return to spawn, then Tab and scroll to **Recent attacks**. The last six completed/interrupted attacks retain the accepted name/form, applied damage/poise, committed advance, observed net horizontal displacement, miss explanation and separate ordinary/linked miss recovery. A bystander hit does not count as landing the linked return. An interrupted action does not claim to have paid its remaining recovery. The on-screen HIT message now shows the actual applied impact, including guard and poise break, rather than authored damage.

This is a session review, not mastery or durable character history. R keeps it for inspection; fresh creation and successful load clear it. Nothing here is serialized or used for eligibility, spending or rewards. Net displacement is not path length or proof of collision, and multi-target totals include all applied contacts. A successful hit is not proof that the ending position was safe.

**Tools > EvolvingRPG > Run Extended Regression Checks** (Ctrl+Shift+F11) runs all twelve focused suites, including Q/E movement, camera, combat, technique event edges, evidence, opponents, practice, snapshots/review, discovery, ownership/save, panel teardown and Heavy experience. Start with saved scenes and Play stopped; keep Unity focused and leave test input alone. Combat input checks use a 60 Hz simulation clock so short input windows receive frames even during Editor stalls; production timing is unchanged. It stops on failed, incomplete or skipped stages and restores the original active scene after success. Reports are under `Logs/`, summarized in `Logs/ExtendedChecks.txt`. It temporarily switches scenes and is not intended for preserving an additive multi-scene editing layout.

**Inspect Action Review Fixture** (Ctrl+Shift+Alt+F12) performs a real Heavy miss at spawn, verifies panel input, then leaves Play running with the review section visible. This is an explicit inspection fixture, not a physical-input or gameplay-feel test. Stop Play when done. It writes no user save. The three prioritized future comparisons are in [NEXT_COMBAT_PLAYTESTS.md](NEXT_COMBAT_PLAYTESTS.md).

Saves now use schema 2 for bounded character experience. Schema 1 migrates in memory with empty experience and unchanged identity/build/knowledge/balance; reading does not rewrite the original. Explicit save writes schema 2 and keeps the prior supported record as its backup. Unknown discovery-rule or experience IDs protect latest and backup records instead of silently recovering older known content. Corrupt supported records retain the documented backup-recovery path. Older checkouts cannot read schema 2; do not expect automatic downgrade.

## Character experience

The panel's **Character experience** section and learned Heavy cards show completed live impact, guard break and windup interruption facts. A fact is recorded once per technique per character, after full recovery. A Heavy against an approaching opponent can count before its first strike. Idle/returning targets, dummies, misses, interruptions and reset/disabled opponents do not count. Normal and evolved Momentum share one history.

R retains completed facts and discards pending receipts. Save explicitly to retain facts across Play sessions; loading replaces unsaved history rather than merging it. Fresh characters start empty. Repetition after reset or reload cannot add a fact twice. These records give no recovery bonus, talent points or automatic discovery, and do not gate the existing primary investment. The older PracticeTest/F2 trial remains separate.

Use **Run Heavy Experience Checks** (Ctrl+Shift+Alt+F7) for the isolated model and native sequence, including real initiative, interruption, stale target, dummy, guard/interception and A/B save/reload cases. It writes only under Logs. See [Heavy experience rules and retention](HEAVY_EXPERIENCE_HISTORY.md) and [the next character-identity path](CHARACTER_IDENTITY_PATH.md).

**Inspect Heavy Experience Fixture** (Ctrl+Shift+Alt+F6) runs those checks and leaves the genuinely experienced Breaker open at safe spawn for visual inspection. Stop Play afterward; this fixture's saves are isolated under Logs.
