# Future character state and content contracts

Specification proposals for the first progression/mission loop. These are design contracts, not a framework to implement in advance. Current code remains unchanged. Read [the progression path](PROGRESSION_DEVELOPMENT_PATH.md), [discovery experiment](CONTEXTUAL_DISCOVERY_EXPERIMENT.md) and [backlog](DEVELOPMENT_EXPERIMENT_BACKLOG.md) before assigning a slice.

## Preserve the boundaries already working

AttackDefinition authors shared values. AttackSnapshot commits values for one action. CombatObservation records session outcomes. HitResolution describes applied consequences. HeavyPractice observes a local stream and uses an optional IAttackResolver to trial a bounded recovery change.

These are useful boundaries; they are not yet durable character state, a complete effect composer, incoming threat attribution, or a mission settlement ledger. The implementation should grow at the missing boundary of the next experiment rather than replacing combat wholesale.

## Proposed state ownership

| Owner | State | Storage / invalidation |
| --- | --- | --- |
| Authored content catalog | Stable content IDs, definitions, prerequisites, rule versions, presentation references | Read-only shared assets; validate at authoring/load |
| Character record | Character ID, mastery, learned developments, commitment, knowledge, paid costs, configuration | Versioned character save; no mutable Unity object references |
| Character history | Bounded summaries of relevant accomplishments and revealed causes | Persist facts needed by current rules; do not serialize every combat frame |
| Account archive | Known discoveries and optional general clues | Separate informational record; cannot satisfy character ownership or experience |
| Mission attempt | Attempt ID, objective state, checkpoints, reward settlement keys, retained practice budgets | Versioned mission/character commit boundary |
| Encounter runtime | Enemy instance/revision/strike keys, pending receipts, short opportunities | Reset/expiry clears; no saving active hitboxes, projectiles or combo windows in the first slice |
| Actor action runtime | Accepted sequence, committed snapshot/context, phase, hit deduplication | Existing actor-local execution; clear on interruption/reset |
| Diagnostic presentation | Recent observations, explanations and report buffers | Bounded and discardable; clearing cannot change rewards or eligibility |

The serialized player prefab is not the character database. Loading a scene must not accidentally instantiate a fresh character with full resources and another mission token. Mission transitions need an explicit session owner when that slice begins; no global singleton is required now.

## Identity types

- **Content ID:** stable authored string or equivalent key, separate from display label and asset path. Renaming a title must not detach purchases. Unity GUIDs remain useful for authoring references, but any conversion to a saved content identity must be explicit and versioned.
- **Character ID:** persistent identity generated when a fresh character is actually created, not inferred from scene name or Player GameObject name.
- **Actor session key:** runtime actor instance plus a session nonce. Together with ActionSequence, distinguishes a new actor from an old callback with the same integer sequence.
- **Enemy threat key:** encounter instance, enemy instance/revision and strike sequence. Suitable for deduplication only in its defined lifecycle.
- **Mission attempt ID:** stable across checkpoint reloads of that attempt; new deliberate deployment creates a new attempt.
- **Transaction key:** unique purchase/reward settlement identity used by the local authoritative state owner to reject duplicate commands.

Do not save Unity instance IDs, object references, Time.time timestamps or raw scene coordinates as the sole identity of durable progress. IDs should not require a cloud service or multiplayer authority for this local prototype.

## Minimal character-save sketch

Illustrative data, not a runtime schema or existing save file:

```json
{
  "schemaVersion": 1,
  "characterId": "example-warrior-a",
  "revision": 12,
  "investmentBalance": 0,
  "learned": [
    { "contentId": "warrior.heavy.momentum", "paidCost": 1 }
  ],
  "configuration": { "heavy": "warrior.heavy.momentum" },
  "mastery": [
    { "trackId": "warrior.heavy", "eligibleUnits": 7 }
  ],
  "knowledge": [
    {
      "contentId": "warrior.heavy.retaliatory_momentum",
      "visibility": "foreshadowed",
      "ruleVersion": 1,
      "causeSummary": "Verified evasive retaliation with invested Momentum"
    }
  ],
  "mission": null
}
```

The example deliberately shows knowledge without ownership of the evolved option. It omits active opportunities. Character familiarity is not an archive value. Values are fixtures and do not map directly to current HeavyPractice points without an explicit migration decision.

