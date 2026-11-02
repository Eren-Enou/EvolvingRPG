using UnityEngine;

namespace EvolvingRPG.Combat
{
    [CreateAssetMenu(menuName = "EvolvingRPG/Attack Definition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        public string DisplayName = "Light";
        [Min(0.01f)] public float Windup = 0.12f;
        [Min(0.01f)] public float ActiveTime = 0.1f;
        [Min(0.01f)] public float Recovery = 0.22f;
        [Min(0f)] public float Damage = 15f;
        [Min(0f)] public float PoiseDamage = 12f;
        [Min(0f)] public float StaminaCost = 8f;
        [Min(0.1f)] public float Range = 2.2f;
        [Range(1f, 180f)] public float Arc = 90f;
        [Range(0f, 1f)] public float MovementMultiplier = 0.35f;
        public Color Color = new Color(0.2f, 0.85f, 1f);
        public float Duration => Windup + ActiveTime + Recovery;

        private void OnValidate()
        {
            Windup = Mathf.Max(0.01f, Windup);
            ActiveTime = Mathf.Max(0.01f, ActiveTime);
            Recovery = Mathf.Max(0.01f, Recovery);
            Damage = Mathf.Max(0f, Damage);
            PoiseDamage = Mathf.Max(0f, PoiseDamage);
            StaminaCost = Mathf.Max(0f, StaminaCost);
            Range = Mathf.Max(0.1f, Range);
        }
    }
}
