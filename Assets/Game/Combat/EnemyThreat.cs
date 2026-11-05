using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    public enum ThreatEnd { Completed, Interrupted, Reset, Disabled, TargetChanged }

    // Encounter-local identity; never a character-save or content ID.
    public readonly struct EnemyThreat : IEquatable<EnemyThreat>
    {
        public readonly SandboxEnemy Source;
        public readonly CombatVitality Target;
        public readonly int Revision, Sequence;
        public readonly float CommittedAt, ActiveAt, ActiveUntil;
        public readonly StrikeGeometry Geometry;
        public readonly string Label;
        private readonly int hash;

        public EnemyThreat(SandboxEnemy source, CombatVitality target, int revision, int sequence,
            float committedAt, float windup, float active, StrikeGeometry geometry)
        {
            Source = source; Target = target; Revision = revision; Sequence = sequence;
            CommittedAt = committedAt; ActiveAt = committedAt + windup; ActiveUntil = ActiveAt + active;
            Geometry = geometry;
            hash = (ReferenceEquals(source, null) ? 0 : RuntimeHelpers.GetHashCode(source))
                ^ (ReferenceEquals(target, null) ? 0 : RuntimeHelpers.GetHashCode(target)) ^ revision * 397 ^ sequence;
            Label = (source != null ? source.name : "Fixture") + $" / encounter {revision} / strike {sequence}";
        }

        public bool Equals(EnemyThreat other) => ReferenceEquals(Source, other.Source)
            && ReferenceEquals(Target, other.Target) && Revision == other.Revision && Sequence == other.Sequence;
        public override bool Equals(object obj) => obj is EnemyThreat other && Equals(other);
        public override int GetHashCode() => hash;
    }

    public readonly struct ThreatResolution
    {
        public readonly EnemyThreat Threat;
        public readonly ThreatEnd End;
        public readonly float Time;
        public readonly bool Connected;
        public ThreatResolution(EnemyThreat threat, ThreatEnd end, float time, bool connected)
        { Threat = threat; End = end; Time = time; Connected = connected; }
    }
}
