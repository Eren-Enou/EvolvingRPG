using System.Collections.Generic;

namespace EvolvingRPG.Combat
{
    // Bounded, actor-owned diagnostics. No global bus, persistence, XP, or inferred combat style.
    public sealed class CombatObservationLog
    {
        public const int Capacity = 64;
        private readonly List<CombatObservation> records = new List<CombatObservation>();
        private readonly IReadOnlyList<CombatObservation> view;
        public IReadOnlyList<CombatObservation> Records => view;
        public int Accepted { get; private set; }
        public int Hits { get; private set; }
        public int Misses { get; private set; }
        public int PoiseBreaks { get; private set; }
        public int DodgesCompleted { get; private set; }
        public int Interrupted { get; private set; }
        public string Latest => records.Count == 0 ? "No observations" : records[records.Count - 1].Summary;

        public CombatObservationLog() => view = records.AsReadOnly();

        internal void Record(CombatObservation observation)
        {
            if (records.Count == Capacity) records.RemoveAt(0);
            records.Add(observation);
            if (observation.Kind == CombatObservationKind.Accepted) Accepted++;
            if (observation.Kind == CombatObservationKind.Interrupted) Interrupted++;
            if (observation.Kind == CombatObservationKind.HitResolved)
            {
                Hits++;
                if (observation.Resolution.PoiseBroken) PoiseBreaks++;
            }
            if (observation.Kind == CombatObservationKind.Completed)
            {
                if (observation.ActionKind == CombatActionKind.Dodge) DodgesCompleted++;
                else if (observation.TargetsHit == 0) Misses++;
            }
        }

        public void Clear()
        {
            records.Clear();
            Accepted = Hits = Misses = PoiseBreaks = DodgesCompleted = Interrupted = 0;
        }
    }
}
