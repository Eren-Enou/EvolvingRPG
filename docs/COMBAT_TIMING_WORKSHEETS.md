# Combat timing worksheets

Paper analysis of authored prototype values, not runtime measurement or gameplay validation. No Unity execution occurred. These worksheets help reject impossible design assumptions before implementation. Actual sampling, movement, collision, target state and player response still require future tests.

Sources: [technique parameters](TECHNIQUE_EXPERIMENT.md), [opponent parameters](ENEMY_SANDBOX.md), the current CombatActions dodge phases and immutable AttackSnapshot. These values are the current experiment baseline, not recommended final balance.

## Known candidate timings

| Action | Windup | Active | Recovery | Ideal total on hit | Extra miss recovery |
| --- | --- | --- | --- | --- | --- |
| Baseline Heavy | 0.50 s | 0.18 s | 0.65 s | 1.33 s | 0 |
| Breaker | 0.65 s | 0.18 s | 0.85 s | 1.68 s | 0 |
| Momentum | 0.40 s | 0.16 s | 0.45 s | 1.01 s | 0.55 s |
| Dodge | Travel 0.25 s | No invulnerability window | Recovery 0.35 s | 0.60 s | Not applicable |
| Guarded enemy strike | 0.85 s | 0.16 s | 1.15 s | 2.16 s | Not applicable |
| Fast enemy strike | 0.45 s | 0.12 s | 0.70 s | 1.27 s | Not applicable |

The enemy's next strike need not start immediately at recovery end: facing, range and line of sight can delay it. The worksheet's immediate recommitment assumption is a conservative pressure scenario. A hit can also stagger the enemy and invalidate that schedule.

The player has no ordinary attack/dodge cancellation. A hit landing during an opening does not imply the player's whole action is safe. Include recovery and the enemy's next possible contact.

## Worksheet A — retaliating against the guarded opponent

Assume enemy commitment at t = 0.00, dodge accepted at t = 0.65, and a successful geometric evasion. The dodge's actual path and contact safety are assumptions to test later, not facts inferred from this schedule.

| Time | Ideal event |
| --- | --- |
| 0.00 | Guarded enemy starts windup and commits facing |
| 0.65 | Player accepts dodge |
| 0.85–1.01 | Enemy strike is active |
| 0.90 | Dodge travel ends; player is still in dodge recovery |
| 1.25 | Full dodge completes; defensive receipt could finalize if evidence is complete |
| 1.25 | Earliest immediate Momentum acceptance in this scenario |
| 1.65 | Momentum's active interval begins |
| 1.81 | Momentum active interval ends |
| 2.16 | Enemy recovery ends; earliest immediate next windup |
| 2.26 | Momentum completes if it hit |
| 2.81 | Momentum completes if it missed |
| 3.01 | Earliest next guarded active interval if it could recommit at 2.16 |

On paper, this enemy can offer a usable retaliation window. That does not prove the player returns to range, remains safe through collision, faces correctly or reads the cue. Extra advancement may help connect, but it may also push the player into the next threat.

With the proposed C3 additional 0.25 s miss drawback, this Momentum miss completes at 3.06, slightly after the next ideal active start. This creates a plausible consequence to investigate. It is not a guarantee of punishment; enemy position, facing and other actions still matter.

## Worksheet B — retaliating against the fast opponent

Assume commitment at t = 0.00 and dodge accepted at t = 0.30. Again, no actual evasion is proved.

| Time | Ideal event |
| --- | --- |
| 0.00 | Fast opponent commits |
| 0.30 | Player accepts dodge |
| 0.45–0.57 | Fast strike active interval |
| 0.55 | Dodge travel ends |
| 0.90 | Full dodge completes; earliest immediately accepted Momentum |
| 1.27 | Enemy recovery ends; earliest next windup |
| 1.30 | Momentum begins its active interval |
| 1.46 | Momentum active interval ends |
| 1.72 | Earliest next fast strike active interval |
| 1.91 | Momentum completes on hit |
| 2.46 | Momentum completes on miss |

