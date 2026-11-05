namespace EvolvingRPG.Combat
{
    // Optional acceptance hook. Resolve remains a side-effect-free preview; this runs only
    // after the actor has passed readiness/grounding checks and successfully spent stamina.
    public interface IAttackAcceptanceResolver
    {
        AttackCommitment? Accepted(AttackDefinition definition, AttackSnapshot committed);
    }

    public readonly struct AttackCommitment
    {
        public readonly EnemyThreat Opportunity;
        public readonly float TargetMissRecovery;
        public AttackCommitment(EnemyThreat opportunity, float targetMissRecovery)
        { Opportunity = opportunity; TargetMissRecovery = targetMissRecovery; }
        public bool SameGeneration => Opportunity.Source != null && Opportunity.Source.EncounterRevision == Opportunity.Revision;
        public bool Matches(HitReceiver target, HitResolution resolution)
        {
            return SameGeneration && target != null && target.GetComponent<SandboxEnemy>() == Opportunity.Source
                && resolution.EnemyContext.HasValue && resolution.EnemyContext.Value.EncounterRevision == Opportunity.Revision
                && (resolution.DamageApplied > 0 || resolution.PoiseApplied > 0);
        }
    }
}
