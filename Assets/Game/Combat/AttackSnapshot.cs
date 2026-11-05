using UnityEngine;

namespace EvolvingRPG.Combat
{
    // Values committed to one attack. Never use the mutable authoring asset to execute that attack.
    public readonly struct AttackSnapshot
    {
        public readonly string DisplayName;
        public readonly float Windup, ActiveTime, Recovery;
        public readonly float Damage, PoiseDamage, StaminaCost;
        public readonly float Range, Arc, MovementMultiplier;
        public readonly float AdvanceDistance, MissRecovery;
        public readonly Color Color;
        public float Duration => Windup + ActiveTime + Recovery;
        public AttackSnapshot WithRecovery(float recovery) => new AttackSnapshot(this, recovery);
        public AttackSnapshot WithAdvance(float distance, string name) => new AttackSnapshot(this, Recovery, distance, name);

        private AttackSnapshot(AttackSnapshot source, float recovery, float? advance = null, string name = null)
        {
            DisplayName = name ?? source.DisplayName; Windup = source.Windup; ActiveTime = source.ActiveTime;
            Recovery = Mathf.Max(0.01f, recovery); Damage = source.Damage; PoiseDamage = source.PoiseDamage;
            StaminaCost = source.StaminaCost; Range = source.Range; Arc = source.Arc;
            MovementMultiplier = source.MovementMultiplier; AdvanceDistance = Mathf.Max(0, advance ?? source.AdvanceDistance);
            MissRecovery = source.MissRecovery; Color = source.Color;
        }

        public AttackSnapshot(AttackDefinition definition)
        {
            DisplayName = definition.DisplayName;
            Windup = definition.Windup;
            ActiveTime = definition.ActiveTime;
            Recovery = definition.Recovery;
            Damage = definition.Damage;
            PoiseDamage = definition.PoiseDamage;
            StaminaCost = definition.StaminaCost;
            Range = definition.Range;
            Arc = definition.Arc;
            MovementMultiplier = definition.MovementMultiplier;
            AdvanceDistance = definition.AdvanceDistance;
            MissRecovery = definition.MissRecovery;
            Color = definition.Color;
        }
    }
}