Start persistence with the actual small state needed by choice/save. Add equipment and mission fields later with schema migration. Do not create a database for every imagined future system.

## Purchase and refund commands

Proposed purchase operation:

1. Resolve a known content ID and check catalog validity.
2. Read the current character revision; recompute reveal state, prerequisites, resource balance, compatibility and commitment constraints.
3. Produce a preview showing cost, changed behavior and any incompatible/deactivated configuration.
4. On explicit selection, revalidate against the same current state. A stale preview is not authorization to overspend.
5. Commit balance, ownership and configuration eligibility as one state change. Store the transaction result and increment revision.
6. Save at the chosen safe boundary; publish presentation after the state change. A notification callback cannot purchase again.

If saving fails, explain whether the operation is only held in memory or rolled back as a unit. Do not deduct the resource durably while leaving ownership only in memory. The prototype needs one consistent policy and a recoverable backup.

A repeated command with the same transaction key returns the previous result; a second distinct command to buy an already learned non-repeatable option rejects without cost. Refund handles dependencies atomically according to the selected design policy, using the recorded paid cost rather than silently granting today's higher catalog price.

Do not implement both an event-driven balance deduction and a separate UI-side deduction. One state owner applies the command. Presentation reads the result.

## Action acceptance with a temporary opportunity

Current IAttackResolver.Resolve runs before CanStart, which spends stamina on success. That means Resolve must remain free of opportunity consumption. A failed attack press must not lose a token.

When a contextual effect needs acceptance-linked consumption, add a narrow action proposal/commit boundary:

```text
Input intent
 -> read legal character configuration
 -> preview effective snapshot + optional opportunity identity
 -> validate actor state, resources, target/opportunity expiry
 -> commit stamina spend + opportunity consumption + action state
 -> publish accepted observation
 -> execute committed snapshot/context
```

The preview is side-effect-free. The local commit cannot partially spend one resource and then reject another. Recheck mutable eligibility before committing; publish only after the action state is coherent. Use a local version/token to prevent a stale proposal being accepted after a reset. This is an actor-local operation, not a distributed transaction service.

If an opportunity expires or its target resets, fall back or reject according to that specific effect's documented rule; never silently use an expired benefit. The first retaliation proposal rejects the expired benefit and uses ordinary legal Momentum if the ordinary action can start, with clear diagnostic wording. Consume only the opportunity actually committed. Resolve the final behavior once, before spending costs.

Interruption after acceptance does not retroactively unspend the opportunity unless that effect explicitly offers a tested refund policy. No general refund-on-miss default. A failed effect cannot enqueue an attack that will execute later without fresh input.

## Committed action context

AttackSnapshot contains scalar action parameters. A target-bound retaliation also needs immutable context: effect ID/version, opportunity ID, source threat key, designated receiver identity plus encounter revision, and target-specific success policy. A reset GameObject is not the same eligible target generation. This context belongs to the accepted action; it does not need to be stuffed into the shared authoring asset.

The current no-hit MissRecovery uses any accepted contact. The future retaliation drawback is stricter: hitting a bystander does not prove that the designated opponent was punished. Record the designated applied outcome separately, then calculate that action's recovery. Source reset, defeat and removal need documented handling without dereferencing destroyed objects.

Snapshot and context stay unchanged through an action. Target outcome can be accumulated by the detector, but it cannot rewrite windup or damage mid-swing. Compatibility rules determine which effects may be committed together before execution begins.

## Discovery evaluation and retained history

Use one authored rule first, with named prerequisites and explanation codes. Feed it verified receipts and character state. Return no-change, foreshadowed or revealed plus the relevant cause. It never deducts currency or changes combat configuration.

Deduplicate by meaningful evidence key, not UI notification timestamp. Keep transient pending chains separate from retained contributions. A delayed valid receipt can update knowledge, but must not retroactively improve an already accepted attack.

Retain enough durable information for current later connections: aggregate guard-break history, distinct verified interaction counts and a short cause record for discoveries. Avoid promising that all future rules can be reconstructed from discarded raw telemetry. If a new rule needs a fact that was never recorded, specify a future-play route rather than inventing past evidence or permanently locking existing characters out.

