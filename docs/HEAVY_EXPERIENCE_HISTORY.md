# Heavy experience history experiment

This slice connects meaningful outcomes to an owned character's durable history. It does not integrate the older PracticeTest point economy or its recovery modifier. Rules are experimental and require player feedback.

## Selected boundary

For Baseline and the character's owned Heavy discipline, record whether the character has completed:

- An applied Heavy impact in a live exchange.
- A Heavy guard break.
- A Heavy poise break during an opponent's windup.

These are qualitative facts, not ranks, XP, talent points or automatic discoveries. A single completed action may demonstrate more than one fact. Repeating the same fact cannot increase it. No timing, damage, poise, movement, stamina, recovery or purchase prerequisite changes.

The stable authored fact IDs include the Heavy profile, outcome and rule version. Ordinary and evolved Momentum share the Momentum history; the effect does not create a second farming track. Baseline experience remains useful history after an investment. The character cannot acquire an unowned discipline's history through a debug selector.

## Evidence policy

Accept only the explicitly referenced Heavy definitions. Capture character/build identity at action acceptance. Require an applied impact against an enabled actual enemy engaging this actor, with pre-impact phase Approach, Windup, Active, Recovery or Staggered. Require full player recovery and an unchanged opponent encounter generation before committing the fact.

Approach is intentionally eligible even before the enemy's first strike. This addresses the older trial's reactive-only bias without claiming to infer the player's intent. Idle/returning targets, training targets, foreign-source receipts, misses, interrupted actions and stale/reset/removed/disabled opponents give no fact. A valid killing impact can count when the player finishes recovery; current defeat does not erase its pre-impact evidence.

Use frozen hit receipts, not the target's post-hit phase, to distinguish guard breaks and windup interruptions. Pending receipts are bounded and are discarded on reset, disable, character replacement or load. They are never saved. Diagnostic/action-review clearing does not affect experience.

## Retention and replay policy

Facts commit to the in-memory character after completed recovery. Explicit safe-boundary save writes the complete character, including its fact set. The fact's presence is also its lifetime credit budget: gain and deduplication are the same serialized datum. There is no separate temporary award ledger that a reload could reopen.

R resets the encounter and pending evidence while retaining completed character facts. A fresh character starts empty. Loading replaces the whole unsaved character; it never merges histories. Repeating a completed fact after reset, reload or another opponent cannot increase history. Unsaved new facts are lost on loading an earlier save or exiting Play; replaying them restores only the missing facts, with no additive resource reward.

Save failure retains the current in-memory facts and existing disk record. This is not mission reward settlement or protection against deliberate save editing. Finite facts cannot establish infinite mastery or long-term reward pacing.

## Save compatibility

Schema 2 adds the bounded `heavyExperience` fact list. Supported schema-1 characters migrate in memory with empty history, preserving their identity, revisions, purchases, knowledge, balance and configuration. No past experience is invented. New play can record every supported fact. Reading alone does not rewrite a file; an explicit supported save writes schema 2 and retains the previous record as the backup.

Unknown schemas, discovery rules and experience IDs are protected from interpretation or overwrite; duplicate/inconsistent histories are rejected. A legacy record containing unexpected populated history is protected rather than silently dropping it. Prior unknown-primary/configuration fallback remains protected. The older executable cannot read schema 2; keep its backup when comparing checkouts. No automatic downgrade or repair is provided.

## Playtest

Use DevelopmentTest with the existing controls. Land a Heavy against an approaching or attacking opponent and finish recovery, then R and Tab to inspect **Character experience** and the technique card. Compare a miss, interruption, first completed contact and repetition. Breaker can demonstrate guard break/interception without learning a new ability.

Save to A, create a fresh character and save to B, then load A. Confirm history belongs to A, while B has independent knowledge, purchases and experience. Repeat an already demonstrated outcome and confirm no additional fact or point appears. Save explicitly before leaving Play.

The remaining question is whether these records help the player recognize a fighting identity and explain progress. They are not a complete familiarity effect. A future refinement needs a separately tested benefit, remaining liability and deliberate learning/configuration policy before these facts become its prerequisites.