If the opponent stays able to strike, Momentum may leave the player committed when the next strike becomes active. But its 25 poise impact can combine with prior accumulation; position may avoid the next arc; the enemy may backstep; and the first hit can change the schedule. This is a specific risk to investigate, not proof of an unavoidable trade.

Design consequence: do not promise that verified evasion always earns a safe Heavy. The evolution can offer an opportunity with a decision, not a guaranteed counterattack. If its intended identity requires reliable retaliation here, test a different enemy opening, travel profile or technique design. Do not silently add invulnerability to make the worksheet fit.

## A reveal window is not a safe-action window

The proposed 1.5 s discovery/opportunity timer is an eligibility limit for Heavy acceptance after full dodge completion. It is not a 1.5 s enemy stun or guaranteed safe interval. Explain this distinction in descriptions.

The C2 knowledge evaluator can finalize a valid prior sequence after all evidence arrives. C3 cannot retroactively modify a committed action. If evidence finishes after a Heavy is accepted, that Heavy remains ordinary. Measure the proportion of opportunities that arrive too late before deciding the effect is responsive enough.

Do not extend the eligibility timer silently to compensate for delayed processing. An expired window should produce a clear diagnostic. If the rule is too conservative, revise the rule explicitly.

## Sensitivity to small recovery changes

The existing practice trial reduces only base recovery by 8%:

| Technique | Authored base recovery | Trial base recovery | Saved commitment |
| --- | --- | --- | --- |
| Baseline Heavy | 0.650 s | 0.598 s | 0.052 s |
| Breaker | 0.850 s | 0.782 s | 0.068 s |
| Momentum | 0.450 s | 0.414 s | 0.036 s |

Momentum's 0.55 s miss addition remains untouched. These are arithmetic consequences of the authored values, not measured feel. Tens of milliseconds can matter near a boundary while being difficult to perceive in a broad comparison.

Test both an obvious opening and a borderline case. Do not increase the trial repeatedly until it is noticeable if that removes all exposure. A mastery effect can be mathematically bounded and still be tactically excessive.

## Why the guard-break follow-through may fail

The lab proposes extra poise on an optional Light after a guard break. The guarded enemy is staggered by the break; additional poise may offer little value while it is already staggered. The player also remains committed to the breaking Heavy's active interval and recovery before any Light can start.

A Heavy that breaks poise at the beginning of its active interval has roughly active duration plus recovery left. For Breaker, that is about 1.03 s, before considering sampling. Against the guarded opponent's 1.2 s stagger, only a small part of stagger may remain once the player is free. An after-completion follow-up window does not itself extend enemy stagger.

This suggests three separate alternatives to compare later, not three automatic fixes:

1. Replace the benefit with a useful positional follow-up rather than more poise during stagger.
2. Trigger after a different disruption event where the target is not already fully staggered.
3. Keep the effect but admit that it is designed for later multi-target/poise-recovery situations, and remove it from the first vertical slice.

Do not shorten Breaker's recovery merely to rescue this follow-up. That would alter the very commitment distinction being evaluated.

## Future authoring worksheet template

For each new encounter/action comparison, record:

```text
Question / player decision:
Unchanged reference:
Enemy commitment start:
Enemy active interval and earliest later threat:
Player accepted action and full completion:
Movement assumptions / required reach:
Collision or target-state assumptions:
Evidence finalization time:
Opportunity acceptance window:
Benefit and drawback:
Expected counterplay:
Unsupported or ambiguous cases:
Future observed result:
Retain / revise / discard decision:
```

Leave observed result blank until an actual test. Do not turn an ideal schedule into a reported pass. Use slower and faster frame conditions, normal movement, walls and interruptions during future validation.

No timing, movement, balance or stamina parameters were changed. These paper examples identify questions; they do not answer them through play.
