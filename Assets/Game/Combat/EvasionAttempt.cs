using UnityEngine;

namespace EvolvingRPG.Combat
{
    public enum EvasionClassification { Verified, Rejected, Unverified }
    public enum EvasionReason
    {
        Verified, NoThreat, BeforeCommitment, StartingNotThreatened, IncomingHit, DodgeInterrupted,
        Obstructed, EnemyInterrupted, EnemyReset, OverlappingThreats, IncompleteSamples, PossibleContact,
        PlayerReset, UnsupportedGeometry, Capacity, ReusedThreat, AdditionalAction, UntrackedThreat
    }

    public readonly struct EvasionResult
    {
        public readonly int DodgeSequence, Samples;
        public readonly EnemyThreat? Threat;
        public readonly EvasionClassification Classification;
        public readonly EvasionReason Reason;
        public readonly StrikeCapsule StartingCapsule;
        public readonly Vector3 Direction, Displacement;
        public readonly float PathLength;
        public readonly bool DodgeCompleted, DodgeInterrupted;
        public EvasionResult(EvasionAttempt attempt, EvasionClassification classification, EvasionReason reason)
        {
            DodgeSequence = attempt.Sequence; Samples = attempt.Samples; Threat = attempt.Threat;
            Classification = classification; Reason = reason; StartingCapsule = attempt.Start;
            Direction = attempt.Direction;
            Displacement = (attempt.DodgeEnded ? attempt.DodgeEndCapsule : attempt.Last).Center - attempt.Start.Center;
            PathLength = attempt.DodgeEnded ? attempt.DodgePathLength : attempt.PathLength;
            DodgeCompleted = attempt.DodgeCompleted; DodgeInterrupted = attempt.DodgeInterrupted;
        }
        public string Explanation => EvasionAttempt.Explain(Reason);
    }

    // Deterministic reducer. Caller supplies actual post-motor poses and authoritative lifecycle receipts.
    // No rewards, attack changes, colliders, or global subscriptions.
    public sealed class EvasionAttempt
    {
        public readonly int Sequence;
        public readonly float AcceptedAt, ExpectedDistance, TravelDuration, MaximumGap, MaximumSpeed;
        public readonly Vector3 Direction;
        public readonly StrikeCapsule Start;
        public readonly EnemyThreat? Threat;
        public StrikeCapsule Last { get; private set; }
        public int Samples { get; private set; }
        public float PathLength { get; private set; }
        public bool DodgeEnded { get; private set; }
        public bool DodgeCompleted => DodgeEnded && !interrupted;
        public bool DodgeInterrupted => interrupted;
        public StrikeCapsule DodgeEndCapsule { get; private set; }
        public float DodgePathLength { get; private set; }
        public bool Finished { get; private set; }
        public bool Ready => DodgeEnded && (interrupted || !Threat.HasValue || resolution.HasValue || issue.HasValue);
        private ThreatResolution? resolution;
        private EvasionReason? issue;
        private bool hit, interrupted, blocked, threatened, possibleContact, activeSample;
        private float lastTime, projectedTravel;

        public EvasionAttempt(int sequence, float acceptedAt, float poseTime, StrikeCapsule start, Vector3 direction,
            float expectedDistance, float travelDuration, EnemyThreat? threat, float maximumGap = 0.025f, float maximumSpeed = 16f)
        {
            Sequence = sequence; AcceptedAt = acceptedAt; Start = Last = start; Direction = direction.normalized;
            ExpectedDistance = expectedDistance; TravelDuration = travelDuration; Threat = threat;
            MaximumGap = maximumGap; MaximumSpeed = maximumSpeed; lastTime = poseTime;
            if (!start.Supported || maximumGap <= 0 || maximumSpeed <= 0) issue = EvasionReason.UnsupportedGeometry;
            if (threat.HasValue && (acceptedAt < threat.Value.CommittedAt || acceptedAt > threat.Value.ActiveAt))
                issue = EvasionReason.IncompleteSamples;
        }

        public void Mark(EvasionReason reason) { if (!Finished && (!issue.HasValue || reason == EvasionReason.PlayerReset)) issue = reason; }
        public void IncomingHit() { if (!Finished) hit = true; }
        public void EndDodge(bool completed, StrikeCapsule? endPose = null)
        {
            if (Finished || DodgeEnded) return;
            DodgeEnded = true; interrupted = !completed; DodgeEndCapsule = endPose ?? Last;
            DodgePathLength = PathLength + (DodgeEndCapsule.Center - Last.Center).magnitude;
        }
        public void Resolve(ThreatResolution ended)
        {
            if (Finished || !Threat.HasValue || !Threat.Value.Equals(ended.Threat) || resolution.HasValue) return;
            resolution = ended; hit |= ended.Connected;
        }

