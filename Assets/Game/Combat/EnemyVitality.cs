using UnityEngine;

namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(SandboxEnemy))]
    public sealed class EnemyVitality : CombatVitality
    {
        [SerializeField, Range(0, 1)] private float guardedDamageMultiplier = 0.2f;
        [SerializeField, Min(1)] private float openingDamageMultiplier = 1.25f;
        [SerializeField, Range(1, 180)] private float guardArc = 140f;

        public override bool ReceiveHit(HitData hit, out HitResolution resolution)
        {
            var enemy = GetComponent<SandboxEnemy>();
            bool guarded = enemy.CanGuard && hit.SourcePosition.HasValue
                && Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(hit.SourcePosition.Value - transform.position, Vector3.up)) <= guardArc * 0.5f;
            bool vulnerable = !guarded && (Staggered || enemy.Phase == EnemyPhase.Recovery);
            float multiplier = guarded ? guardedDamageMultiplier * (enemy.Bracing ? 0.5f : 1f)
                : vulnerable ? openingDamageMultiplier : 1f;
            var context = new EnemyImpactContext(enemy.Phase, enemy.EncounterRevision, enemy.AttacksStarted,
                enemy.LastAttackStartedAt, enemy.EngagedWith(hit.Source));
            return ApplyHit(hit, multiplier, guarded, vulnerable, out resolution, context);
        }
    }
}
