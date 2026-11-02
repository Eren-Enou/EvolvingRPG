using UnityEngine;

namespace EvolvingRPG.Combat
{
    public readonly struct HitData
    {
        public readonly string AttackName;
        public readonly float Damage;
        public readonly float PoiseDamage;
        public readonly Color Color;

        public HitData(AttackDefinition attack)
        {
            AttackName = attack.DisplayName;
            Damage = attack.Damage;
            PoiseDamage = attack.PoiseDamage;
            Color = attack.Color;
        }
    }
}
