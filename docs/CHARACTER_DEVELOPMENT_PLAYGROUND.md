# Character development playground

Open **Assets/Game/Scenes/DevelopmentTest.unity** in Unity 6000.6.7f1. Press Play and click Game view. This isolated scene preserves all earlier comparisons. If missing, use **Tools > EvolvingRPG > Create Character Development Sandbox** with Play mode stopped.

At the starting area press **Tab** to inspect developments. The panel releases the cursor and temporarily suspends player input; it does not pause combat or grant invulnerability. Closing Tab restores input; click Game view to recapture. Development is allowed only within 2 m of spawn, while ready/unstaggered and with active opponents idle or defeated. Close the panel and press **R** to return to that state.

A fresh experimental character has one point. Preview and confirm **Breaker** or **Momentum**; purchase spends one point and selects that Heavy. Only one primary discipline may be owned. Baseline remains free and selectable. These restrictions are a test fixture, not final specialization/respec rules. Create another fresh character to compare; this replaces unsaved state and requires confirmation.

For the Momentum path, perform verified evasion followed by applied same-opponent Momentum within 1.5 s of full dodge recovery and complete Heavy recovery. One qualifying exchange supplies a clue; three distinct enemy commitments reveal Retaliatory Momentum. The verified evasion itself supplies defensive experience: no empty defensive talent or investment switch is required or sold. Reveal grants one additional experimental point once per character. It does not learn or activate the effect.

Return with R, inspect with Tab, explicitly purchase the revealed return, then explicitly activate it. A subsequent verified evasion can arm its opponent-specific return window. Forward movement intent relative to current facing + RMB requests +0.4 m advance; neutral/backward/lateral intent keeps ordinary Momentum. Missing the linked opponent generation adds +0.25 s recovery, including bystander-only contact. Normal Momentum miss recovery also remains. No stamina, damage, poise or invulnerability change.

Normal WASD, Q/E, Space, mouse look/wheel, LMB/RMB and Shift remain. R resets the encounter and pending chains while preserving character development. Old debug selectors 1/2/3, F2/F4/F5/F8 cannot grant or change owned power in this scene. F3 still toggles the optional fast opponent through the evidence diagnostics. All tuning remains subject to player testing.

**Tools > EvolvingRPG > Run Character Development Checks** validates state commands and runs the real native discovery -> purchase -> activation -> retaliation chain. Reports: `Logs/DevelopmentChecks.txt`, `Logs/DiscoveryChecks.txt`, `Logs/RetaliationChecks.txt`. Focused combat/technique input checks were also run successfully before this implementation; automated correctness does not establish feel.

## Explicit local saves

The development panel provides slots **A** and **B**. Saving requires a confirmation; loading explicitly replaces unsaved in-memory state. There is no autosave or automatic load. Save before exiting Play mode, then enter Play again, open Tab at spawn and load the desired slot. Create another fresh character and save it to the other slot to compare paths.

Saves live under `Application.persistentDataPath/CharacterDevelopmentPrototype`, currently `%USERPROFILE%\AppData\LocalLow\DefaultCompany\EvolvingRPG\CharacterDevelopmentPrototype`. The panel shows the resolved directory. The schema contains character GUID, revisions, stable development IDs, paid costs, knowledge contributions, legal configuration and the checked experimental resource balance. It contains no enemy/Unity references, Time.time timestamps, pending discoveries, current attacks or temporary opportunities. Loading resets the encounter and requires a new verified evasion before an active return can arm.

A same-directory temporary file is flushed and validated before replacement; the previous supported record becomes `.bak`. Malformed latest data can load a complete valid backup with an explanation. The damaged original is preserved and cannot be overwritten automatically: save recovered data to the other healthy slot. Unsupported schemas are preserved without falling back to older power. Unknown primary/configuration IDs retain their saved ownership record, select Baseline, deactivate the effect and block saving pending reviewed migration. Invalid balances/prerequisites/configurations refuse activation. This is a local prototype, not anti-cheat protection or a general crash-proof guarantee.

Save errors retain the in-memory character and show a message. No automatic refunds, migration, deletion or repair are performed. Schemas other than version 1 are not supported yet. Saves do not include the earlier HeavyPractice proficiency trial; this is the ownership/knowledge boundary, not a completed production mastery economy.

**Run Character Save Checks** exercises temporary files under ignored Logs, not user saves. **Run Character Development Checks** also exercises native owner save/reload after actual discovery and combat, including two independent characters, busy-action rejection and absence of restored opportunities.
