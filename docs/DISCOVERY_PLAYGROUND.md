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
