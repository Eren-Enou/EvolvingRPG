# Talent map, journal and debrief flows

Future UX specification, not implemented UI. The first prototype needs a few truthful cards, not the complete living-map interface. Related: [talent charter](EVOLVING_TALENT_SYSTEM.md), [character contracts](CHARACTER_STATE_AND_CONTENT_CONTRACTS.md), [opening journey](FIRST_HOURS_AND_CHARACTER_JOURNEYS.md).

## What the player needs to understand

Before a choice: what will change, why it is available, what it costs, what it prevents or deactivates, and whether it can be reversed. After an exchange: what happened and whether there is something worth investigating. During combat: enough feedback to act without reading a build spreadsheet.

Begin with a development panel centered on Heavy, two visible alternative cards and a contextual journal entry. Add map zoom/pan when actual content no longer fits, not because a future tree might be large.

## State presentation

Visibility, eligibility, ownership and configuration are separate dimensions. Not all theoretical combinations are valid: an owned development should be inspectable, and an active development must be learned and currently legal.

| Situation | Primary presentation | Honest explanation |
| --- | --- | --- |
| Undiscovered | Absent from character map | Do not show a fake cost or pretend unfinished content exists |
| Foreshadowed | Subtle incomplete connection or journal clue | Direction is suggested; exact outcome may not yet be known |
| Revealed but prerequisites missing | Known card, locked state | Show achievable missing conditions without hiding the name again |
| Revealed, eligible but resource insufficient | Known card, unavailable purchase | Show cost and current balance; no ambiguous disabled button |
| Revealed and purchasable | Available choice with comparison | Cost, changed behavior, drawback and reversibility visible |
| Learned but not configured | Owned card with inactive status | Say whether this is ordinary configuration or missing eligibility |
| Learned and active | Active mark and current-action summary | Do not imply every learned effect affects every action |
| Learned definition missing after content update | Preserved unresolved record | Effect inactive; safe fundamentals retained; no silent deletion |

Eligibility should be derived from current prerequisites/resources, not stored as a stale permanent "available" flag. Learning updates ownership; choosing a legal configuration updates active behavior. Discovery alone does neither.

## First choice storyboard

1. Player returns from a mission and sees a concise debrief. One controlled development token is shown as secured only if settlement succeeded.
2. The player may open development immediately or return to movement. A forced purchase is unnecessary.
3. Heavy shows current ordinary behavior and two named alternatives. Each card shows a behavior sentence and a downside before detailed numbers.
4. Selecting a card opens a comparison. Example: "Planted strike with stronger poise impact; longer preparation and recovery." The exact current proposed timings can be shown in expandable detail.
5. Preview creates a labeled temporary test context with no practice/rewards, only at a safe practice location. It does not grant ownership or edit shared content.
6. Purchase shows cost and affected configuration. On selection, the authoritative state owner revalidates and returns one result.
7. A successful result shows learned/configured status truthfully. A failed result explains the actual current cause without deducting anything.

For reversible debug purchases, state "Prototype refund available" rather than implying a permanent release policy. Do not introduce a frightening irreversible confirmation for a reversible test. If a future design adopts an irreversible commitment, it needs explicit review and a preview of the exact consequences.

## Compact card example

Illustrative future UI text, not a new balanced talent:

```text
BREAKER — Heavy development
Create a stronger frontal disruption opening.

Changes: planted strike, higher poise impact.
Tradeoff: longer preparation and recovery; no windup advance.
Requires: this character's relevant Heavy familiarity.
Cost: 1 prototype development token.

Status: available / learned / active (show actual applicable states)
[Compare] [Preview in practice] [Invest]
```

Do not claim that every hit will break a guard: target thresholds and prior poise accumulation differ. Tooltips describe authored changes; combat receipts describe actual applied consequences. Use consistent terms for damage, poise and stagger.

## A clue arrives

During a fight, a small non-blocking indicator may mark a new journal observation. It must not cover enemy anticipation or take mouse focus. At a safe moment or debrief, the player can read the clue.

