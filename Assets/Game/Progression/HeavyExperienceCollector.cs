using System;
using System.Collections.Generic;
using EvolvingRPG.Combat;

namespace EvolvingRPG.Progression
{
    // Reads authoritative receipts. Never resolves attacks, grants resources or reveals talents.
    public sealed class HeavyExperienceCollector : IDisposable
    {
        readonly CombatActions actions;
        readonly CombatVitality vitality;
        readonly Func<CharacterDevelopmentState> current;
        readonly Func<AttackDefinition, string> profileFor;
        readonly List<CombatObservation> candidates = new List<CombatObservation>(16);
        // Presentation only; never used for eligibility, credit or persistence.
        readonly Queue<(int Sequence, string Explanation)> review = new Queue<(int, string)>();
        public int ReviewCount => review.Count;
        public string ExplanationFor(int sequence)
        {
            foreach (var entry in review) if (entry.Sequence == sequence) return entry.Explanation;
            return null;
        }
        CharacterDevelopmentState acceptedCharacter;
        string profile;
        bool attached;
        int pending = -1, lastAccepted = -1, build;
        string rejection;
        public string Latest { get; private set; } = "Heavy experience: finish a live Heavy impact. History only; no bonus or points.";
        public HeavyExperienceCollector(CombatActions actions, CombatVitality vitality, Func<CharacterDevelopmentState> state, Func<AttackDefinition, string> profiles)
        { this.actions = actions; this.vitality = vitality; current = state; profileFor = profiles; }
        public void Attach()
        {
            if (attached) return;
            actions.Observed += Observe; vitality.ResetPerformed += ResetPending; attached = true;
        }
        public void Dispose()
        {
            if (attached) { actions.Observed -= Observe; vitality.ResetPerformed -= ResetPending; }
            attached = false; Discard();
        }
        void Discard() { pending = -1; candidates.Clear(); acceptedCharacter = null; }
        void ResetPending() { Discard(); Latest = "Encounter reset: completed Heavy experience retained; pending evidence discarded."; }
        public void ReplaceCharacter() { Discard(); review.Clear(); Latest = "Character experience loaded or fresh; no pending combat evidence restored."; }
        void Finish(string explanation)
        {
            Latest = explanation;
            if (review.Count == 6) review.Dequeue();
            review.Enqueue((pending, explanation));
            Discard();
        }
        void Observe(CombatObservation value)
        {
            if (value.Kind == CombatObservationKind.Accepted)
            {
                if (value.ActionSequence <= lastAccepted) return;
                lastAccepted = value.ActionSequence; Discard();
                if (value.ActionKind != CombatActionKind.Attack) return;
                profile = profileFor(value.Definition);
                if (profile == null) return; // Light and unlisted definitions cannot become Heavy practice.
                acceptedCharacter = current();
                if (!acceptedCharacter.Owns(profile) || acceptedCharacter.HasUnresolvedContent) { Discard(); return; }
                pending = value.ActionSequence; build = acceptedCharacter.BuildRevision;
                rejection = "no qualifying applied enemy contact";
                Latest = "Heavy experience pending: finish the action's recovery.";
                return;
            }
            if (pending < 0 || value.ActionSequence != pending) return;
            if (value.Kind == CombatObservationKind.Interrupted)
            { Finish("Heavy experience not recorded: interrupted before recovery completed."); return; }
            if (value.Kind == CombatObservationKind.HitResolved)
            {
                string reason = Reject(value);
                if (reason != null) rejection = reason;
                else if (candidates.Count < 16) candidates.Add(value);
                return;
            }
            if (value.Kind != CombatObservationKind.Completed) return;
            if (!ReferenceEquals(current(), acceptedCharacter))
            { Latest = "Heavy experience not recorded: character changed."; Discard(); return; }
            if (current().BuildRevision != build)
            { Finish("Heavy experience not recorded: build changed."); return; }
            var facts = new HashSet<string>(StringComparer.Ordinal);
            foreach (var candidate in candidates)
            {
                string reason = Reject(candidate);
                if (reason != null) { rejection = reason; continue; }
                facts.Add(HeavyExperienceCatalog.Id(profile, HeavyExperienceCatalog.Contact));
                if (candidate.Resolution.Guarded && candidate.Resolution.PoiseBroken)
                    facts.Add(HeavyExperienceCatalog.Id(profile, HeavyExperienceCatalog.GuardBreak));
                if (candidate.Resolution.PoiseBroken && candidate.Resolution.EnemyContext.Value.PhaseBeforeHit == EnemyPhase.Windup)
                    facts.Add(HeavyExperienceCatalog.Id(profile, HeavyExperienceCatalog.Interception));
            }
            var newLabels = new List<string>(3);
            foreach (string outcome in HeavyExperienceCatalog.Outcomes)
                if (facts.Contains(HeavyExperienceCatalog.Id(profile, outcome)) && !acceptedCharacter.HasExperience(profile, outcome))
                    newLabels.Add(HeavyExperienceCatalog.Label(outcome));
            int added = acceptedCharacter.RecordExperience(facts);
            Finish(facts.Count == 0 ? "Heavy experience not recorded: " + rejection + "."
                : added == 0 ? "Heavy experience already demonstrated for " + DevelopmentIds.Label(profile) + "; no repeated credit."
                : "Heavy experience: " + DevelopmentIds.Label(profile) + " demonstrated " + string.Join(", ", newLabels) + ". History only; no bonus or points.");
        }
        string Reject(CombatObservation hit)
        {
            if (hit.Target == null) return "receiver was removed before recovery completed";
            if (!(hit.Target is EnemyVitality) || !hit.Resolution.EnemyContext.HasValue) return "training or non-enemy contact";
            var enemy = hit.Target.GetComponent<SandboxEnemy>(); var context = hit.Resolution.EnemyContext.Value;
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.EncounterRevision != context.EncounterRevision) return "opponent reset, removed or disabled";
            if (!context.EngagedWithSource || !enemy.EngagedWith(vitality)) return "opponent was not engaging this actor";
            if (context.PhaseBeforeHit != EnemyPhase.Approach && context.PhaseBeforeHit != EnemyPhase.Windup
                && context.PhaseBeforeHit != EnemyPhase.Active && context.PhaseBeforeHit != EnemyPhase.Recovery
                && context.PhaseBeforeHit != EnemyPhase.Staggered) return "opponent was idle or outside a live exchange";
            if (hit.Resolution.DamageApplied <= 0 && hit.Resolution.PoiseApplied <= 0) return "no applied impact";
            return null;
        }
    }
}
