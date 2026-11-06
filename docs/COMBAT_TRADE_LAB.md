# Combat trade comparison lab

This is a controlled experiment supporting the existing experience-fairness question, not a new talent, reward rule or balance recommendation.

## Run or inspect

With saved scene edits and Play stopped in the existing Unity 6000.6.7f1 Editor:

- Tools > EvolvingRPG > Run Combat Trade Checks (Ctrl+Shift+Alt+F8) runs the cases and stops Play.
- Tools > EvolvingRPG > Inspect Combat Trade Fixture (Ctrl+Shift+Alt+F9) runs the cases and leaves the two results in DevelopmentTest's Recent attacks at safe spawn. Stop Play after inspection.
- Run Extended Regression Checks now includes this as the thirteenth suite.

No scene rebuild or Inspector wiring is required. The fixture opens DevelopmentTest and uses its real motor, combat actions, guarded enemy, vitality, receipts, review and experience collector. It changes runtime positions and one runtime enemy-home field, restoring the home and encounter afterward. It temporarily uses a 60 Hz simulation clock and restores the prior clock/background setting. It never changes an attack definition, scene asset, poise threshold, recovery duration, stamina or save file. It does not preserve an additive scene-editing layout; run from a saved single-scene setup.

## What is controlled

A Baseline Heavy is deliberately accepted shortly after the nearby guarded opponent commits its strike. It lands before that strike hits during player recovery. Both actions are real; no outgoing/incoming receipt is fabricated.

| Case | Initial player poise | Observed outgoing | Observed incoming | Outcome |
| --- | --- | --- | --- | --- |
| Fresh | 0 | 9 guarded HP, 40 poise | 18 HP, 20 poise during recovery | Completes; records the first Baseline contact fact |
| Nearly broken | Fixture preloads 20 poise before acceptance | 9 guarded HP, 40 poise | 18 HP, 20 poise during recovery | Stagger interrupts; no new fact |

These are measurements at current assigned tuning, not assertions that future values must remain unchanged. The fixture checks structural outcomes and reports actual totals. The initial poise preload uses an explicit zero-health-damage setup hit while ready. It is clearly artificial setup, not naturally earned player evidence, a saved build or a new ability.

R retains the two immutable attack results and matching experience explanations. Completion and contact are different facts. The experiment must not conclude that the interrupted trade was bad play, or that the completed one was safe. It measures only received impact while committed, not enemy damage prevented, later danger, positional advantage or the player's intended result.

## Three decisions to investigate next

1. **Credit interpretation:** did the guard pressure justify a trade even when recovery did not finish? Compare the player's explanation with the recorded facts. Support the current rule if it is understandable and fair; revise descriptive language if it sounds judgmental; test any alternate eligibility policy separately before changing saved facts or points.
2. **Familiarity application:** use the existing isolated PracticeTest recovery trial off/on, without retaliation stacked onto it. Compare actual committed recovery, incoming impact and next return to control. Keep the liability if a refinement is proposed. A small timing difference alone is not evidence that it deserves an owned talent.
3. **Return or preserve the exit:** use the existing developed Momentum character inactive/active at matched investment, varying distance/obstruction/second threat. Ask when the player chooses ordinary Momentum, Light or withdrawal. Received damage during commitment helps explain consequences but cannot establish safety after completion. Do not add a shorter-return talent unless existing intent fails to express a demonstrated choice.

These refine the same three experiments in CHARACTER_IDENTITY_PATH.md. They add no new mandatory button sequence, progression grind or inferred tactical-intent score.
