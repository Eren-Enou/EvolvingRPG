using UnityEngine;

namespace EvolvingRPG.Combat
{
    public readonly struct HitData
    {
        public readonly string AttackName;
        public readonly float Damage;
        public readonly float PoiseDamage;
        public readonly Color Color;
        public readonly Vector3? SourcePosition;
        public readonly HitReceiver Source;

        public HitData(AttackDefinition attack) : this(new AttackSnapshot(attack)) { }

        public HitData(AttackSnapshot attack, Vector3? sourcePosition = null, HitReceiver source = null)
        {
            AttackName = attack.DisplayName;
            Damage = attack.Damage;
            PoiseDamage = attack.PoiseDamage;
            Color = attack.Color;
            SourcePosition = sourcePosition;
            Source = source;
        }

        public HitData(string name, float damage, float poiseDamage, Color color, Vector3 sourcePosition, HitReceiver source = null)
        { AttackName = name; Damage = damage; PoiseDamage = poiseDamage; Color = color; SourcePosition = sourcePosition; Source = source; }
    }
}
