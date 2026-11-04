namespace EvolvingRPG.Combat
{
    // Captured before damage callbacks can change the enemy's phase or reset it.
    public readonly struct EnemyImpactContext
    {
        public readonly EnemyPhase PhaseBeforeHit;
        public readonly int EncounterRevision, ThreatSequence;
        public readonly float ThreatStartedAt;
        public readonly bool EngagedWithSource;
        public EnemyImpactContext(EnemyPhase phase, int revision, int threat, float startedAt, bool engaged)
        { PhaseBeforeHit = phase; EncounterRevision = revision; ThreatSequence = threat; ThreatStartedAt = startedAt; EngagedWithSource = engaged; }
    }
}