Candidate foreshadowed entry:

> Your evasive movement is beginning to shape your advancing strikes.

Supporting text can identify the relevant relationship without a hidden numerical checklist: "Investigate how a developed advancing technique follows an actually evaded strike." A debug panel may expose exact rule conditions and evidence counts for testing.

A journal explanation cannot call a dodge successful unless the verified evidence exists. An empty-space dodge or blocked enemy attack may produce ordinary diagnostics, but not this clue.

## A discovery becomes known

Reveal entry contains: name, what it could change, its drawback, the character's relevant general history and current prerequisites. The primary action is Inspect, not auto-equip. If purchase is unavailable, explain why. Do not show a celebratory equipped banner on a knowledge-only event.

Example cause: "Developed Momentum and defensive technique; repeatedly returned to the same opponent after verified evasive exchanges." The exact cause record should reflect actual rule evidence, not boilerplate pasted onto any reveal.

If a parent investment is later refunded, preserve the journal entry and distinguish "known" from "currently eligible." Do not erase the historical experience or keep the effect active by accident.

## Debrief hierarchy

Proposed order:

1. Mission outcome and secured objective/rewards.
2. One or two noteworthy character developments, if any actually occurred.
3. Optional equipment comparison or development action.
4. A clear next step: retry, another offered contract, practice, or leave the panel.

Failure debrief distinguishes secured knowledge, retained practice, unsecured loot and uncompleted objective according to the selected policy. A reward not saved successfully is not labeled secured. Reopening a report does not apply it again.

Avoid displaying every miss and rejected attack as a performance judgment. Detailed diagnostics remain inspectable for debugging. The main report should support reflection and choice, not grade the player into one correct style.

## Account knowledge and a fresh Warrior

Provide an optional archive view labeled "Known to you." Character development remains labeled "Developed by this character." An archived option can explain a direction without filling in this character's experience, mastery or purchases.

Two presentation policies remain open: archive knowledge can expose a known information card in a separate view, or add an annotated clue to the character map. Neither policy may mark a character-specific discovery prerequisite fulfilled automatically. Compare which preserves surprise while allowing informed replay.

Do not require another character merely to read an archive entry. Conversely, importing an archive must not reveal every unobserved possibility in the whole game. Archive records come from actual discoveries or authored information sources.

## Configuration and dependency changes

Before a deep-focus change or parent refund, show: removed costs/returned resources, affected learned dependencies, active effects becoming inactive, and resulting legal action configuration. Cancel leaves everything unchanged. Apply is one command against a current state revision.

The prototype can use simple text rather than drawing a dependency animation. A truthful small interface is preferable to a large graph that hides why a button is disabled.

If rules or content change after a save, show a concise repair report at a safe location. Keep unresolved records and safe fundamentals; do not surprise the player mid-fight with vanished Heavy input. No automatic repurchase or arbitrary compensation without a selected migration policy.

## Input and accessibility

Development menus release or deliberately manage cursor capture; closing them restores the expected game focus. Combat buttons must not leak through an open menu into an attack. Distinguish paused inspection from in-world non-paused overlays and test that choice explicitly.

Remapping should display action names and current bindings rather than hard-coded Q/E/Shift assumptions. Keyboard/mouse and future controller navigation must not alter what counts as an accepted action or verified outcome. Color is supplementary to icon/shape/text. Keep text scalable and motion/feedback intensity adjustable when those features are implemented.

## Future evaluation

Ask a player to identify one available option, explain its drawback, find why another is locked, preview without buying, deliberately buy, discover something without activating it, refund a parent, and inspect a fresh character with an archive entry.

Watch for false assumptions: "I saw it, so I own it," "I own it, so it must be active," or "I found it on another character, so this one has done the prerequisite." Fix labels and flow before making the map larger.

No UI, purchase flow, archive, preview mode or reward report was implemented or run in this design pass.
