using UnityEngine;

namespace EvolvingRPG.Combat
{
    public enum CombatObservationKind { Accepted, HitResolved, Completed, Interrupted, ImpactReceived }
    public enum CombatActionKind { Attack, Dodge }

    // Session-local evidence, not saved character progress or a mastery/reward decision.
    public readonly struct CombatObservation
    {
        public readonly int ActionSequence;
        public readonly CombatObservationKind Kind;
        public readonly CombatActionKind ActionKind;
        public readonly AttackDefinition Definition;
        public readonly AttackSnapshot? Attack;
        public readonly HitReceiver Target;
        public readonly float Time;
        public readonly HitResolution Resolution;
        public readonly int TargetsHit;
        public readonly MeleeMissReason MissReason;
        public readonly CombatPhase Phase;

        public CombatObservation(int sequence, CombatObservationKind kind, CombatActionKind actionKind,
            AttackDefinition definition, HitReceiver target, HitResolution resolution, int targetsHit,
            AttackSnapshot? attack = null, MeleeMissReason missReason = MeleeMissReason.None, CombatPhase phase = CombatPhase.Ready)
        {
            ActionSequence = sequence; Kind = kind; ActionKind = actionKind;
            Definition = definition; Target = target; Resolution = resolution;
            Attack = attack ?? (definition != null ? new AttackSnapshot(definition) : (AttackSnapshot?)null);
            TargetsHit = targetsHit; Time = UnityEngine.Time.time;
            MissReason = missReason;
            Phase = phase;
        }

        public string Summary
        {
            get
            {
                string name = ActionKind == CombatActionKind.Dodge ? "Dodge" : Attack?.DisplayName ?? "Attack";
                if (Kind == CombatObservationKind.ImpactReceived)
                    return $"#{ActionSequence} {name}: received {Resolution.DamageApplied:0.#} HP / {Resolution.PoiseApplied:0.#} poise during {Phase}"
                        + (Resolution.PoiseBroken ? " | stagger" : "") + (Resolution.Defeated ? " | defeated" : "");
                if (Kind == CombatObservationKind.HitResolved)
                    return $"#{ActionSequence} {name}: {(Target is TrainingTarget ? "training hit" : "hit")}, {Resolution.DamageApplied:0.#} HP / {Resolution.PoiseApplied:0.#} poise"
                        + (Resolution.Guarded ? " | guarded" : "") + (Resolution.Vulnerable ? " | opening" : "")
                        + (Resolution.PoiseBroken ? " | poise break" : "");
                if (Kind == CombatObservationKind.Completed)
                    return $"#{ActionSequence} {name}: completed"
                        + (ActionKind == CombatActionKind.Dodge ? " (evasion unverified)" : TargetsHit == 0
                            ? $" (miss: {MeleeMissFeedback.Describe(MissReason)})" : $" ({TargetsHit} target hit)");
                return $"#{ActionSequence} {name}: {Kind}";
            }
        }
    }

    public readonly struct HitResolution
    {
        public readonly float DamageApplied;
        public readonly float PoiseApplied;
        public readonly bool PoiseBroken;
        public readonly bool Defeated;
        public readonly bool Guarded, Vulnerable;
        public readonly EnemyImpactContext? EnemyContext;
        public HitResolution(float damage, float poise, bool broken, bool defeated, bool guarded = false, bool vulnerable = false,
            EnemyImpactContext? enemyContext = null)
        { DamageApplied = damage; PoiseApplied = poise; PoiseBroken = broken; Defeated = defeated; Guarded = guarded; Vulnerable = vulnerable; EnemyContext = enemyContext; }
    }
}