Changing a discovery rule version must not quietly retract a legitimate reveal or duplicate its reward. Default proposal: preserve prior knowledge and explain new prerequisites for use; new evidence follows the new rule. Exact migration policy is content-specific and must be reviewed.

## Mission settlement and checkpoint recovery

Before durable mission rewards, define which progress commits during a run and which settles at extraction. If practice commits immediately, its encounter budget must commit in the same durable unit. If both wait for debrief, the failure policy must say what is lost. Do not combine immediate character gains with an in-memory-only anti-replay ledger.

For the first local implementation, use a single versioned save envelope for character state, the active mission attempt and its settlement keys, plus a recoverable prior copy. Validate and write a complete next state before replacement. Do not claim general crash-proof exactly-once behavior from two independently written JSON files.

Potential local recovery test: interrupt between objective completion and debrief, load the last valid envelope, and resume from its consistent state. The same settlement key applies at most once. A failed write should not prompt a second award under a new ID. Document the limits of the target filesystem/backup approach; this is local resilience, not protection against deliberate save editing.

The first slice need not save mid-combat. Permit saves at safe hub/debrief/checkpoint boundaries, restore a defined safe encounter state, and never restore half a sword swing. If a checkpoint resets enemies while retaining practice, preserve the budget spent before the checkpoint.

## Content validation before scale

When a small catalog exists, validate:

- IDs are unique; references resolve; display names are not keys.
- Prerequisite relationships have the intended AND/OR semantics and no unintended dependency cycles.
- Revealing, purchasing and configuring are different operations.
- A configured development is learned, eligible and compatible.
- Numerical fields are finite and within authored safety ranges; no zero-duration division or negative costs.
- Triggered effects have explicit source and once-only behavior; no unsupported recursive trigger chain.
- Each supported action has matching presentation/timing references and a truthful description.
- Mutually exclusive options cannot both contribute through separate resolver components.

Do not make invalid or incomplete content purchasable by returning defaults. A debug validator can report all errors; gameplay should present unavailable content with a reason or omit unfinished catalog entries intentionally.

## Save evolution and missing content

Load into temporary data, validate, migrate known versions, resolve content, then activate a complete character. Keep the original until successful. A future unsupported schema must not be overwritten by an older reader.

If a learned definition is missing, retain its saved record as unresolved and deactivate the effect with an explanation. Do not repurpose another asset with the same display name. Do not delete history or issue arbitrary refunds automatically. A reviewed migration can map an old stable ID to a successor or refund recorded costs.

If a prerequisite changes, preserve knowledge but re-evaluate legal configuration. Show exactly what became inactive and why. A content update should not stealthily make a saved character unplayable: shared fundamentals remain available, and safe fallback configuration is explicit.

Account knowledge loads separately from character power. A broken archive cannot erase character purchases; an archive entry cannot fill missing character evidence. New-character creation initializes a fresh character record even when the archive is populated.

## Future verification matrix

| Failure case | Expected behavior |
| --- | --- |
| Double-click purchase / replay command | One cost and one ownership change |
| Stale preview / insufficient balance / lost prerequisite | Reject without partial mutation |
| Refund parent / dependent effect active | Apply one explicit policy; no orphan active power |
| Disabled resolver / old scene | Authored action fallback, no progression dependency |
| Failed attack start with opportunity | No spend or consumption |
| Opportunity expires or target resets during proposal | No expired benefit; documented ordinary fallback or rejection |
| Toggle/equipment changes during running action | Existing snapshot/context remains stable |
| Same evidence delivered twice / F6 clear | No duplicate progress or unlocked budget |
| Save interrupted / corrupt latest copy | Recover a complete prior state with a visible explanation |
| Debrief replay / checkpoint load | Settlement not duplicated; retained practice matches spent budget |
| Renamed label / moved asset | Stable identity still resolves |
| Missing ID / future schema | Preserve original data, safe inactive/fallback behavior, no silent overwrite |
| Fresh Warrior with populated archive | Knowledge policy only; no inherited mastery, purchases or character evidence |

These are future tests. No schema, save system, transaction layer, mission owner or composition framework has been implemented in this design pass. Do not add all of them at once.