        public void Sample(float now, StrikeCapsule pose, bool collidedSide, Vector3 sourceOrigin,
            bool startingPositionThreatened, bool actualContact, bool ambiguousGeometry = false)
        {
            if (Finished) return;
            float gap = now - lastTime;
            float distance = (pose.Center - Last.Center).magnitude;
            Samples++; PathLength += distance; blocked |= collidedSide && !DodgeEnded;
            if (!Start.SameShape(pose) || ambiguousGeometry) Mark(EvasionReason.UnsupportedGeometry);
            if (gap < 0 || gap > MaximumGap + 0.0001f || distance > MaximumSpeed * Mathf.Max(gap, 0) + 0.001f)
                Mark(EvasionReason.IncompleteSamples);
            if (now <= AcceptedAt + TravelDuration + MaximumGap)
                projectedTravel = Mathf.Max(projectedTravel, Vector3.Dot(pose.Center - Start.Center, Direction));
            if (Threat.HasValue)
            {
                var threat = Threat.Value;
                if ((sourceOrigin - threat.Geometry.Origin).magnitude > 0.02f) Mark(EvasionReason.UnsupportedGeometry);
                float end = resolution.HasValue ? Mathf.Max(threat.ActiveUntil, resolution.Value.Time) : threat.ActiveUntil;
                if (now >= threat.ActiveAt && lastTime <= end)
                {
                    activeSample = true; threatened |= startingPositionThreatened;
                    possibleContact |= actualContact || !StrikeContactQuery.ProvesClear(threat.Geometry, Last, MaximumSpeed * Mathf.Max(0, gap));
                }
            }
            Last = pose; lastTime = now;
        }

        public EvasionResult Finish()
        {
            Finished = true;
            if (hit) return Result(EvasionClassification.Rejected, EvasionReason.IncomingHit);
            if (issue == EvasionReason.PlayerReset || issue == EvasionReason.AdditionalAction) return Result(EvasionClassification.Unverified, issue.Value);
            if (interrupted) return Result(EvasionClassification.Rejected, EvasionReason.DodgeInterrupted);
            if (resolution.HasValue && resolution.Value.End != ThreatEnd.Completed)
                return Result(EvasionClassification.Unverified, resolution.Value.End == ThreatEnd.Reset ? EvasionReason.EnemyReset : EvasionReason.EnemyInterrupted);
            if (issue.HasValue) return Result(issue == EvasionReason.BeforeCommitment ? EvasionClassification.Rejected : EvasionClassification.Unverified, issue.Value);
            if (!DodgeCompleted) return Result(EvasionClassification.Unverified, EvasionReason.IncompleteSamples);
            if (!Threat.HasValue) return Result(EvasionClassification.Rejected, EvasionReason.NoThreat);
            if (!resolution.HasValue || !activeSample || Samples < 2 || lastTime + 0.0001f < resolution.Value.Time
                || resolution.Value.Time + 0.002f < Threat.Value.ActiveUntil)
                return Result(EvasionClassification.Unverified, EvasionReason.IncompleteSamples);
            if (!threatened) return Result(EvasionClassification.Rejected, EvasionReason.StartingNotThreatened);
            if (blocked || projectedTravel < Mathf.Max(0.1f, ExpectedDistance * 0.8f))
                return Result(EvasionClassification.Rejected, EvasionReason.Obstructed);
            if (possibleContact) return Result(EvasionClassification.Unverified, EvasionReason.PossibleContact);
            return Result(EvasionClassification.Verified, EvasionReason.Verified);
        }

        private EvasionResult Result(EvasionClassification classification, EvasionReason reason) => new EvasionResult(this, classification, reason);
        public static string Explain(EvasionReason reason)
        {
            switch (reason)
            {
                case EvasionReason.Verified: return "Committed strike avoided during dodge; starting position was threatened (stationary-position proxy).";
                case EvasionReason.NoThreat: return "No relevant committed enemy threat when dodge was accepted.";
                case EvasionReason.BeforeCommitment: return "Dodge began before the enemy committed; it cannot adopt that later strike.";
                case EvasionReason.StartingNotThreatened: return "The original capsule was not threatened, including line of sight.";
                case EvasionReason.IncomingHit: return "An incoming strike connected during the evidence interval.";
                case EvasionReason.DodgeInterrupted: return "The accepted dodge was interrupted before full recovery completed.";
                case EvasionReason.Obstructed: return "Side collision or less than 80% of intended travel; movement obstructed.";
                case EvasionReason.EnemyInterrupted: return "Enemy strike interrupted, disabled, or target changed before normal completion.";
                case EvasionReason.EnemyReset: return "Opponent reset invalidated the committed strike.";
                case EvasionReason.OverlappingThreats: return "Multiple committed threats make attribution ambiguous.";
                case EvasionReason.IncompleteSamples: return "Incomplete trajectory: late start, frame gap, motion bound, capacity, or timeout.";
                case EvasionReason.PossibleContact: return "The conservative swept enclosure could contact the strike; safety is unverified.";
                case EvasionReason.PlayerReset: return "Player reset invalidated this dodge.";
                case EvasionReason.UnsupportedGeometry: return "Unsupported capsule/source movement, shape change, or dynamic blocker.";
                case EvasionReason.Capacity: return "Bounded evidence capacity reached; reset opponents before continuing.";
                case EvasionReason.ReusedThreat: return "This actor already emitted a result for this enemy strike.";
                case EvasionReason.UntrackedThreat: return "An opponent resumed an attack without a live evidence lifecycle; wait for a fresh commitment.";
                default: return "Another action began before the evidence interval finished.";
            }
        }
    }
}
