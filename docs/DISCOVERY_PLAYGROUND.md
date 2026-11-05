# Contextual discovery playground

C2 implementation in DiscoveryTest: explicit session-only investment fixtures plus ordinary Momentum combat can foreshadow and reveal Retaliatory Momentum. Discovery grants knowledge only. It neither purchases nor equips an effect. Thresholds and the 1.5-second acceptance window are experimental.

## Launch

Open `Assets/Game/Scenes/DiscoveryTest.unity` in Unity 6000.6.7f1. If absent, use **Tools > EvolvingRPG > Create Discovery Sandbox**. Press Play and click Game view.

- Existing WASD, Q/E, Space, mouse look/zoom, LMB, RMB and Shift remain.
- **3** selects ordinary Momentum; 1/2 remain debug comparisons, not ownership.
- **F4** toggles the experimental Momentum investment prerequisite; **F5** toggles the separate defensive investment fixture. Both start off. Changes require an idle, unstaggered actor and discard a pending exchange. These flags are temporary research controls, not real talents.
- Approach the guarded opponent. Dodge early after its yellow commitment. Starting space must actually be threatened, and C1 must verify the complete evasion. Turn/reposition deliberately and land Momentum against that opponent, accepting RMB within 1.5 seconds of full dodge recovery. Complete the attack recovery.
- One qualifying exchange supplies a clue; three distinct enemy strikes reveal the possibility. The journal explains the linked evidence. No automatic power is granted.
- **R** resets the encounter and pending exchange while retaining session knowledge/fixtures. **F8** starts a fresh session character, clearing knowledge and prerequisites. Exiting Play mode discards all progress. PracticeTest's original reset rules are unchanged.

## Boundaries

The evaluator requires a verified receipt, prerequisites captured at dodge acceptance and unchanged through completion, the actual Momentum definition, an applied impact on the evaded enemy's same encounter generation, and uninterrupted Heavy recovery. Enemy reset, wrong target, Light/other Heavy, unsupported evidence, missing prerequisites and late acceptance cannot qualify. Only the first accepted attack following an eligible dodge is considered by this small experiment. Duplicate enemy strike keys cannot supply another contribution. Buffers are bounded and delayed receipt processing does not extend the original timing window.

C1's documented conservative geometry/sample bounds still apply. There is no Heavy Practice refinement here. Nothing persists; investment fixtures are not a talent economy. Combat feel, clue readability and whether the exchange is tactically enjoyable await player testing.

## Checks

Use **Tools > EvolvingRPG > Run Discovery Checks**, with scenes saved and Play mode stopped. It runs deterministic negative cases and three normal-frame native-motor evade/Momentum exchanges, writing `Logs/DiscoveryChecks.txt`. See `OVERNIGHT_PROGRESS_REPORT.md` for results actually executed in this session.

Files: `RetaliationDiscovery.cs` separates knowledge evaluation from `ContextualDiscovery.cs`'s actor subscriptions/input fixtures. `DiscoveryHUD.cs` presents diagnostics/journal. `DiscoverySandboxBuilder.cs` copies the evidence scene with Unity APIs and preserves existing scenes. `DiscoveryChecks.cs` covers the model and real action chain. All custom sources remain under Assets/Game.

## Optional C3 combat comparison

Open **Assets/Game/Scenes/RetaliationTest.unity** for discovery plus the optional combat effect. This separate copy preserves DiscoveryTest as the knowledge-only reference. If absent, use **Tools > EvolvingRPG > Create Retaliation Trial Sandbox**. No manual component wiring is required.

Start with **3**, **F4**, **F5**, and demonstrate the three qualifying exchanges described above. Once knowledge is Revealed, **F2** enables or disables the **trial, not purchased** effect while idle. Enabling does not reuse an old opportunity: perform another verified evasion.

A finalized verified receipt arms one opponent-specific opportunity until **full dodge recovery + 1.5 seconds**. With the trial enabled, **forward world movement intent relative to current character facing + RMB** requests the extended form. Neutral, backward and lateral intent retain ordinary Momentum. This checks existing camera-relative movement intent, not a new modifier key; forward diagonals can opt in. The camera does not steer or aim the committed strike. Read the armed-state cue before committing. Held forward input can opt in, so intent/readability needs playtesting.

The trial adds **0.4 m** to Momentum's **1.2 m** windup advance. It changes no damage, poise, stamina or dodge parameters. It adds **0.25 s recovery unless the linked enemy generation receives an applied impact**. A bystander hit does not remove that cost; a complete miss also retains ordinary Momentum's **0.55 s** miss recovery. An opportunity is consumed once after successful action acceptance, even if the action later misses, collides with a wall or is interrupted. Rejected input and previews do not consume it. Expired, defeated, reset, disabled or unavailable opportunities permit ordinary legal Momentum.

The snapshot and target context stay committed for the action. Disabling the resolver does not rewrite an accepted strike. Resetting the target after acceptance invalidates linked-target success. There is no homing, cancellation, invulnerability, stacking, refund or Heavy Practice refinement. R clears transient opportunities while retaining session knowledge/fixtures/trial selection. F8 clears knowledge, fixtures and trial activation; it grants no learned power. F7 expands the bounded journal. The combined HUD keeps combat state visible; R/F1/F6 remain owned by the original encounter component.

Inspector tuning: `ContextualDiscovery` has a counter window sampled when Play mode begins; `RetaliatoryMomentumTrial` exposes added advance and target-miss recovery for subsequent accepted actions. Authored attack definitions are unchanged. These values and the intent policy are proposals.

**Tools > EvolvingRPG > Run Retaliation Trial Checks** first reveals the possibility through three actual native C2 exchanges, then tests nine C3 cases with real motor travel, impacts and recovery. It writes `Logs/RetaliationChecks.txt`. Automated cases establish correctness, not fun, balance or validated feel. Full mouse/keyboard interaction remains for focused Game-view testing.
