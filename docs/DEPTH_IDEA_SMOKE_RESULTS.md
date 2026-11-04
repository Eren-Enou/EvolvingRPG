# Basic depth-idea checks — 2026-11-04

At the user's request, ran a small standalone PowerShell design-model check. **Six checks passed. Unity was not launched, loaded or controlled.** No gameplay code, scenes, assets or stamina parameters changed.

Run again from the project root:

```powershell
& ./tools/DesignChecks/DepthIdeaSmokeChecks.ps1
```

## What passed

| Check | Result |
| --- | --- |
| Four commitment units, deep cost three, support cost one | Deep + support fits; two deep commitments do not |
| Inactive learned deep development | Still counts toward the proposed learned-commitment policy |
| Rejected then accepted action, followed by duplicate acceptance | Rejection preserves the opportunity; acceptance consumes once; duplicate cannot consume again |
| Wrong opponent, changed encounter revision, expired opportunity | Contextual benefit rejects without consuming the fixture |
| Target at 48/60 poise, 0.30 s until contact | Authored Light meets the ideal break deadline; Breaker does not |
| Fresh target at 0/60 poise, 0.80 s until contact | Breaker can meet the one-contact break goal; Light cannot |

The timing checks read Light and Breaker windup/poise values directly from the existing attack assets. Target thresholds, windows, capacity and opportunity data are explicit hypothetical fixtures. The tiny opportunity model treats expiry as a half-open interval; this is a fixture convention, not a finalized gameplay rule.

## Meaning and limits

The timing examples illustrate how target state and available time can change which existing action meets a specific goal. That supports investigating depth in the existing actions before adding more branches.

The commitment and opportunity checks exercise a proposed standalone model. They do **not** test an implemented specialization or retaliation system: those systems are still design work. A model passing its assertions does not establish that these policies are desirable.

No collision, movement, animation, actual hit sampling, defensive attribution, camera behavior, save persistence, reward economy or gameplay feel was tested. The fixtures assume both attacks are in reach, unobstructed, accepted immediately and aimed at a non-staggered target. Meeting the break deadline does not prove an action is optimal in a real encounter.

Related design: [depth-first principles](DEPTH_FIRST_DESIGN.md) and [Heavy discipline study](HEAVY_DISCIPLINE_DEPTH_STUDY.md). Interactive validation remains future work.
